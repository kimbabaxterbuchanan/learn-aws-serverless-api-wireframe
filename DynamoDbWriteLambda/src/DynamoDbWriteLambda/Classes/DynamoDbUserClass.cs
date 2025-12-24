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

namespace DynamoDbWriteLambda.Classes
{
    public class DynamoDbUserClass
    {
        [DynamoDBTable("AwsServerlessLambda-User")]
        public class AwsServerlessLambdaUser
        {
            [DynamoDBHashKey]
            public string email { get; set; }
            [DynamoDBProperty("firstName")]
            public string firstName { get; set; }
            [DynamoDBProperty("lastName")]
            public string lastName { get; set; }
        }
    }
}