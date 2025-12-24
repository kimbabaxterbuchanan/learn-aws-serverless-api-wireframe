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
using RekognitionIndexLambda.Utilities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RekognitionIndexLambda
{
    public class RekognitionIndexLambdaFunction
    {
        private APIUtils apiUtils = new APIUtils();

        /* {
         "collectionName":"collectionName",
         "imageId": "patient string",
         "imageContent": "patient photo base64string"
         */
        public object LearnIndexCollectionHandler(object input, ILambdaContext context)
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

                apiUtils = new APIUtils();

                apiUtils.getCollectionIndex(input);

                var collectionId = apiUtils.collectionId;

                var imageId = apiUtils.imageId;

                var imageContent = apiUtils.imageContent;

                AmazonRekognitionClient amazonClient = new AmazonRekognitionClient(RegionEndpoint.GetBySystemName(region));

                responseText += collectionId + " " + imageId + " ";

                try
                {
                    var describeCollectionRequest = new DescribeCollectionRequest { CollectionId = collectionId };
                    var describeCollectionResponseTask = amazonClient.DescribeCollectionAsync(describeCollectionRequest);
                    if (describeCollectionResponseTask != null && describeCollectionResponseTask.Result != null)
                    {
                        var describeCollectionResponse = describeCollectionResponseTask.Result;

                        if (describeCollectionResponse != null && !string.IsNullOrWhiteSpace(describeCollectionResponse.CollectionARN))
                        {
                            Console.WriteLine($"Collection {collectionId}");
                            Console.WriteLine($"Image {imageContent}");
                            Console.WriteLine($"Image {imageContent}");

                            Image _image = new Image();
                            _image.Bytes = new MemoryStream(Convert.FromBase64String(imageContent));

                            bool bFoundImage = false;
                            try
                            {
                                var searchRequest = new SearchFacesByImageRequest();
                                searchRequest.Image = _image;
                                searchRequest.CollectionId = collectionId;


                                var searchFacesByImageResponseTask = amazonClient.SearchFacesByImageAsync(searchRequest);
                                var searchFacesByImageResponse = searchFacesByImageResponseTask.Result;

                                if (searchFacesByImageResponse.HttpStatusCode == HttpStatusCode.OK &&
                                    searchFacesByImageResponse != null &&
                                    searchFacesByImageResponse.FaceMatches.Count() > 0)
                                {
                                    var faceMatch = searchFacesByImageResponse.FaceMatches[0];
                                    if (double.TryParse(faceMatch.Similarity.ToString(), out var confidence))
                                    {
                                        float confidenceLevel = (float)Math.Round(confidence, 4);
                                        if (confidenceLevel > 98f)
                                        {
                                            responseText += "302";
                                        }
                                        else
                                        {
                                            responseText += "304";
                                        }
                                        success = true;
                                        bFoundImage = true;
                                    }
                                }
                                else
                                {
                                    responseText += "304";
                                }
                            }
                            catch { }

                            if (!bFoundImage)
                            {
                                var indexRequest = new IndexFacesRequest();
                                indexRequest.Image = _image;
                                indexRequest.ExternalImageId = imageId.ToString();
                                indexRequest.CollectionId = collectionId;

                                var indexFacesResponseTask = amazonClient.IndexFacesAsync(indexRequest);

                                var indexFacesResponse = indexFacesResponseTask.Result;

                                if (indexFacesResponse.HttpStatusCode == HttpStatusCode.OK)
                                {
                                    responseText += " Successful Index Collection";
                                    success = true;
                                }
                                else
                                {
                                    message += indexFacesResponse.HttpStatusCode.ToString();
                                }
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
                    responseText += "IndexCollectionLambdaHandler Errored Indexing for Collection " + ex.Message + ":" + ex.StackTrace;
                }
            }
            catch (Exception exc)
            {
                message += "IndexCollectionLambdaHandler Exception:" + exc.Message + ":" + exc.StackTrace;
                success = false;
            }

            //create the responseBody for the response
            var response = apiUtils.returnResponse(responseText, success, message);

            return response;

        }
    }
}