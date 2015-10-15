using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Migrate_Nintex_Workflow
{
    public class Workflow
    {
        public string Name { get; set; }
        public string ListName { get; set; }
        public string Category { get; set; }
        public string SPWebURL { get; set; }
        public string LocalFilePath { get; set; }
        
        public Workflow(string name, string category, string listName, string spwebUrl)
        {
            Name = name;
            Category = category;
            SPWebURL = spwebUrl;
            if (category.ToLower() == "list")
                ListName = listName;
            else 
                ListName = null;
        }
        public string Serialize(string path)
        {
            string xml;
            var xmlDoc = new XmlDocument();
            var xmlSerializer = new XmlSerializer(GetType());
            using (var xmlStream = new MemoryStream())
            {
                xmlSerializer.Serialize(xmlStream, this);
                xmlStream.Position = 0;
                xmlDoc.Load(xmlStream);
                xml = xmlDoc.InnerXml;
            }
            if (string.IsNullOrEmpty(xml)) return string.Empty;
            var directory = Directory.GetParent(path);
            if (!Directory.Exists(directory.FullName))
            {
                Directory.CreateDirectory(directory.FullName);
            }
            var filename = string.Concat(Path.GetFileNameWithoutExtension(path), "_metadata.xml");
            var fullName = Path.Combine(directory.FullName, filename);
            xmlDoc.Save(fullName);
            return fullName;
        }

        public static Workflow Deserialize(string file)
        {
            var serializer = new XmlSerializer(typeof(Workflow));
            using (var fileStream = new FileStream(file, FileMode.Open))
            {
                var xmlReader = XmlReader.Create(fileStream);
                var workFlowNode = (Workflow)serializer.Deserialize(xmlReader);
                fileStream.Close();
                return workFlowNode;
            }
        }
    }
}
