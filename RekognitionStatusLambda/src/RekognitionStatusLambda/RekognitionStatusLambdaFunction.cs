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
using RekognitionStatusLambda.Utilities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RekognitionStatusLambda
{
    public class RekognitionStatusLambdaFunction
    {
        private APIUtils apiUtils = new APIUtils();

        /* {
         "collectionName":"collectionName"
         */
        public object LearnStatusCollectionHandler(object input, ILambdaContext context)
        {
            //basic elements of our response
            bool success = false;
            string message = "";
            string responseText = "";
            string requestBody = "";

            try
            {
                string bucketName = Environment.GetEnvironmentVariable("BUCKET");
                string region = Environment.GetEnvironmentVariable("REGION");
                string environment = Environment.GetEnvironmentVariable("ENVIRONMENT");

                apiUtils.getCollectionName(input);

                var collectionId = apiUtils.collectionId;

                AmazonRekognitionClient amazonClient = new AmazonRekognitionClient(RegionEndpoint.GetBySystemName(region));

                var describeCollectionRequest = new DescribeCollectionRequest { CollectionId = collectionId };
                var describeCollectionResponseTask = amazonClient.DescribeCollectionAsync(describeCollectionRequest);

                responseText = collectionId + " ";

                try
                {
                    var describeCollectionResponse = describeCollectionResponseTask.Result;

                    if (describeCollectionResponse != null && !string.IsNullOrWhiteSpace(describeCollectionResponse.CollectionARN))
                    {
                        responseText += "collection exists";
                        success = true;
                    }
                    else
                    {
                        responseText += "collection does not exists\n";

                    }
                }
                catch (Exception ex)
                {
                    responseText += " StatusCollectionLambdaHandler Error collection " + ex.Message + ":" + ex.StackTrace;
                }
            }
            catch (Exception exc)
            {
                message += "StatusCollectionLambdaHandler Exception:" + exc.Message + ":" + exc.StackTrace;
                success = false;
            }

            //create the responseBody for the response
            var response = apiUtils.returnResponse(responseText, success, message);

            return response;
        }
    }
}