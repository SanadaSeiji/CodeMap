using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using CodeMap.Models;

namespace CodeMap
{
    public partial class Aws
    {

        private readonly string tableName;
        
        private readonly string bucketName;

        private readonly AmazonDynamoDBClient dbClient;
        private readonly IAmazonS3 s3Client;

        public Aws(Config config)
        { 
            var endpoint = RegionEndpoint.GetBySystemName(config.Region);

            dbClient = new AmazonDynamoDBClient(config.AccessKey, config.SecretKey, endpoint);
            s3Client = new AmazonS3Client(config.AccessKey, config.SecretKey, endpoint);

            tableName = config.DynamorTable;
            bucketName = config.Bucket;
        }
    }
}
