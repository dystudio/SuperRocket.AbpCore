using System;
using System.Collections.Generic;
using System.Data;
using System.IO;


namespace RabbitMQ
{
    public static class ExcelReportGenerator
    {
        
        /// <summary>
        /// run
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="processedSql"></param>
        /// <param name="returnProcessedSql"></param>
        /// <param name="headList"></param>
        /// <param name="SheetNameList"></param>
        public static void Run(string fileName, string processedSql, string returnProcessedSql, List<Dictionary<string, string>> headList, List<string> SheetNameList)
        {
            System.Console.WriteLine($"start to run for the report");

            DataSet dsData = new DataSet();
            System.Console.WriteLine($"after new  DataSet");
            DataTable dtDataCopy = DbHelper.ExecuteDataTable(processedSql, null);
            dtDataCopy.TableName = "基础数据表-已办";
            System.Console.WriteLine($"基础数据表-已办's length is: {dtDataCopy.Rows.Count.ToString()}");
            dsData.Tables.Add(dtDataCopy);

            System.Console.WriteLine($"after add 基础数据表-已办");

            DataTable dtTdDataCopy = DbHelper.ExecuteDataTable(returnProcessedSql, null);
            dtTdDataCopy.TableName = "基础数据表-退单";
            System.Console.WriteLine($"基础数据表-退单's length is: {dtTdDataCopy.Rows.Count.ToString()}");
            dsData.Tables.Add(dtTdDataCopy);
            System.Console.WriteLine($"after add 基础数据表-退单");

            System.Console.WriteLine($"after write data to dataset");

            var networkSharePath = System.Configuration.ConfigurationManager.AppSettings["NetworkSharePath"].ToString();
            var username = System.Configuration.ConfigurationManager.AppSettings["username"].ToString();
            var password = System.Configuration.ConfigurationManager.AppSettings["password"].ToString();
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Report\\" + fileName + ".xls");
            //var path = Path.Combine(networkSharePath, "Report\\" + fileName + ".xls");

            System.Console.WriteLine($"before to export the excel");
            try
            {
                ExcelUtility.ExportExcel(dsData, headList, SheetNameList, path);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"when export the excel :{ex.Message}");
            }
            System.Console.WriteLine($"after to export the excel");

            //System.Console.WriteLine($"before to upload the excel report");
            //copy file from local to network share folder
            //UpLoadFile(path, networkSharePath, username, password);
            //System.Console.WriteLine($"after to upload the excel report");

            System.Console.WriteLine($"before to upload the excel report to minio");
            var endPoint = System.Configuration.ConfigurationManager.AppSettings["EndPoint"].ToString();
            var accessKey = System.Configuration.ConfigurationManager.AppSettings["AccessKey"].ToString();
            var secretKey = System.Configuration.ConfigurationManager.AppSettings["SecretKey"].ToString();
            var bucketName = System.Configuration.ConfigurationManager.AppSettings["BucketName"].ToString();
            var location = System.Configuration.ConfigurationManager.AppSettings["Location"].ToString();

            MinioUploader uploader = new MinioUploader();
            uploader.BucketName = bucketName;
            uploader.Location = location;
            uploader.ObjectName = fileName + ".xls";
            uploader.ContentType = "application/vnd.ms-excel";
            uploader.FileFullPath = path;

            uploader.UploadToMinio(endPoint, accessKey, secretKey, false);
            System.Console.WriteLine($"after to upload the excel report to minio");

            System.Console.WriteLine("Finished ExcelReportGenerator.Run() on {0} successfully!", DateTime.Now.ToString());
        }
    }
}
