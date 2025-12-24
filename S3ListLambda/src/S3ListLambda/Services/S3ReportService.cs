using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3ListLambda.Services
{
    internal class S3ReportService
    {
        private static AmazonS3Client s3Client = null;

        public string reportLog { get; set; }
        public ArrayList s3Objects = new ArrayList();
        public List<Tag> s3KeyTagObjects = new List<Tag>();
        public Dictionary<string,List<Tag>> s3TagObjects = new Dictionary<string, List<Tag>>();

        public void generateReport(string region, string bucketName)
        {
            reportLog = "S3ReportService ReportLog region=" + region + " bucketName=" + bucketName + " ";
            if (s3Client == null)
            {
                s3Client = new AmazonS3Client(RegionEndpoint.GetBySystemName(region));
            }
            ReadObjectDataAsync(region, bucketName).Wait();
            ReadObjectTagDataAsync(bucketName).Wait();
        }
        public async Task ReadObjectDataAsync(string region, string bucketName)
        {
            try
            {
                s3Objects = new ArrayList();
                s3TagObjects = new Dictionary<string, List<Tag>>();

                ListObjectsRequest request = new ListObjectsRequest
                {
                    BucketName = bucketName,
                };
                ListObjectsResponse response = await s3Client.ListObjectsAsync(request);
                int index = 0;
                if (response.S3Objects != null)
                {
                    foreach (S3Object o in response.S3Objects)
                    {
                        index++;
                        s3Objects.Add(o);
                    }
                }
            }
            catch (Exception e)
            {
                reportLog += "S3ReportService.ReadObjectDataAsync Exception:" + e.Message + ":" + e.StackTrace;
            }
        }
        public async Task ReadObjectTagDataAsync(string bucketName)
        {
            if (s3Objects != null)
            {
                foreach (S3Object o in s3Objects)
                {
                    Console.WriteLine("TaggingRequest = " + bucketName + " " + o.Key);
                    var getObjectTaggingResponse = await s3Client.GetObjectTaggingAsync(new GetObjectTaggingRequest
                    {
                        BucketName = bucketName,
                        Key = o.Key
                    });

                    Console.WriteLine("parse tags");
                    s3KeyTagObjects = new List<Tag>();

                    if (getObjectTaggingResponse != null && getObjectTaggingResponse.Tagging.Count > 0)
                    {
                        Console.WriteLine("Tags = " + getObjectTaggingResponse.Tagging);
                        foreach (Tag tag in getObjectTaggingResponse.Tagging)
                        {
                            s3KeyTagObjects.Add(tag);
                        }
                    }
                    Console.WriteLine("Key = " + s3KeyTagObjects.Count.ToString());
                    s3TagObjects.Add(o.Key, s3KeyTagObjects);
                }
            }
        }
    }
}
