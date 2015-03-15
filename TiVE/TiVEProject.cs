﻿using System.IO;
using System.Xml.Serialization;

namespace ProdigalSoftware.TiVE
{
    /// <summary>
    /// Information about a TiVE project loaded from an XML file
    /// </summary>
    [XmlRoot("Game")]
    public sealed class TiVEProject
    {
        /// <summary>
        /// The name of the project to show to the user
        /// </summary>
        [XmlElement("Name")]
        public string Name;
        
        /// <summary>
        /// Name of the author of the project
        /// </summary>
        [XmlElement("Author")]
        public string Author;

        /// <summary>
        /// Copyright information about the project
        /// </summary>
        [XmlElement("Copyright")]
        public string Copyright;

        /// <summary>
        /// Name of the script to run for the project
        /// </summary>
        [XmlElement("Start")]
        public string StartScript;

        /// <summary>
        /// Creates a new TiVEProject from the specified XML file
        /// </summary>
        public static TiVEProject FromFile(string filePath)
        {
            XmlSerializer ser = new XmlSerializer(typeof(TiVEProject), "");
            return (TiVEProject)ser.Deserialize(new StreamReader(filePath));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
