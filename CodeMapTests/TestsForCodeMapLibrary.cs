using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using CodeMap;
using System.Threading.Tasks;
using CodeMap.Controllers;
using CodeMap.Models;

namespace CodeMapTests
{
    [TestClass]
    public class TestsForCodeMapLibrary
    {
        private readonly Config config = new Config()
        {
            AccessKey = "",
            SecretKey = "",
            Region = "eu-west-2",
            Bucket = "bucket-name",
            DynamorTable = "table-name",
            Token = "user-token-on-github",
            ServiceBase = "address-on-github",
            GitlabGroups = new List<string>(){"group-name"}
        };      
        private readonly List<string> addNewTestList = new List<string>() { "test","AProject", "BProject" };
        private readonly List<string> deleteOldTestList = new List<string>() { "AProject", "BProject" };

        private Library lib;
        private ChordMapController controller;

        [TestInitialize]
        public void SetUp()
        {
            lib = new Library(config);
            controller = new ChordMapController(config);
        }

        [TestMethod]
        public async Task MapUpdate_When_Project_Deployed_by_CI()
        {
            await lib.UpdateCmdAsync("git-group-name", "project-name");
        }

        [TestMethod]
        public void ChordMapController_RewriteDataSource_ForChordHtml_Test()
        {
            controller.GetAsync("group-name").GetAwaiter().GetResult();
        }

        [TestMethod]
        public async Task Populate_Empty_DB_TestAsync()
        {
            await lib.PopulateDynamoDBAsync();
        }

        [TestMethod]
        public async Task Print_All_Packages_TestAsync()
        {
            await lib.PrintAllPackagesAsync();
        }
    }
}
