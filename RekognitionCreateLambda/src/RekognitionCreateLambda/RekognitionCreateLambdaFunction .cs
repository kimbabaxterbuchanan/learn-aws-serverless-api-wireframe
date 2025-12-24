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
using RekognitionCreateLambda.Utilities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RekognitionCreateLambda
{
    public class RekognitionCreateLambdaFunction
    {
        private APIUtils apiUtils = new APIUtils();

        /* {
         "collectionName":"collectionName"
         */
        public object LearnCreateCollectionHandler(object input, ILambdaContext context)
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

                    var describeCollectionResponse = describeCollectionResponseTask.Result;

                    if (describeCollectionResponse != null && !string.IsNullOrWhiteSpace(describeCollectionResponse.CollectionARN))
                    {
                        var deleteCollectionRequest = new DeleteCollectionRequest { CollectionId = collectionId };
                        amazonClient.DeleteCollectionAsync(deleteCollectionRequest);
                    }
                }
                catch { }

                var createCollectionRequest = new CreateCollectionRequest { CollectionId = collectionId };
                var createCollectionResponseTask = amazonClient.CreateCollectionAsync(createCollectionRequest);
                if (createCollectionResponseTask != null && createCollectionResponseTask.Result != null)
                {

                    var createCollectionResponse = createCollectionResponseTask.Result;

                    if (createCollectionResponse.HttpStatusCode == HttpStatusCode.OK)
                    {
                        responseText += "successful creation occurred";
                        success = true;
                    }
                    else
                    {
                        message += createCollectionResponse.HttpStatusCode.ToString();
                    }
                }
                else
                {
                    responseText += "Create Collection Failed";
                }
            }
            catch (Exception exc)
            {
                message += "CreateCollectionLambdaHandler Exception:" + exc.Message + ":" + exc.StackTrace;
                success = false;
            }

            //create the responseBody for the response
            var response = apiUtils.returnResponse(responseText, success, message);

            return response;
        }

    }
}