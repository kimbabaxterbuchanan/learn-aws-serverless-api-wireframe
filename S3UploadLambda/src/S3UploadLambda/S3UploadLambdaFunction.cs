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
using S3UploadLambda.Utilities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace S3UploadLambda
{
    public class S3UploadLambdaFunction
    {

        private APIUtils apiUtils = new APIUtils();

        /** Sample request json
           * {
                "bucketName":"somevalue",
                "fileName":"somevalue",
                "fileContent" "base64string"
             }
           */
        public object LearnS3uploadHandler(object input, ILambdaContext context)
        {
            //basic elements of our response
            bool success = true;
            string message = "";
            string responseText = "";

            try
            {
                apiUtils.getS3Upload(input);

                string bucketName = apiUtils.bucketName;
                string region = Environment.GetEnvironmentVariable("REGION");
                string environment = Environment.GetEnvironmentVariable("ENVIRONMENT");

                responseText = "S3 Bucket location:" + bucketName + "\n";

                string filename = string.IsNullOrEmpty(apiUtils.fileName) ? "" : apiUtils.fileName;
                string fileContent = string.IsNullOrEmpty(apiUtils.fileContent) ? "" : apiUtils.fileContent;
                if (!string.IsNullOrEmpty(filename) && !string.IsNullOrEmpty(fileContent))
                {
                    MemoryStream filestream = new MemoryStream();
                    byte[] fileBytes = Convert.FromBase64String(fileContent);
                    filestream.Write(fileBytes, 0, fileBytes.Length);
                    AmazonS3Client s3Client = new AmazonS3Client(RegionEndpoint.GetBySystemName(region));

                    var s3request = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = filename,
                        InputStream = filestream,
                    };
                    var putObjectResponse = s3Client.PutObjectAsync(s3request);
                    if (putObjectResponse.Result.HttpStatusCode == HttpStatusCode.OK)
                    {
                        responseText += filename + " successful upload occurred";
                    }
                    else
                    {
                        message += putObjectResponse.Result.HttpStatusCode.ToString();
                    }
                }
            }
            catch (Exception exc)
            {
                message += "s3UploadLambdaHandler Exception:" + exc.Message + ":" + exc.StackTrace;
                success = false;
            }

            //create the responseBody for the response
            var response = apiUtils.returnResponse(responseText, success, message);

            return response;

        }

    }
}