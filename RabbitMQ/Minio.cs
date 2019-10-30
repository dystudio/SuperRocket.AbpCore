using System;
using Minio;
using Minio.Exceptions;
using Minio.DataModel;
using System.Threading.Tasks;

namespace RabbitMQ
{
    public class MinioUploader
    {
        public string BucketName { get; set; }
        public string Location { get; set; }
        public string ObjectName { get; set; }
        public string FileFullPath { get; set; }
        public string ContentType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="accessKey"></param>
        /// <param name="secretKey"></param>
        /// <param name="enableSSL"></param>
        public void UploadToMinio(string endpoint,
            string accessKey, 
            string secretKey, 
            bool enableSSL)
        {
            try
            {
                var minio = new MinioClient(endpoint, accessKey, secretKey);
                if (enableSSL)
                {
                    minio.WithSSL();
                }
                
                this.Run(minio).Wait();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="minio"></param>
        /// <returns></returns>
        private async Task Run(MinioClient minio)
        {
            //var bucketName = "mymusic";
            //var location = "us-east-1";
            //var objectName = "golden-oldies.zip";
            //var filePath = "C:\\Users\\username\\Downloads\\golden_oldies.mp3";
            //var contentType = "application/zip";
            var bucketName = this.BucketName;
            var location = this.Location;
            var objectName = this.ObjectName;
            var fileFullPath = this.FileFullPath;
            var contentType = this.ContentType;
            try
            {
                // Make a bucket on the server, if not already present.
                bool found = await minio.BucketExistsAsync(bucketName);
                if (!found)
                {
                    await minio.MakeBucketAsync(bucketName, location);
                }
                // Upload a file to bucket.
                await minio.PutObjectAsync(bucketName, objectName, fileFullPath, contentType);
                System.Console.WriteLine("Successfully uploaded " + objectName);
            }
            catch (MinioException e)
            {
                System.Console.WriteLine("File Upload Error: {0}", e.Message);
            }
        }

    }
}
