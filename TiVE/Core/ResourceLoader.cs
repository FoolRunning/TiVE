using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Ionic.Zip;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Core
{
    /// <summary>
    /// Manages loading of resource files for TiVE
    /// </summary>
    internal sealed class ResourceLoader : IDisposable
    {
        #region Constants/member variables
        private const string DataDirName = "Data";
        private const string ResourcePackExtension = ".TiVEPack";

        private static readonly string dataDirPath;

        private readonly HashSet<string> looseFiles = new HashSet<string>();
        private readonly List<ZipFile> packFiles = new List<ZipFile>(5);
        private readonly Dictionary<string, ushort> filesInPack = new Dictionary<string, ushort>(1000);
        #endregion

        #region Constructor
        static ResourceLoader()
        {
            dataDirPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), DataDirName) + Path.DirectorySeparatorChar;
        }
        #endregion

        #region Implementation of IDisposable
        public void Dispose()
        {
            foreach (ZipFile zipFile in packFiles)
                zipFile.Dispose();
            
            looseFiles.Clear();
            filesInPack.Clear();
            packFiles.Clear();
        }
        #endregion

        #region Public methods
        public bool Initialize()
        {
            Messages.Print("Initializing resource loader...");

            if (looseFiles.Count > 0 || filesInPack.Count > 0)
                Dispose();

            try
            {
                foreach (string file in Directory.EnumerateFiles(dataDirPath, "*", SearchOption.AllDirectories).Where(f => Path.GetExtension(f) != ResourcePackExtension))
                    looseFiles.Add(MakeRelativeToDataDir(file));

                foreach (string packFile in Directory.EnumerateFiles(dataDirPath, "*" + ResourcePackExtension))
                {
                    ushort packIndex = (ushort)packFiles.Count;
                    ZipFile zipFile = ZipFile.Read(packFile);
                    packFiles.Add(zipFile);

                    foreach (ZipEntry entry in zipFile.Where(entry => !entry.IsDirectory))
                        filesInPack[entry.FileName.Replace('/', Path.DirectorySeparatorChar)] = packIndex;
                }
            }
            catch (Exception e)
            {
                Messages.AddFailText();
                Messages.AddStackTrace(e);
                return false;
            }

            Messages.AddDoneText();
            return true;
        }

        public IEnumerable<string> Files(string relFolder, string searchStr)
        {
            Regex searchRegex = new Regex(Regex.Escape(searchStr).Replace("\\*", ".+"));

            if (!string.IsNullOrEmpty(relFolder) && !relFolder.EndsWith(Path.DirectorySeparatorChar) && !relFolder.EndsWith(Path.AltDirectorySeparatorChar))
                relFolder += Path.DirectorySeparatorChar;

            foreach (string relFilePath in looseFiles.Concat(filesInPack.Keys))
            {
                if ((string.IsNullOrEmpty(relFolder) || relFilePath.StartsWith(relFolder, StringComparison.Ordinal)) &&
                    searchRegex.IsMatch(Path.GetFileName(relFilePath)))
                {
                    yield return relFilePath;
                }
            }
        }

        public bool FileExists(string relFilePath)
        {
            return looseFiles.Contains(relFilePath) || filesInPack.ContainsKey(relFilePath);
        }

        /// <summary>
        /// Opens the specified file for reading. Path is relative to the Data directory.
        /// <para>WARNING: Stream must be disposed of by caller.</para>
        /// </summary>
        public Stream OpenFile(string relFilePath)
        {
            // Loose files have priority over files inside a pack
            if (looseFiles.Contains(relFilePath))
            {
                string fullPath = Path.Combine(dataDirPath, relFilePath);
                if (File.Exists(fullPath))
                    return new FileStream(fullPath, FileMode.Open);
            }

            ushort packIndex;
            if (!filesInPack.TryGetValue(relFilePath, out packIndex))
                throw new FileNotFoundException("File " + relFilePath + " was not found in resources.", relFilePath);
            
            return packFiles[packIndex][relFilePath].OpenReader();
        }
        #endregion

        #region Private helper methods
        private static string MakeRelativeToDataDir(string fullPath)
        {
            Debug.Assert(fullPath.StartsWith(dataDirPath, StringComparison.Ordinal));
            return fullPath.Substring(dataDirPath.Length);
        }
        #endregion
    }
}
