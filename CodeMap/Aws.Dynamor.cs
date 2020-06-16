using Amazon.DynamoDBv2.Model;
using CodeMap.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeMap
{
    public partial class Aws
    {
        public async Task<List<DBRecord>> GetRecordsAsync(string filterColumn, string searchedNode)
        {
            Console.WriteLine($"GetItemAsync: search for {searchedNode} as {filterColumn}");

            var list = new List<DBRecord>();
            var request = new ScanRequest
            {
                TableName = tableName,
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                        {":val", new AttributeValue { S = searchedNode }}
                 },
                FilterExpression = filterColumn + "= :val",
                ProjectionExpression = "Id, SourceProject, TargetProject" // when delete, needs Id
            };
            var response = await dbClient.ScanAsync(request);

            Console.WriteLine($"Already Existed Dependency in DB: Scan result for project {searchedNode} as {filterColumn}");
           
            foreach (Dictionary<string, AttributeValue> item in response.Items)
            {             
                Console.WriteLine($"{item["Id"].S}  {item["SourceProject"].S} -> {item["TargetProject"].S}");
                list.Add(new DBRecord {
                    Id = item["Id"].S,
                    Source = item["SourceProject"].S,
                    Target = item["TargetProject"].S
                });
            }
            return list;
        }

        public async Task PutRecordAsync(string source, string target)
        {
            var guid = Guid.NewGuid();
            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = new Dictionary<string, AttributeValue>()
                  {
                      { "Id", new AttributeValue { S = guid.ToString() }},
                      { "SourceProject", new AttributeValue { S = source }},
                      { "TargetProject", new AttributeValue { S = target}}
                  }
            };
           await dbClient.PutItemAsync(request);
           Console.WriteLine($"inserted link: {source} -> {target}");
        }

        public async Task DeleteRecordAsync(string hashKey)
        {
            var request = new DeleteItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>()
                  {
                      { "Id", new AttributeValue { S = hashKey }} // provide only hash_key
                  }
            };
            await dbClient.DeleteItemAsync(request);
        }
    }
}
