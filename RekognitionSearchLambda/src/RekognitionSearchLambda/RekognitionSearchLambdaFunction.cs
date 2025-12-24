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
using RekognitionSearchLambda.Utilities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RekognitionSearchLambda
{
    public class RekognitionSearchLambdaFunction
    {
        private APIUtils apiUtils = new APIUtils();

        /* {
         "collectionName":"collectionName",
         "imageContent": "patient photo base64string"
         */
        public object LearnSearchCollectionHandler(object input, ILambdaContext context)
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

                apiUtils.getCollectionSearch(input);

                string collectionId = string.IsNullOrEmpty(apiUtils.collectionId) ? "" : apiUtils.collectionId;

                string imageId = string.IsNullOrEmpty(apiUtils.imageId) ? "" : apiUtils.imageId;
                string imageContent = string.IsNullOrEmpty(apiUtils.imageContent) ? "" : apiUtils.imageContent;

                AmazonRekognitionClient amazonClient = new AmazonRekognitionClient(RegionEndpoint.GetBySystemName(region));

                responseText += collectionId + " " + imageId + " ";

                try
                {
                    var describeCollectionRequest = new DescribeCollectionRequest { CollectionId = collectionId };
                    var describeCollectionResponseTask = amazonClient.DescribeCollectionAsync(describeCollectionRequest);
                    if (describeCollectionResponseTask != null && describeCollectionResponseTask.Result != null)
                    {
                        var describeCollectionResponse = describeCollectionResponseTask.Result;

                        if (!string.IsNullOrWhiteSpace(describeCollectionResponse.CollectionARN))
                        {

                            Image _image = new Image();
                            _image.Bytes = new MemoryStream(Convert.FromBase64String(imageContent));

                            var searchRequest = new SearchFacesByImageRequest();
                            searchRequest.Image = _image;
                            searchRequest.CollectionId = collectionId;


                            var searchFacesByImageResponseTask = amazonClient.SearchFacesByImageAsync(searchRequest);
                            var searchFacesByImageResponse = searchFacesByImageResponseTask.Result;

                            if (searchFacesByImageResponse.HttpStatusCode == HttpStatusCode.OK)
                            {
                                var faceMatch = searchFacesByImageResponse.FaceMatches[0];
                                if (double.TryParse(faceMatch.Similarity.ToString(), out var confidence))
                                {
                                    float confidenceLevel = (float)Math.Round(confidence, 4);
                                    responseText += "Confidence of " + confidenceLevel + "%";
                                    success = true;
                                }
                            }
                            else
                            {
                                message += searchFacesByImageResponse.HttpStatusCode.ToString();
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
                    responseText += "SearchCollectionLambdaHandler Errored searching for Collection " + ex.Message + ":" + ex.StackTrace;
                }
            }
            catch (Exception exc)
            {
                message += "SearchCollectionLambdaHandler Exception:" + exc.Message + ":" + exc.StackTrace;
                success = false;
            }

            //create the responseBody for the response
            var response = apiUtils.returnResponse(responseText, success, message);
            return response;
        }
    }
}