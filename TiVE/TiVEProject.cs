using System.IO;
using System.Xml.Serialization;

namespace ProdigalSoftware.TiVE
{
    [XmlRoot("Game")]
    public sealed class TiVEProject
    {
        [XmlElement("Name")]
        public string Name;
        
        [XmlElement("Author")]
        public string Author;

        [XmlElement("Copyright")]
        public string Copyright;

        [XmlElement("Start")]
        public string StartScript;

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
