using CodeMap.Models;
using GitLabApiClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeMapTests
{
    public partial class LibraryTests
    {
        private readonly static List<Node> Nodes = new List<Node>() { new Node() { id = "CProject" },
                                                                      new Node() { id = "AProject" } };
        private readonly static List<Link> Links = new List<Link>() { new Link() { source = "AProject", target = "CProject" } };
        private readonly Graph map = new Graph() { nodes = Nodes, links = Links };

        [TestMethod]
        public async Task GivenLinks_CanPutToTable_IntegrationTest()
        {
            await lib.AddNewRecordsAsync(sourceInReference, "CProject", sourceNewlyPushed);
        }

        [TestMethod]
        public async Task GinvenLinks_CanDeleteFromTable_IntegrationTest()
        {
            await lib.DeleteBrokenRecordsAsync(sourceInReference, sourceNewlyPushed);
        }

        [TestMethod]
        public async Task GivenGroupAndProjectName_GetDependencyFromGitlab()
        {
            string thisProjectName = "CodeMap";
            string thisGroup = "group-name";
            var result = await lib.GetDependencyFromCsprojOnGitlab(thisGroup, thisProjectName);
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public async Task GivenGroupName_GenerateForceMap_StoreInLocalDataset()
        {
            var map = new Graph();
            var client = new GitLabClient(config.ServiceBase, config.Token);

            await lib.GetOneGroupMapAsync("group-name", map, client);
            // as long as no exception thrown, test past
        }

        [TestMethod]
        public async Task GivenMap_ConvertToDB_IntegrationTest()
        {
            await lib.MapToDynamoTableAsync(map);
        }
    }
}
