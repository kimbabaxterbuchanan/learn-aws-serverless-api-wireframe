using System;
using System.Collections.Generic;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Net;
using Newtonsoft.Json.Linq;
using Amazon.S3;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System.Collections;
using System.Text.Json;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using System.Text;
using S3ListLambda.Services;
using S3ListLambda.Utilities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace S3ListLambda
{
    public class S3ListLambdaFunction
    {
        private APIUtils apiUtils = new APIUtils();

        /** Sample request json
           * {
                "bucketName":"somevalue",
             }
           */
        public object LearnS3listHandler(object input, ILambdaContext context)
        {
            //basic elements of our response
            bool success = true;
            string message = "";
            string responseText = "";
            string s3ObjectsJson = "";

            try
            {
                apiUtils.getS3List(input);

                string bucketName = apiUtils.bucketName;
                string region = Environment.GetEnvironmentVariable("REGION");
                string environment = Environment.GetEnvironmentVariable("ENVIRONMENT");

                responseText = "S3 Bucket location:" + bucketName + "\n";

                S3ReportService s3Report = new S3ReportService();
                s3Report.generateReport(region, bucketName);
                responseText += "# Files in the bucket=" + s3Report.s3Objects.Count + "\n";

                //prepare to return information on any s3 objects found
                string s3ObjectJson = "\n";
                for (int i = 0; i < s3Report.s3Objects.Count; i++)
                {
                    Amazon.S3.Model.S3Object currentS3Object = (Amazon.S3.Model.S3Object)s3Report.s3Objects[i];
                    if (currentS3Object != null)
                    {
                        s3ObjectJson += "key: " + currentS3Object.Key + "\t";
                        s3ObjectJson += "bucketname: " + currentS3Object.BucketName + "\t";
                        s3ObjectJson += "region: " + region + "\t";
                        s3ObjectJson += "size: " + currentS3Object.Size + "\t";
                        s3ObjectJson += "lastmodified: " + currentS3Object.LastModified + "";

                        s3ObjectsJson += s3ObjectJson + "\n";

                        List<Tag> keyTags = new List<Tag>();
                        s3Report.s3TagObjects.TryGetValue(currentS3Object.Key, out keyTags);
                        String keyTagCount = (keyTags == null || keyTags.Count == 0) ? "0" : keyTags.Count.ToString();
                        s3ObjectsJson += "tagcount: " + keyTagCount + "\n";
                        if (keyTags != null && keyTags.Count > 0)
                        {
                            foreach (Tag tag in keyTags)
                            {
                                s3ObjectsJson += tag.Key + ":" + tag.Value + "\n";
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                message += "S3ListLambdaHandler Exception:" + exc.Message + ":" + exc.StackTrace;
                success = false;
            }

            //create the responseBody for the response
            var response = apiUtils.returnResponse(responseText, success, message, s3ObjectsJson);

            return response;
        }
    }
}