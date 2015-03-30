using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework.Generators;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Resources
{
    #region EntryValueType enum
    /// <summary>
    /// Valid types of entry values within a table
    /// </summary>
    internal enum EntryValueType
    {
        /// <summary>Entry has a value of an boolean (bool) type</summary>
        Boolean,
        /// <summary>Entry has a value of an integer (int) type</summary>
        Integer,
        /// <summary>Entry has a value of a floating point (float) type</summary>
        Float,
        /// <summary>Entry has a value of a string type</summary>
        String,
        /// <summary>Entry has a value of a color type</summary>
        Color,
    }
    #endregion

    /// <summary>
    /// Manages the definition of resource tables for TiVE. These definitions are used for validation on loading of the table data.
    /// </summary>
    internal sealed class ResourceTableDefinitionManager : IDisposable
    {
        #region Member variables
        private readonly Dictionary<string, TableDefinition> tableDefinitions = new Dictionary<string, TableDefinition>();
        #endregion

        #region Properties
        /// <summary>
        /// Gets a list of all the table definitions in the manager
        /// </summary>
        public IEnumerable<TableDefinition> Definitions 
        {
            get { return tableDefinitions.Values; }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Loads any table definition files
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public bool Initialize()
        {
            Messages.Print("Loading table definitions...");

            // Table definition that ships with TiVE should always be valid since it is embedded
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (StreamReader stream = new StreamReader(assembly.GetManifestResourceStream("ProdigalSoftware.TiVE.Resources.defaultTables.def")))
                ParseResourceDefinition(stream.ReadToEnd());

            ITableDefinitionProvider lastProvider = null;
            try
            {
                foreach (ITableDefinitionProvider provider in TiVEController.PluginManager.GetPluginsOfType<ITableDefinitionProvider>())
                {
                    lastProvider = provider;
                    ParseResourceDefinition(provider.GetTableDefinitionContents());
                }

                Messages.AddDoneText();
                return true;
            }
            catch (InvalidResourceDefinitionException e)
            {
                // Failed to parse the table definition contents
                Messages.AddFailText();
                if (lastProvider != null)
                    Messages.AddError(lastProvider.ToString());
                Messages.AddError(e.Message);
            }
            catch (Exception e)
            {
                // Some other error happened in the plugin so just spit out the error and exit
                Messages.AddFailText();
                if (lastProvider != null)
                    Messages.AddError(lastProvider.ToString());
                Messages.AddStackTrace(e);
            }
            return false;
        }

        public void Dispose()
        {
            tableDefinitions.Clear();
        }
        #endregion

        #region Methods for parsing definition file
        /// <summary>
        /// Parses the specified table resource definition file contents
        /// </summary>
        /// <exception cref="InvalidResourceDefinitionException">If something goes wrong with parsing the contents</exception>
        internal void ParseResourceDefinition(string contents)
        {
            using (StringReader reader = new StringReader(contents))
            {
                string currentTableName = "";
                List<EntryDefinition> currentValues = new List<EntryDefinition>();

                string line;
                int lineNum = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNum++;
                    line = line.Trim();
                    if (line.Length == 0)
                        continue; // Empty line

                    if (line[0] == '[' && line[line.Length - 1] == ']')
                    {
                        // Start of a new table definition
                        if (!string.IsNullOrEmpty(currentTableName))
                            AddTable(new TableDefinition(currentTableName, currentValues.ToArray()));

                        if (line.Length < 3)
                            throw new InvalidResourceDefinitionException("", lineNum, "Table name must not be empty");
                        
                        currentTableName = line.Substring(1, line.Length - 2).Trim();
                        if (currentTableName.IndexOf(' ') >= 0)
                            throw new InvalidResourceDefinitionException(currentTableName, lineNum, "Table name can not contain whitespace");

                        currentValues.Clear();
                    }
                    else
                    {
                        // Only other thing we allow in the file is a entry value definition
                        if (string.IsNullOrEmpty(currentTableName))
                            throw new InvalidResourceDefinitionException("", lineNum, "Resource table definitions must start with a table name of the form '[tableName]'");
                        currentValues.Add(GetDefinition(line, currentTableName, lineNum));
                    }
                }

                // Make sure we add the last table definition in the file contents
                if (!string.IsNullOrEmpty(currentTableName))
                    AddTable(new TableDefinition(currentTableName, currentValues.ToArray()));
            }
        }

        /// <summary>
        /// Gets an entry definition by parsing the specified line of the definition file
        /// </summary>
        /// <exception cref="InvalidResourceDefinitionException">If something goes wrong with parsing the line</exception>
        private static EntryDefinition GetDefinition(string line, string currentTableName, int lineNum)
        {
            int colonIndex = line.IndexOf(':');
            if (colonIndex == -1 || colonIndex == line.Length - 1)
                throw new InvalidResourceDefinitionException(currentTableName, lineNum, "Entry definition should be of the form 'name: type, required, defaultValue'");

            string valueName = line.Substring(0, colonIndex).Trim();
            if (string.IsNullOrEmpty(valueName))
                throw new InvalidResourceDefinitionException(currentTableName, lineNum, "Entry definition can not have an empty name");

            if (valueName.IndexOf(' ') >= 0)
                throw new InvalidResourceDefinitionException(currentTableName, lineNum, "Entry name can not contain spaces");

            string valueInfoStr = line.Substring(colonIndex + 1);
            string[] info = valueInfoStr.Split(new [] { ',' }, 3);
            if (info.Length != 3)
                throw new InvalidResourceDefinitionException(currentTableName, lineNum, "Entry definition should be of the form 'name: type, required, defaultValue'");

            EntryValueType valueType = ParseType(info[0], currentTableName, lineNum);
            bool required = ParseRequired(info[1], currentTableName, lineNum);
            object defaultValue = ParseDefaultValue(info[2], valueType, currentTableName, lineNum);

            return new EntryDefinition(valueName, valueType, required, defaultValue);
        }

        /// <summary>
        /// Parses the part of the entry definition that represents the entry type
        /// </summary>
        /// <exception cref="InvalidResourceDefinitionException">If something goes wrong with parsing the type string</exception>
        private static EntryValueType ParseType(string typeStr, string currentTableName, int lineNum)
        {
            typeStr = typeStr.Trim().ToLowerInvariant();
            if (typeStr == "b" || typeStr == "bool")
                typeStr = "boolean";
            else if (typeStr == "i" || typeStr == "int")
                typeStr = "integer";
            else if (typeStr == "f")
                typeStr = "float";
            else if (typeStr == "s" || typeStr == "str")
                typeStr = "string";
            else if (typeStr == "c")
                typeStr = "color";

            try
            {
                return (EntryValueType)Enum.Parse(typeof(EntryValueType), typeStr, true);
            }
            catch (ArgumentException)
            {
                throw new InvalidResourceDefinitionException(currentTableName, lineNum, 
                    "Type must be one of: [b,bool,boolean], [i,int,integer], [f,float], [s,str,string], [c,color]");
            }
        }

        /// <summary>
        /// Parses the part of the entry definition that represents whether the entry is required in the table
        /// </summary>
        /// <exception cref="InvalidResourceDefinitionException">If something goes wrong with parsing the required string</exception>
        private static bool ParseRequired(string requiredStr, string currentTableName, int lineNum)
        {
            requiredStr = requiredStr.Trim().ToLowerInvariant();
            if (requiredStr == "required" || requiredStr == "r")
                return true;

            if (!string.IsNullOrEmpty(requiredStr))
                throw new InvalidResourceDefinitionException(currentTableName, lineNum, "");

            return false;
        }

        /// <summary>
        /// Parses the part of the entry definition that represents the default value for the entry
        /// </summary>
        /// <exception cref="InvalidResourceDefinitionException">If something goes wrong with parsing the default value string</exception>
        private static object ParseDefaultValue(string defaultValueStr, EntryValueType valueType, string currentTableName, int lineNum)
        {
            defaultValueStr = defaultValueStr.Trim();
            switch (valueType)
            {
                case EntryValueType.Boolean:
                    bool boolValue;
                    if (!bool.TryParse(defaultValueStr, out boolValue))
                        throw new InvalidResourceDefinitionException(currentTableName, lineNum, "Invalid boolean: " + defaultValueStr);
                    return boolValue;

                case EntryValueType.Integer:
                    return ParseInt(defaultValueStr, currentTableName, lineNum);

                case EntryValueType.Float:
                    double floatValue;
                    if (!double.TryParse(defaultValueStr, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out floatValue))
                        throw new InvalidResourceDefinitionException(currentTableName, lineNum, "Invalid float: " + defaultValueStr);
                    return (float)floatValue;

                case EntryValueType.Color:
                    return ParseColor(defaultValueStr, currentTableName, lineNum);

                default:
                    return defaultValueStr;
            }
        }

        /// <summary>
        /// Parses a color string
        /// </summary>
        /// <exception cref="InvalidResourceDefinitionException">If something goes wrong with parsing the color string</exception>
        private static Color4b ParseColor(string colorStr, string currentTableName, int lineNum)
        {
            colorStr = colorStr.Trim();
            if (string.IsNullOrEmpty(colorStr) || colorStr.Length < 3)
                throw new InvalidResourceDefinitionException(currentTableName, lineNum, "Color value can not be empty");

            if (colorStr[0] != '(' || colorStr[colorStr.Length - 1] != ')')
                throw new InvalidResourceDefinitionException(currentTableName, lineNum, "Color value should be of the form '(red,green,blue)' or '(red,green,blue,alpha)");

            colorStr = colorStr.Substring(1, colorStr.Length - 2);
            string[] colorParts = colorStr.Split(',');
            if (colorParts.Length == 1)
            {
                // Color specified as a hex value in the form RRGGBB or RRGGBBAA
                string hexStr = colorParts[0].Trim();
                if (hexStr.Length == 6)
                    hexStr += "FF";
                if (hexStr.Length != 8)
                    throw new InvalidResourceDefinitionException(currentTableName, lineNum, "Invalid hex color value: " + hexStr);

                int value;
                if (!int.TryParse(hexStr, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out value))
                    throw new InvalidResourceDefinitionException(currentTableName, lineNum, "Invalid hex color value: " + hexStr);

                return new Color4b((byte)(((value >> 24) & 0xFF)), (byte)(((value >> 16) & 0xFF)), (byte)(((value >> 8) & 0xFF)), (byte)((value >> 0) & 0xFF));
            }
            
            if (colorParts.Length < 3 || colorParts.Length > 4)
                throw new InvalidResourceDefinitionException(currentTableName, lineNum, "Color value should be of the form '(red,green,blue)' or '(red,green,blue,alpha) or '(hexColor)'");

            // Color specified as 3 or 4 integers
            int red = ParseInt(colorParts[0], currentTableName, lineNum);
            int green = ParseInt(colorParts[1], currentTableName, lineNum);
            int blue = ParseInt(colorParts[2], currentTableName, lineNum);
            int alpha = colorParts.Length == 4 ? ParseInt(colorParts[3], currentTableName, lineNum) : 255;
            return new Color4b((byte)red, (byte)green, (byte)blue, (byte)alpha);
        }

        /// <summary>
        /// Attempts to parse the specified string as an integer. Handles hexadecimal numbers that start with '0x'
        /// </summary>
        /// <exception cref="InvalidResourceDefinitionException">If something goes wrong with parsing the int string</exception>
        private static int ParseInt(string intStr, string currentTableName, int lineNum)
        {
            int intValue;
            if (intStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(intStr.Substring(2), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out intValue))
                    throw new InvalidResourceDefinitionException(currentTableName, lineNum, "Invalid hexadecimal integer: " + intStr);
            }
            else if (!int.TryParse(intStr, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out intValue))
                throw new InvalidResourceDefinitionException(currentTableName, lineNum, "Invalid integer: " + intStr);

            return intValue;
        }
        #endregion

        #region Private helper methods
        /// <summary>
        /// Adds the specified table definition to the manager
        /// </summary>
        private void AddTable(TableDefinition table)
        {
            tableDefinitions.Add(table.Name, table);
        }
        #endregion
    }

    #region InvalidResourceDefinitionException class
    /// <summary>
    /// Exception thrown when reading the table definition file if something fails to parse
    /// </summary>
    internal sealed class InvalidResourceDefinitionException : TiVEException
    {
        /// <summary>
        /// Creates a new InvalidResourceDefinitionException with the specified information
        /// </summary>
        /// <param name="currentTable">The name of the table that is currently being parsed</param>
        /// <param name="lineNum">The line number (1-based) of the line that is currently being parsed</param>
        /// <param name="message">Error message describing the problem with the current line</param>
        public InvalidResourceDefinitionException(string currentTable, int lineNum, string message) :
            base(string.Format("Invalid definition in table: {0}\nLine number: {1}\nMessage: {2}", currentTable, lineNum, message))
        {
        }
    }
    #endregion

    #region TableDefinition class
    /// <summary>
    /// Represents the definition of a resource table and any valid entries
    /// </summary>
    internal sealed class TableDefinition
    {
        /// <summary>
        /// The name of the table for which this definition describes
        /// </summary>
        public readonly string Name;

        private readonly Dictionary<string, EntryDefinition> entries;

        /// <summary>
        /// Creates a new TableDefinition with the specified information
        /// </summary>
        /// <param name="name">The name of the table for which this definition describes</param>
        /// <param name="entries">List of valid entries in the table for which this definition describes</param>
        public TableDefinition(string name, IEnumerable<EntryDefinition> entries)
        {
            Name = name;
            this.entries = entries.ToDictionary(e => e.Name);
        }

        /// <summary>
        /// Gets a list of valid entries in the table for which this definition describes
        /// </summary>
        public IEnumerable<EntryDefinition> Entries
        {
            get { return entries.Values; }
        }
    }
    #endregion

    #region EntryDefinition class
    /// <summary>
    /// Represents the constraints for a single entry in a resource table
    /// </summary>
    internal sealed class EntryDefinition
    {
        /// <summary>The name of the entry in the table</summary>
        public readonly string Name;
        /// <summary>The value type of the entry in the table</summary>
        public readonly EntryValueType ValueType;
        /// <summary>Whether the entry is required in the table</summary>
        public readonly bool Required;
        /// <summary>The default value of the entry in the table</summary>
        public readonly object DefaultValue;

        /// <summary>
        /// Creates a new EntryDefinition with the specified constraints
        /// </summary>
        /// <param name="name">The name of the entry in the table</param>
        /// <param name="valueType">The value type of the entry in the table</param>
        /// <param name="required">Whether the entry is required in the table</param>
        /// <param name="defaultValue">The default value of the entry in the table</param>
        public EntryDefinition(string name, EntryValueType valueType, bool required, object defaultValue)
        {
            Name = name;
            ValueType = valueType;
            Required = required;
            
            bool validDefault;
            switch (valueType)
            {
                case EntryValueType.Boolean: validDefault = defaultValue is bool; break;
                case EntryValueType.Integer: validDefault = defaultValue is int; break;
                case EntryValueType.Float: validDefault = defaultValue is float; break;
                case EntryValueType.Color: validDefault = defaultValue is Color4b; break;
                default: validDefault = defaultValue is string; break;
            }
            if (!validDefault)
                throw new ArgumentException(string.Format("default value type {0} does not coorespond to the resource type {1}", defaultValue, valueType), "defaultValue");
            DefaultValue = defaultValue;
        }

        //public int DefaultInt
        //{
        //    get
        //    {
        //        if (ValueType != EntryValueType.Integer)
        //            throw new InvalidOperationException("Can not get a int value from resource type: " + ValueType);
        //        return (int)DefaultValue;
        //    }
        //}

        //public float DefaultFloat
        //{
        //    get
        //    {
        //        if (ValueType != EntryValueType.Float)
        //            throw new InvalidOperationException("Can not get a float value from resource type: " + ValueType);
        //        return (float)DefaultValue;
        //    }
        //}

        //public string DefaultString
        //{
        //    get
        //    {
        //        if (ValueType != EntryValueType.String)
        //            throw new InvalidOperationException("Can not get a string value from resource type: " + ValueType);
        //        return (string)DefaultValue;
        //    }
        //}

        //public Color4b DefaultColor
        //{
        //    get
        //    {
        //        if (ValueType != EntryValueType.Color)
        //            throw new InvalidOperationException("Can not get a color value from resource type: " + ValueType);
        //        return (Color4b)DefaultValue;
        //    }
        //}
    }
    #endregion
}
