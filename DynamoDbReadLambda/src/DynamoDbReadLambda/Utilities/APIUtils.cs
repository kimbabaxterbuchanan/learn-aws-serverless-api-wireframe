using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;

namespace DynamoDbReadLambda.Utilities
{
    public class APIUtils
    {
        public string? email { get; set; }

        public string? firstName { get; set; }

        public string? lastName { get; set; }

        public void getDynamoSelect(object input)
        {
            var request = JObject.Parse("" + input);
            var requestBody = request["body"].ToString();
            var requestBodyJson = JObject.Parse(requestBody);

            email = requestBodyJson["email"].ToString();
        }
        public void getDynamoInsert(object input)
        {
            var request = JObject.Parse("" + input);
            var requestBody = request["body"].ToString();
            var requestBodyJson = JObject.Parse(requestBody);

            email = requestBodyJson["email"].ToString();
            firstName = requestBodyJson["firstName"].ToString();
            lastName = requestBodyJson["lastName"].ToString();
        }
        public void getDynamoRead(object input)
        {
            var request = JObject.Parse("" + input);
            var requestBody = request["body"].ToString();
            var requestBodyJson = JObject.Parse(requestBody);

            email = requestBodyJson["email"].ToString();
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
