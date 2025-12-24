using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;

namespace RekognitionStatusLambda.Utilities
{
    public class APIUtils
    {
        public string? collectionId { get; set; }

        public string? imageId { get; set; }

        public string? imageContent { get; set; }


        public void getCollectionName(object input)
        {
            var request = JObject.Parse("" + input);
            var requestBody = request["body"].ToString();
            var requestBodyJson = JObject.Parse(requestBody);

            collectionId = requestBodyJson["collectionName"].ToString();

        }
        public void getCollectionIndex(object input)
        {
            var request = JObject.Parse("" + input);
            var requestBody = request["body"].ToString();
            var requestBodyJson = JObject.Parse(requestBody);

            collectionId = requestBodyJson["collectionName"].ToString();

            imageId = requestBodyJson["imageId"].ToString();

            imageContent = requestBodyJson["imageContent"].ToString();
        }
        public void getCollectionSearch(object input)
        {
            var request = JObject.Parse("" + input);
            var requestBody = request["body"].ToString();
            var requestBodyJson = JObject.Parse(requestBody);

            collectionId = requestBodyJson["collectionName"].ToString();

            imageId = requestBodyJson["imageId"].ToString();

            imageContent = requestBodyJson["imageContent"].ToString();
        }

        public APIGatewayProxyResponse returnResponse(string responseText, bool success, string message = "", string s3ObjectsJson = "")
        {
            string responseBody = "{";

            responseBody += " \"response\":\"" + responseText + "\",\n";
            responseBody += " \"success\":\"" + success + "\"";

            if (!string.IsNullOrWhiteSpace(s3ObjectsJson))
            {
                responseBody += "\",\n";
                responseBody += " \"s3Objects\":\"" + s3ObjectsJson + "\"";
            }
            if (!string.IsNullOrWhiteSpace(message))
            {
                responseBody += "\",\n";
                responseBody += " \"message\":\"" + message + "\"";
            }
            responseBody += "\n";
            responseBody += "}";

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = responseBody,
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            };

            return response;
        }
        public APIGatewayProxyResponse returnResponseText(string responseText)
        {
            string responseBody = "{\n";
            responseBody += responseText;
            responseBody += "\n}";

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = responseBody,
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            };

            return response;
        }

    }
}
