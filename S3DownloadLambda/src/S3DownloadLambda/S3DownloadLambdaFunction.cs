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
using S3DownloadLambda.Utilities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace S3DownloadLambda
{
    public class S3DownloadLambdaFunction
    {
        private APIUtils apiUtils = new APIUtils();

        public object LearnS3downloadHandler(object input, ILambdaContext context)
        {
            //basic elements of our response
            bool success = true;
            string message = "";
            var responseText = "";

            try
            {
                apiUtils.getS3Download(input);

                string bucketName = apiUtils.bucketName;
                string region = Environment.GetEnvironmentVariable("REGION");
                string environment = Environment.GetEnvironmentVariable("ENVIRONMENT");

                string filename = string.IsNullOrEmpty(apiUtils.fileName) ? "" : apiUtils.fileName;

                AmazonS3Client s3Client = new AmazonS3Client(RegionEndpoint.GetBySystemName(region));

                var s3request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = filename
                };
                var getObjectResponse = s3Client.GetObjectAsync(s3request);
                if (getObjectResponse.Result.HttpStatusCode == HttpStatusCode.OK)
                {
                    var rtnGetObjectResponse = getObjectResponse.Result;
                    using (Stream responseStream = rtnGetObjectResponse.ResponseStream)
                    using (var reader = new StreamReader(responseStream))
                    {
                        MemoryStream ms = new MemoryStream();
                        responseStream.CopyTo(ms);
                        var filestring = Convert.ToBase64String(ms.ToArray());
                        Console.WriteLine("FileStrig Length : " + filestring.Length.ToString());
                        responseText += "\"fileContent\":" + "\"" + filestring + "\"";
                    }

                    /*
                                        using (StreamReader reader = new StreamReader(getObjectResponse.Result.ResponseStream))
                                        {
                                            reader.Read()
                                            responseText += "\"filecontent\":" + "\"" + reader.ReadToEnd();
                                        }
                                        Stream responseStream = getObjectResponse.Result.
                                        byte[] responseBytes = new byte[responseStream.Length];
                                        getObjectResponse.Result.ResponseStream.Read(responseBytes,0,responseBytes.Length);
                                        responseText += "\"filecontent\":" + "\"" + Convert.ToBase64String(responseBytes);
                                        */
                }
                else
                {
                    responseText += getObjectResponse.Result.HttpStatusCode.ToString();
                }
            }
            catch (Exception exc)
            {
                responseText += "s3UploadLambdaHandler Exception:" + exc.Message + ":" + exc.StackTrace;
                success = false;
            }

            //create the responseBody for the response
            var response = apiUtils.returnResponseText(responseText);

            return response;

        }
    }
}