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
using DynamoDbLambda.Utilities;
using static DynamoDbLambda.Classes.DynamoDbUserClass;

namespace DynamoDbLambda.Services
{
    public class DynamoDbService
    {

        private APIUtils apiUtils = new APIUtils();

        public class DynamoDbUserService
        {
            private static AmazonDynamoDBClient dynamoDbClient = null;
            public AwsServerlessLambdaUser user { get; set; }
            public string log { get; set; }

            public async Task GetDynamoDbUser(string environment, string email)
            {
                if (dynamoDbClient == null)
                {
                    dynamoDbClient = new AmazonDynamoDBClient();
                }

                DynamoDBContextConfig config = new DynamoDBContextConfig()
                {
//                    TableNamePrefix = environment + "-",
                };
                DynamoDBContext context = new DynamoDBContext(dynamoDbClient, config);
                log = "Looking for user(" + email + ") within dynamodb table " + environment + "-User.";
                user = await context.LoadAsync<AwsServerlessLambdaUser>(email);
                if (user != null)
                {
                    log += "*** Found that user ***";
                }
                else
                {
                    log += "*** Did not find that user";
                }
            }
        }

    }
}