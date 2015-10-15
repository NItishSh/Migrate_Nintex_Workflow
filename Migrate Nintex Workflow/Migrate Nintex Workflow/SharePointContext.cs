using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrate_Nintex_Workflow
{
    public class SharePointContext
    {
        public List<Workflow> GetNintexWorkflowsfromSharePoint(string siteurl)
        {
            var clientContext = new ClientContext(siteurl);
            var spList = clientContext.Web.Lists.GetByTitle("NintexWorkflows");
            clientContext.Load(spList);
            clientContext.ExecuteQuery();
            var allNintexWFs = new List<Workflow>();
            if (spList != null && spList.ItemCount > 0)
            {
                var camlQuery = new CamlQuery { ViewXml = @"<View Scope='RecursiveAll'>
                                        <Query>
                                            <Where><Eq><FieldRef Name='ContentType' /><Value Type='Computed'>Workflow</Value></Eq></Where>
                                        </Query>
                                        <ViewFields>
                                            <FieldRef Name='FileLeafRef' />
                                            <FieldRef Name='NintexWorkflowID' />
                                            <FieldRef Name='AssociatedListID' />
                                            <FieldRef Name='NWAssociatedWebID' />
                                            <FieldRef Name='WorkflowCategory' />
                                            <FieldRef Name='ContentType' />
                                            <FieldRef Name='Title' />
                                        </ViewFields> 
                                    </View>" };

                var listItems = spList.GetItems(camlQuery);
                clientContext.Load(listItems);
                clientContext.ExecuteQuery();
                foreach (var wfItem in listItems)
                {
                    var fileName = Convert.ToString(wfItem["FileLeafRef"]);
                    if (fileName.EndsWith(".xoml"))
                    {
                        var wfName = fileName.Replace(".xoml", "");
                        var wfCategory = Convert.ToString(wfItem["WorkflowCategory"]);
                        var associatedListId = Convert.ToString(wfItem["AssociatedListID"]).Trim('{', '}');
                        var associatedWebId = Convert.ToString(wfItem["NWAssociatedWebID"]);
                        var listTitle = "";
                        listTitle = GetListName(associatedWebId, associatedListId, clientContext,
                            listTitle);
                        allNintexWFs.Add(new Workflow(wfName, wfCategory, listTitle, associatedWebId));
                    }
                }
            }
            return allNintexWFs;
        }
        private string GetListName(string associatedWebId, string associatedListId, ClientContext clientContext,
            string listTitle)
        {
            if (!string.IsNullOrEmpty(associatedWebId) && !string.IsNullOrEmpty(associatedListId))
            {
                var webId = new Guid(associatedWebId);
                var oWebsite = clientContext.Site.OpenWebById(webId);
                var collList = oWebsite.Lists;
                var resultCollection = clientContext.LoadQuery(
                    collList.Include(
                        list => list.Title,
                        list => list.Id));
                clientContext.ExecuteQuery();
                foreach (var oList in resultCollection)
                {
                    if (associatedListId.Equals(Convert.ToString(oList.Id),
                        StringComparison.InvariantCultureIgnoreCase))
                        listTitle = oList.Title;
                }
            }
            return listTitle;
        }

        public bool IsFeatureActivated(string siteUrl, Guid featureId)
        {
            var featureActivated = false;
            using (var clientContext = new ClientContext(siteUrl))
            {
                var clientWeb = clientContext.Web;
                var webFeatures = clientWeb.Features;
                clientContext.Load(webFeatures);
                clientContext.ExecuteQuery();
                foreach (var feature in webFeatures)
                {
                    if (feature.DefinitionId.CompareTo(featureId) == 0)
                    {
                        featureActivated = true;
                    }
                }
            }

            return featureActivated;
        }
    }
}
