using Amazon.S3.Transfer;

namespace CodeMap
{
    public partial class Aws
    {                  
        public void DownloadFile(string filePath, string key)
        {
            using (var fileTransferUtility = new TransferUtility(s3Client))
                fileTransferUtility.Download(filePath, bucketName, key);
        }
    }
}
