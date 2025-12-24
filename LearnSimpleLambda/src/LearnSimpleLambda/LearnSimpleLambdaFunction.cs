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
using LearnSimpleLambda.Utilities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LearnSimpleLambda
{
    public class LearnSimpleLambdaFunction
    {
        private APIUtils apiUtils = new APIUtils();

        /* 
        *  Sample input json body to submit for this Lambda
        *  
        *  {
             "anyjson":"somevalue"
           }
        */
        public object LearnSimpleHandler(object input, ILambdaContext context)
        {
            //basic elements of our response
            bool success = true;
            string message = "";
            string responseText = "";
            try
            {
                string environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
                responseText += "LearnSimpleLambda CDK Lambda call at " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss") + "\n";
                responseText += "with Environment variable=" + environment + "\n";

                //Reading the incoming request body to show we received it
                var request = JObject.Parse("" + input);
                responseText += request["body"].ToString();
            }
            catch (Exception exc)
            {
                message += "SimpleLambdaHandler Exception:" + exc.Message + "," + exc.StackTrace;
                success = false;
            }

            //create the responseBody for the response
            var response = apiUtils.returnResponse(responseText, success, message);

            return response;
        }
    }
}