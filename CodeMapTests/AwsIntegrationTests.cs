using CodeMap;
using CodeMap.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

using System.Threading.Tasks;

namespace CodeMapTests
{
    [TestClass]
    public class AwsIntegrationTest
    {        
        private readonly Aws aws = new Aws(new Config()
        {
            AccessKey = "",
            SecretKey = "",
            Region = "eu-west-2",
            Bucket = "bucket-name",
            DynamorTable = "table-name",
            Token = "git-user-token",
            ServiceBase = "git-address",
            GitlabGroups = new List<string>(){"gitGroupName"}
        });

        [TestMethod]
        public async Task GetLinks_GivenWantedNode_FromDynamoDb()
        {
            string filterColumn = "SourceProject"; // "Source/UUID/Target" are reserved by AWS unusable as filter
            string searchedNode = "test";

            var list = await aws.GetRecordsAsync(filterColumn, searchedNode);
            Console.WriteLine(list.Count);
        }

        [TestMethod]
        public async Task InsertLink_InDynamoDb()
        {
            string source = "source_project";
            string target = "target_project";

            await aws.PutRecordAsync(source, target);
        }

        [TestMethod]
        public async Task DeleteLink_GivenId_FromDynamoDb()
        {
            var obj = new DBRecord()
            {
                Id = "000",
                Source = "source_project",
                Target = "target_project"
            };
            await aws.DeleteRecordAsync(obj.Id);// when item does not exist, do not throw exception
        }

        [TestMethod]
        public void DownloadFile_Inlocalfile_FromS3()
        {
            string path = Helper.GetDatasetPath("projects.csv");
            string key = "test_projects.csv";
            aws.DownloadFile(path, key);
        }
    }
}
