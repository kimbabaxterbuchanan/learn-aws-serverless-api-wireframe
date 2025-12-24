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
using static DynamoDbReadLambda.Services.DynamoDbService;
using DynamoDbReadLambda.Utilities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DynamoDbReadLambda
{
    public class DynamoDbReadLambdaFunction
    {
        private APIUtils apiUtils = new APIUtils();


        /* 
        *  
        *  {
            "email":"jeff.bezos@amazon.com"
         }
        */
        public object LearnReadDynamoDBHandler(object input, ILambdaContext context)
        {

            //basic elements of our response
            bool success = true;
            string message = "";
            string responseText = "";
            string requestBody = "";

            try
            {
                responseText = "ReadDynamoDB CDK Lambda " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss") + "\n";
                string environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
                string tableName = Environment.GetEnvironmentVariable("TABLE");

                apiUtils.getDynamoRead(input);

                var email = apiUtils.email;

                responseText += "Searching for user(" + email + ")\n";
                DynamoDbUserService dynamoDbUserService = new DynamoDbUserService();
                Console.WriteLine("Email Address: " + environment + "\n" + email);
                dynamoDbUserService.GetDynamoDbUser(environment, email).Wait();
                var user = dynamoDbUserService.user;
                responseText += "DynamoDBUserService Log:" + dynamoDbUserService.log + ".";
                if (user != null)
                {
                    responseText += " Found the User:" + user.email + "," + user.firstName + "," + user.lastName;
                }
                else
                {
                    success = false;
                    responseText += " Did not find the user(" + email + ")";
                }
            }
            catch (Exception exc)
            {
                success = false;
                message += "ReadDynamoDBLambdaHandler Exception:" + exc.Message + ",\n" + exc.StackTrace;
                Console.WriteLine("Error: " + message);
            }


            //create the responseBody for the response
            var response = apiUtils.returnResponse(responseText, success, message);

            return response;
        }
    }
}