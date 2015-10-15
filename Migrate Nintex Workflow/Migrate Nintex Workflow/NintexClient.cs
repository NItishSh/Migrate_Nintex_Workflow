using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Migrate_Nintex_Workflow.NintexWS;
using System.Net;
using System.IO;
using System.Xml.Linq;

namespace Migrate_Nintex_Workflow
{   
    public class NintexClient
    {
        private const string NintexServiceUrl = "/_vti_bin/NintexWorkflow/Workflow.asmx";
        private static readonly Guid NintexWorkflowFeatureId = new Guid("9bf7bf98-5660-498a-9399-bc656a61ed5d");
        private readonly string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

        public SharePointContext SpContext { get; set; }
        public NintexClient()
        {
            SpContext = new SharePointContext();
        }

        public void Import(string url, string workflowFile, string metadatafile)
        {
            var nintexService = new NintexWorkflowWS
            {
                Url = string.Concat(url, NintexServiceUrl),
                Credentials = CredentialCache.DefaultCredentials
            };

            if (!SpContext.IsFeatureActivated(url, NintexWorkflowFeatureId))
            {
                throw new Exception(string.Format("Web feature \"Nintex Workflow 2010\" is not activated for the site {0}. Please activate and try again.", url));
            }
            var workflow = Workflow.Deserialize(metadatafile);
            var workflowBytes = File.ReadAllBytes(workflowFile);
            nintexService.PublishFromNWF(workflowBytes,
                    workflow.Category.ToLower().Equals("list") ? workflow.ListName : null, workflow.Name,
                    true);
            System.Windows.MessageBox.Show("Successfully imported workflow");
        }

        public string Export(string url, Workflow workflow, string path)
        {
            var nintexService = new NintexWorkflowWS
            {
                Url = string.Concat(url, NintexServiceUrl),
                Credentials = CredentialCache.DefaultCredentials
            };

            if (!SpContext.IsFeatureActivated(url, NintexWorkflowFeatureId))
            {
                throw new Exception(string.Format("Web feature \"Nintex Workflow 2010\" is not activated for the site {0}. Please activate and try again.", url));
            }

            var workflowXml = nintexService.ExportWorkflow(workflow.Name, workflow.ListName, workflow.Category);
            var localFilePath = SaveWorkflowXmlToFile(workflow.Name, workflowXml, path);
            workflow.Serialize(localFilePath);
            return localFilePath;
        }
        private string SaveWorkflowXmlToFile(string name, string workflowXml, string path)
        {
            var fileName = string.Concat(name.Replace(" ", "_"), ".nwf");
            if (workflowXml.StartsWith(_byteOrderMarkUtf8))
            {
                workflowXml = workflowXml.Remove(0, _byteOrderMarkUtf8.Length);
            }
            var xmlDocument = XDocument.Parse(workflowXml);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var fullFileName = Path.Combine(path, fileName);
            xmlDocument.Save(fullFileName);
            return fullFileName;
        }
    }
}
