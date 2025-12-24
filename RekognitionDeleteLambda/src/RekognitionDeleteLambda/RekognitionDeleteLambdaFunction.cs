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
using RekognitionDeleteLambda.Utilities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RekognitionDeleteLambda
{
    public class RekognitionDeleteLambdaFunction
    {
        private APIUtils apiUtils = new APIUtils();

        /* {
         "collectionName":"collectionName"
         */
        public object LearnDeleteCollectionHandler(object input, ILambdaContext context)
        {
            //basic elements of our response
            bool success = false;
            string message = "";
            string responseText = "";

            try
            {
                string bucketName = Environment.GetEnvironmentVariable("BUCKET");
                string region = Environment.GetEnvironmentVariable("REGION");
                string environment = Environment.GetEnvironmentVariable("ENVIRONMENT");

                apiUtils.getCollectionName(input);

                var collectionId = apiUtils.collectionId;

                AmazonRekognitionClient amazonClient = new AmazonRekognitionClient(RegionEndpoint.GetBySystemName(region));

                responseText = collectionId + " ";

                try
                {
                    var describeCollectionRequest = new DescribeCollectionRequest { CollectionId = collectionId };
                    var describeCollectionResponseTask = amazonClient.DescribeCollectionAsync(describeCollectionRequest);
                    if (describeCollectionResponseTask != null && describeCollectionResponseTask.Result != null)
                    {

                        var describeCollectionResponse = describeCollectionResponseTask.Result;

                        if (describeCollectionResponse != null && !string.IsNullOrWhiteSpace(describeCollectionResponse.CollectionARN))
                        {
                            var deleteCollectionRequest = new DeleteCollectionRequest { CollectionId = collectionId };
                            var deleteCollectionResponseTask = amazonClient.DeleteCollectionAsync(deleteCollectionRequest);

                            var deleteCollectionResponse = deleteCollectionResponseTask.Result;

                            if (deleteCollectionResponse.HttpStatusCode == HttpStatusCode.OK)
                            {
                                responseText += "successful Deletion occurred";
                                success = true;
                            }
                            else
                            {
                                message += deleteCollectionResponse.HttpStatusCode.ToString();
                            }
                        }
                        else
                        {
                            responseText += "Collection does not exists";
                        }
                    }
                    else
                    {
                        responseText += "Collection Was Not Found";
                    }
                }
                catch (Exception ex)
                {
                    responseText += "DeleteCollectionLambdaHandler Errored Deleting for Collection " + ex.Message + ":" + ex.StackTrace;
                }
            }
            catch (Exception exc)
            {
                message += "DeleteCollectionLambdaHandler Exception:" + exc.Message + ":" + exc.StackTrace;
                success = false;
            }

            //create the responseBody for the response
            var response = apiUtils.returnResponse(responseText, success, message);

            return response;
        }
    }
}