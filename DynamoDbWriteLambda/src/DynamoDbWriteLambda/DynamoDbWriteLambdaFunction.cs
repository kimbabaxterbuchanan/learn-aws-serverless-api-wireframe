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
using static DynamoDbWriteLambda.Classes.DynamoDbUserClass;
using DynamoDbWriteLambda.Utilities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DynamoDbWriteLambda
{
    public class DynamoDbWriteLambdaFunction
    {
        private APIUtils apiUtils = new APIUtils();

        /** Sample request json
         * {
                "email":"jeff.bezos@amazon.com",
         "firstName":"Jeff",
            "lastName":"Bezos"
                }
         */
        public object LearnWriteDynamoDBHandler(object input, ILambdaContext context)
        {

            //basic elements of our response
            bool success = true;
            string message = "";
            string responseText = "";
            string requestBody = "";

            try
            {
                var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
                var tableName = Environment.GetEnvironmentVariable("TABLE");
                responseText = "WriteDynamoDB CDK Lambda " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss") + "\n";
                responseText += "DynamoDB Table:" + tableName + "\n";

                apiUtils.getDynamoInsert(input);

                var email = apiUtils.email;
                var firstName = apiUtils.firstName;
                var lastName = apiUtils.lastName;

                //Create the user object to save
                var user = new AwsServerlessLambdaUser();
                user.email = email;
                user.firstName = firstName;
                user.lastName = lastName;
                DynamoDBContextConfig config = new DynamoDBContextConfig()
                {
                    //                    TableNamePrefix = environment + "-"
                };
                DynamoDBContext dynamoDbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
                var task = Task.Run(() => dynamoDbContext.SaveAsync(user).ConfigureAwait(false));
                task.Wait();

                responseText += " Successfully saved User(" + user.email + "," + firstName + "," + lastName + ") to our DynamoDB table " + environment + "-AwsServerlessLambdaUser";
                dynamoDbContext.Dispose();
            }
            catch (Exception exc)
            {
                success = false;
                message += "WriteDynamoDBLambdaHandler Exception:" + exc.Message + "," + exc.StackTrace;
            }


            //create the responseBody for the response
            var response = apiUtils.returnResponse(responseText, success, message);

            return response;
        }

    }
}