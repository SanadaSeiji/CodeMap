using CodeMap;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using CodeMap.Models;

namespace CodeMapTests
{
    [TestClass]
    public partial class LibraryTests
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
            GitlabGroups = new List<string>() { "group-name" }
        };
        private readonly List<string> addNewTestList = new List<string>() { "test", "AProject", "BProject" };
        private readonly List<string> deleteOldTestList = new List<string>() { "AProject", "BProject" };

        private readonly List<DBRecord> sourceInReference = new List<DBRecord>() {
                new DBRecord{Id="102-135-25", Source="AProject", Target="CProject" },
                new DBRecord{Id="4c334bd9-a1d1-4b2b-a123-5c69b23d255e", Source="BProject", Target="CProject" }
            };
        private readonly List<string> sourceNewlyPushed = new List<string>() { "AProject", "test" };

        private Library lib;

        [TestInitialize]
        public void SetUp()
        {
            lib = new Library(config);
        }

        [TestMethod]
        public void GetDependenciesNeedAdd__ByCompareRecordsInDbAndRecordsInCsproj_UnitTest()
        {
            var sourceNewlyPushed = new List<string>() { "BProject", "AProject", "test" };

            var result = lib.GetRecordsNeedToAdd(sourceInReference, sourceNewlyPushed);
            foreach (string s in result)
            {
                Console.WriteLine(s);
            }
            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual("test", result[0]);
        }

        [TestMethod]
        public void GetDependenciesNeedDelete_ByCompareRecordsInDbAndRecordsInCsproj_UnitTest()
        {
            var result = lib.GetRecordsNeedToDelete(sourceInReference, sourceNewlyPushed);
            foreach (string s in result.Select(obj => obj.Source))
            {
                Console.WriteLine(s);
            }
            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual("BProject", result[0].Source);
        }

        [TestMethod]
        public void IsNewPackage_GivenNewPackage_Test()
        {
            var p = new PackageReference()
            {
                Include = "test_new",
                Version = "V"
            };

            var searchResult = new List<string>();
            bool result = lib.IsNewPackage(searchResult, p);
            Assert.IsTrue(result);

            lib.AddPackages(searchResult, new List<PackageReference>() { p });
            bool secondTimeResult = lib.IsNewPackage(searchResult, p);
            Assert.IsFalse(secondTimeResult);
        }

        [TestMethod]
        public void CanAddPackages_GivenResultList_AndPackagesList()
        {
            var p = new PackageReference()
            {
                Include = "test_new",
                Version = "V"
            };

            var searchResult = new List<string>();

            lib.AddPackages(searchResult, new List<PackageReference>() { p });

            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual("test_new", searchResult[0]);
        }
    }
}
