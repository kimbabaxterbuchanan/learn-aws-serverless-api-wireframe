using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using System.Collections.Generic;
using Amazon.CDK.AWS.DynamoDB;

using System;
using Amazon.CDK.AWS.IAM;
/*
 * This class is used to generate the CloudFormation, IAM, Lambda, S3 DynamoDb and API entries.  To publish this to AWS
 * the CDK cli must be executed.  The awsDeploy.bat performs all commands required to deploy. Once, deployed 
 * each Lambda solution can be modified and published to AWS.  To change the add new lambda functions or change lambda function
 * permissions, execute the awsdeploy.bat.  If it is desired to remove this CloudFormation application simply open a 
 * command window, and execute a cdk destroy command.
 */ 
namespace LearnAwsServerlessApiWireframe
{
    public class LearnAwsServerlessApiWireframeStack : Stack
    {
        internal LearnAwsServerlessApiWireframeStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            //Stage setting for Deployment (Need to have Deploy = false in RestApiProps to configure the Stage
            string environment = "AwsServerlessLambda";


            //STORAGE INFRASTRUCTURE
            // default RemovalPolicy is RETAIN (on a "cdk destroy")
            //Create an S3 Bucket (s3 Buckets must be unique for each region)
            //S3 Buckets must be unique by region
            //NOTE: If you put objects in this S3 bucket you will be required to delete it manually
            var rand = new Random();
            int randNumber = rand.Next(100000);
            var s3Bucket = new Bucket(this, "AwsServerlessLambdaS3Bucket", new BucketProps
            {
                BucketName = (environment + "-").ToLower(),
                RemovalPolicy = RemovalPolicy.RETAIN
            });

            //Create a DynamoDB Table
            var dynamoDbTable = new Table(this, "AwsServerlessLambdaUser", new TableProps
            {
                TableName = environment + "-" + "User",
                PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
                {
                    Name = "email",
                    Type = AttributeType.STRING
                },
                ReadCapacity = 1,
                WriteCapacity = 1,
                RemovalPolicy = RemovalPolicy.RETAIN
            });


            //COMPUTE INFRASTRUCTURE
            //Basic Lambda
            var simpleLambdaHandler = new Function(this, "lambdaSimpleHandler", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                FunctionName = "LearnSimpleLambda",
                //Where to get the code
                Code = Code.FromAsset("LearnSimpleLambda\\src\\LearnSimpleLambda\\bin\\Debug\\net6.0"),
                Handler = "LearnSimpleLambda::LearnSimpleLambda.LearnSimpleLambdaFunction::LearnSimpleHandler",
                Environment = new Dictionary<string, string>
                {
                    ["ENVIRONMENT"] = environment,
                    ["BUCKET"] = s3Bucket.BucketName
                }
            });
            s3Bucket.GrantReadWrite(simpleLambdaHandler);

            var appS3Role = new Role(this, "LearnS3listHandlerService-Role" + randNumber.ToString(), new RoleProps
            {
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
                ManagedPolicies = new[] {
                    ManagedPolicy.FromAwsManagedPolicyName("AmazonS3FullAccess")
                }
            });

            //S3 Lambda
            var LearnS3listHandler = new Function(this, "LearnS3listHandler", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                Timeout = Duration.Seconds(20),
                FunctionName = "LearnS3list",
                //Where to get the code
                Code = Code.FromAsset("S3ListLambda\\src\\S3ListLambda\\bin\\Debug\\net6.0"),
                Handler = "S3ListLambda::S3ListLambda.S3ListLambdaFunction::LearnS3listHandler",
                Environment = new Dictionary<string, string>
                {
                    ["ENVIRONMENT"] = environment,
                    ["BUCKET"] = s3Bucket.BucketName,
                    ["REGION"] = this.Region
                },
                Role = appS3Role
            });

            appS3Role = new Role(this, "LearnS3uploadHandlerService-Role" + randNumber.ToString(), new RoleProps
            {
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
                ManagedPolicies = new[] {

                    ManagedPolicy.FromAwsManagedPolicyName("AmazonS3FullAccess")
                }
            });

            var LearnS3uploadHandler = new Function(this, "LearnS3uploadHandler", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                Timeout = Duration.Seconds(20),
                FunctionName = "LearnS3upload",
                //Where to get the code
                Code = Code.FromAsset("S3UploadLambda\\src\\S3UploadLambda\\bin\\Debug\\net6.0"),
                Handler = "S3UploadLambda::S3UploadLambda.S3UploadLambdaFunction::LearnS3uploadHandler",
                Environment = new Dictionary<string, string>
                {
                    ["ENVIRONMENT"] = environment,
                    ["BUCKET"] = s3Bucket.BucketName,
                    ["REGION"] = this.Region
                },
                Role = appS3Role
            });

            appS3Role = new Role(this, "LearnS3downloadHandlerService-Role" + randNumber.ToString(), new RoleProps
            {
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
                ManagedPolicies = new[] {

                    ManagedPolicy.FromAwsManagedPolicyName("AmazonS3FullAccess")
                }
            });

            var LearnS3downloadHandler = new Function(this, "LearnS3downloadHandler", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                Timeout = Duration.Seconds(20),
                FunctionName = "LearnS3download",
                //Where to get the code
                Code = Code.FromAsset("S3DownloadLambda\\src\\S3DownloadLambda\\bin\\Debug\\net6.0"),
                Handler = "S3DownloadLambda::S3DownloadLambda.S3DownloadLambdaFunction::LearnS3downloadHandler",
                Environment = new Dictionary<string, string>
                {
                    ["ENVIRONMENT"] = environment,
                    ["BUCKET"] = s3Bucket.BucketName,
                    ["REGION"] = this.Region
                },
                Role = appS3Role
            });

            randNumber = rand.Next(100000);

            var appRole = new Role(this, "LearnSStatusCollectionHandler-Role" + randNumber.ToString(), new RoleProps
            {
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
                ManagedPolicies = new[] {

                    ManagedPolicy.FromAwsManagedPolicyName("AmazonRekognitionFullAccess"),
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AmazonRekognitionServiceRole"),
                    ManagedPolicy.FromAwsManagedPolicyName("AmazonRekognitionCustomLabelsFullAccess"),
                }
            });

            var LearnSStatusCollectionHandler = new Function(this, "LearnSStatusCollectionHandler", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                Timeout = Duration.Seconds(20),
                FunctionName = "LearnStatusCollection",
                //Where to get the code
                Code = Code.FromAsset("RekognitionStatusLambda\\src\\RekognitionStatusLambda\\bin\\Debug\\net6.0"),
                Handler = "RekognitionStatusLambda::RekognitionStatusLambda.RekognitionStatusLambdaFunction::LearnStatusCollectionHandler",
                Environment = new Dictionary<string, string>
                {
                    ["ENVIRONMENT"] = environment,
                    ["BUCKET"] = s3Bucket.BucketName,
                    ["REGION"] = this.Region
                },
                Role = appRole,
            });

            appRole = new Role(this, "LearnCreateCollectionHandler-Role" + randNumber.ToString(), new RoleProps
            {
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
                ManagedPolicies = new[] {

                    ManagedPolicy.FromAwsManagedPolicyName("AmazonRekognitionFullAccess"),
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AmazonRekognitionServiceRole"),
                    ManagedPolicy.FromAwsManagedPolicyName("AmazonRekognitionCustomLabelsFullAccess"),
                }
            });

            var LearnCreateCollectionHandler = new Function(this, "LearnCreateCollectionHandler", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                Timeout = Duration.Seconds(20),
                FunctionName = "LearnCreateCollectio",
                //Where to get the code
                Code = Code.FromAsset("RekognitionCreateLambda\\src\\RekognitionCreateLambda\\bin\\Debug\\net6.0"),
                Handler = "RekognitionCreateLambda::RekognitionCreateLambda.RekognitionCreateLambdaFunction::LearnCreateCollectionHandler",
                Environment = new Dictionary<string, string>
                {
                    ["ENVIRONMENT"] = environment,
                    ["BUCKET"] = s3Bucket.BucketName,
                    ["REGION"] = this.Region
                },
                Role = appRole,
            });

            appRole = new Role(this, "LearnDeleteCollectionHandler-Role" + randNumber.ToString(), new RoleProps
            {
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
                ManagedPolicies = new[] {

                    ManagedPolicy.FromAwsManagedPolicyName("AmazonRekognitionFullAccess"),
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AmazonRekognitionServiceRole"),
                    ManagedPolicy.FromAwsManagedPolicyName("AmazonRekognitionCustomLabelsFullAccess"),
                }
            });

            var LearnDeleteCollectionHandler = new Function(this, "LearnDeleteCollectionHandler", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                Timeout = Duration.Seconds(20),
                FunctionName = "LearnDeleteCollection",
                //Where to get the code
                Code = Code.FromAsset("RekognitionDeleteLambda\\src\\RekognitionDeleteLambda\\bin\\Debug\\net6.0"),
                Handler = "RekognitionDeleteLambda::RekognitionDeleteLambda.RekognitionDeleteLambdaFunction::LearnDeleteCollectionHandler",
                Environment = new Dictionary<string, string>
                {
                    ["ENVIRONMENT"] = environment,
                    ["BUCKET"] = s3Bucket.BucketName,
                    ["REGION"] = this.Region
                },
                Role = appRole
            });

            appRole = new Role(this, "LearnIndexCollectionHandler-Role" + randNumber.ToString(), new RoleProps
            {
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
                ManagedPolicies = new[] {

                    ManagedPolicy.FromAwsManagedPolicyName("AmazonRekognitionFullAccess"),
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AmazonRekognitionServiceRole"),
                    ManagedPolicy.FromAwsManagedPolicyName("AmazonRekognitionCustomLabelsFullAccess"),
                }
            });

            var LearnIndexCollectionHandler = new Function(this, "LearnIndexCollectionHandler", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                Timeout = Duration.Seconds(20),
                FunctionName = "LearnIndexCollection",
                //Where to get the code
                Code = Code.FromAsset("RekognitionIndexLambda\\src\\RekognitionIndexLambda\\bin\\Debug\\net6.0"),
                Handler = "RekognitionIndexLambda::RekognitionIndexLambda.RekognitionIndexLambdaFunction::LearnIndexCollectionHandler",
                Environment = new Dictionary<string, string>
                {
                    ["ENVIRONMENT"] = environment,
                    ["BUCKET"] = s3Bucket.BucketName,
                    ["REGION"] = this.Region
                },
                Role = appRole,
            });

            appRole = new Role(this, "LearnSearchCollectionHandler-Role" + randNumber.ToString(), new RoleProps
            {
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
                ManagedPolicies = new[] {

                    ManagedPolicy.FromAwsManagedPolicyName("AmazonRekognitionFullAccess"),
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AmazonRekognitionServiceRole"),
                    ManagedPolicy.FromAwsManagedPolicyName("AmazonRekognitionCustomLabelsFullAccess"),
                }
            });

            var LearnSearchCollectionHandler = new Function(this, "LearnSearchCollectionHandler", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                Timeout = Duration.Seconds(20),
                FunctionName = "LearnSearchCollection",
                //Where to get the code
                Code = Code.FromAsset("RekognitionSearchLambda\\src\\RekognitionSearchLambda\\bin\\Debug\\net6.0"),
                Handler = "RekognitionSearchLambda::RekognitionSearchLambda.RekognitionSearchLambdaFunction::LearnSearchCollectionHandler",
                Environment = new Dictionary<string, string>
                {
                    ["ENVIRONMENT"] = environment,
                    ["BUCKET"] = s3Bucket.BucketName,
                    ["REGION"] = this.Region
                },
                Role = appRole,
            });

            //Write DynamoDb Lambda
            var LearnWriteDynamoDBHandler = new Function(this, "LearnWriteDynamoDBHandler", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                FunctionName = "LearnWriteDynamoDB",
                Timeout = Duration.Seconds(20),
                //Where to get the code
                Code = Code.FromAsset("DynamoDbWriteLambda\\src\\DynamoDbWriteLambda\\bin\\Debug\\net6.0"),
                Handler = "DynamoDbWriteLambda::DynamoDbWriteLambda.DynamoDbWriteLambdaFunction::LearnWriteDynamoDBHandler",
                Environment = new Dictionary<string, string>
                {
                    ["ENVIRONMENT"] = environment,
                    //["BUCKET"] = s3Bucket.BucketName,
                    ["TABLE"] = dynamoDbTable.TableName

                } 
            });
            String[] dynamoDbpermissions = new string[] { "dynamodb:DescribeTable" };

            dynamoDbTable.GrantFullAccess(LearnWriteDynamoDBHandler);
            dynamoDbTable.Grant(LearnWriteDynamoDBHandler, dynamoDbpermissions);

            //Read DynamoDb Lambda
            var LearnReadDynamoDBHandler = new Function(this, "LearnReadDynamoDBHandler", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                FunctionName = "LearnReadDynamoDB",
                Timeout = Duration.Seconds(20),
                //Where to get the code
                Code = Code.FromAsset("DynamoDbReadLambda\\src\\DynamoDbReadLambda\\bin\\Debug\\net6.0"),
                Handler = "DynamoDbReadLambda::DynamoDbReadLambda.DynamoDbReadLambdaFunction::LearnReadDynamoDBHandler",
                Environment = new Dictionary<string, string>
                {
                    ["ENVIRONMENT"] = environment,
                    ["BUCKET"] = s3Bucket.BucketName,
                    ["TABLE"] = dynamoDbTable.TableName
                }
            });

            dynamoDbTable.GrantFullAccess(LearnReadDynamoDBHandler);
            dynamoDbTable.Grant(LearnReadDynamoDBHandler, dynamoDbpermissions);

            //This is the name of the API in the APIGateway
            var api = new RestApi(this, "AwsServerlessAPI", new RestApiProps
            {
                RestApiName = "AwsServerlessAPI",
                Description = "This our Learn API",
                Deploy = false
            });

            var deployment = new Deployment(this, "My Deployment", new DeploymentProps { Api = api });
            var stage = new Amazon.CDK.AWS.APIGateway.Stage(this, "stage name", new Amazon.CDK.AWS.APIGateway.StageProps
            {
                Deployment = deployment,
                StageName = environment
            });
            api.DeploymentStage = stage;

            //Lambda integrations
            var simpleLambdaIntegration = new LambdaIntegration(simpleLambdaHandler, new LambdaIntegrationOptions
            {
                RequestTemplates = new Dictionary<string, string>
                {
                    ["application/json"] = "{ \"statusCode\": \"200\" }"
                }
            });

            var LearnS3listIntegration = new LambdaIntegration(LearnS3listHandler, new LambdaIntegrationOptions
            {
                RequestTemplates = new Dictionary<string, string>
                {
                    ["application/json"] = "{ \"statusCode\": \"200\" }"
                }
            });

            var LearnS3uploadIntegration = new LambdaIntegration(LearnS3uploadHandler, new LambdaIntegrationOptions
            {
                RequestTemplates = new Dictionary<string, string>
                {
                    ["application/json"] = "{ \"statusCode\": \"200\" }"
                }
            });

            var LearnS3downloadIntegration = new LambdaIntegration(LearnS3downloadHandler, new LambdaIntegrationOptions
            {
                RequestTemplates = new Dictionary<string, string>
                {
                    ["application/json"] = "{ \"statusCode\": \"200\" }"
                }
            }); 
            
            var LearnCreateCollectionIntegration = new LambdaIntegration(LearnCreateCollectionHandler, new LambdaIntegrationOptions
            {
                RequestTemplates = new Dictionary<string, string>
                {
                    ["application/json"] = "{ \"statusCode\": \"200\" }"
                }
            });
            var LearnDeleteCollectionIntegration = new LambdaIntegration(LearnDeleteCollectionHandler, new LambdaIntegrationOptions
            {
                RequestTemplates = new Dictionary<string, string>
                {
                    ["application/json"] = "{ \"statusCode\": \"200\" }"
                }
            });
            var LearnSStatusCollectionIntegration = new LambdaIntegration(LearnSStatusCollectionHandler, new LambdaIntegrationOptions
            {
                RequestTemplates = new Dictionary<string, string>
                {
                    ["application/json"] = "{ \"statusCode\": \"200\" }"
                }
            });
            var LearnIndexCollectionIntegration = new LambdaIntegration(LearnIndexCollectionHandler, new LambdaIntegrationOptions
            {
                RequestTemplates = new Dictionary<string, string>
                {
                    ["application/json"] = "{ \"statusCode\": \"200\" }"
                }
            });
            var LearnSearchCollectionIntegration = new LambdaIntegration(LearnSearchCollectionHandler, new LambdaIntegrationOptions
            {
                RequestTemplates = new Dictionary<string, string>
                {
                    ["application/json"] = "{ \"statusCode\": \"200\" }"
                }
            });

            var LearnWriteDynamoDBIntegration = new LambdaIntegration(LearnWriteDynamoDBHandler, new LambdaIntegrationOptions
            {
                RequestTemplates = new Dictionary<string, string>
                {
                    ["application/json"] = "{ \"statusCode\": \"200\" }"
                }
            });

            var LearnReadDynamoDBIntegration = new LambdaIntegration(LearnReadDynamoDBHandler, new LambdaIntegrationOptions
            {
                RequestTemplates = new Dictionary<string, string>
                {
                    ["application/json"] = "{ \"statusCode\": \"200\" }"
                }
            });


            //It is up to you if you want to structure your lambdas in separate APIGateway APIs (RestApi)

            //Option 1: Adding at the top level of the APIGateway API
            // api.Root.AddMethod("POST", simpleLambdaIntegration);

            //Option 2: Or break out resources under one APIGateway API as follows
            var simpleResource = api.Root.AddResource("simple");
            var simpleMethod = simpleResource.AddMethod("POST", simpleLambdaIntegration);

            var s3ListResource = api.Root.AddResource("s3List");
            var s3ListMethod = s3ListResource.AddMethod("POST", LearnS3listIntegration);
            var s3UploadResource = api.Root.AddResource("s3Upload");
            var s3UploadMethod = s3UploadResource.AddMethod("POST", LearnS3uploadIntegration);
            var s3DownloadResource = api.Root.AddResource("s3Download");
            var s3DownloadMethod = s3DownloadResource.AddMethod("POST", LearnS3downloadIntegration);

            var createCollectionResource = api.Root.AddResource("createCollection");
            var createCollectionMethod = createCollectionResource.AddMethod("POST", LearnCreateCollectionIntegration);
            var deleteCollectionResource = api.Root.AddResource("deleteCollection");
            var deleteCollectionMethod = deleteCollectionResource.AddMethod("POST", LearnDeleteCollectionIntegration);
            var statusCollectionResource = api.Root.AddResource("statusCollection");
            var statusCollectionMethod = statusCollectionResource.AddMethod("POST", LearnSStatusCollectionIntegration);
            var indexCollectionResource = api.Root.AddResource("indexCollection");
            var indexCollectionMethod = indexCollectionResource.AddMethod("POST", LearnIndexCollectionIntegration);
            var searchCollectionResource = api.Root.AddResource("searchCollection");
            var searchCollectionMethod = searchCollectionResource.AddMethod("POST", LearnSearchCollectionIntegration);


            var writeDynamoDBResource = api.Root.AddResource("writeDynamoDb");
            var writeDynamoDBMethod = writeDynamoDBResource.AddMethod("POST", LearnWriteDynamoDBIntegration);
            var readDynamoDBResource = api.Root.AddResource("readDynamoDb");
            var readDynamoDBMethod = readDynamoDBResource.AddMethod("POST", LearnReadDynamoDBIntegration);

            //Output results of the CDK Deployment
            new CfnOutput(this, "A Region:", new CfnOutputProps() { Value = this.Region });
            new CfnOutput(this, "B S3 Bucket:", new CfnOutputProps() { Value = s3Bucket.BucketName });
            new CfnOutput(this, "C DynamoDBTable:", new CfnOutputProps() { Value = dynamoDbTable.TableName });
            new CfnOutput(this, "D API Gateway API:", new CfnOutputProps() { Value = api.Url });
            string urlPrefix = api.Url.Remove(api.Url.Length - 1);
            new CfnOutput(this, "E Simple Lambda:", new CfnOutputProps() { Value = urlPrefix + simpleMethod.Resource.Path });
            new CfnOutput(this, "F S3 List Lambda:", new CfnOutputProps() { Value = urlPrefix + s3ListMethod.Resource.Path });
            new CfnOutput(this, "G S3 Upload Lambda:", new CfnOutputProps() { Value = urlPrefix + s3UploadMethod.Resource.Path });
            new CfnOutput(this, "H S3 Download Lambda:", new CfnOutputProps() { Value = urlPrefix + s3DownloadMethod.Resource.Path });
            new CfnOutput(this, "I Write DynamoDB Lambda:", new CfnOutputProps() { Value = urlPrefix + writeDynamoDBMethod.Resource.Path });
            new CfnOutput(this, "J Read DynamoDB Lambda:", new CfnOutputProps() { Value = urlPrefix + readDynamoDBMethod.Resource.Path });
            new CfnOutput(this, "K Create Collection Lambda:", new CfnOutputProps() { Value = urlPrefix + createCollectionMethod.Resource.Path });
            new CfnOutput(this, "L Delete Collection Lambda:", new CfnOutputProps() { Value = urlPrefix + deleteCollectionMethod.Resource.Path });
            new CfnOutput(this, "M Status Collection Lambda:", new CfnOutputProps() { Value = urlPrefix + statusCollectionMethod.Resource.Path });
            new CfnOutput(this, "N Index Collection Lambda:", new CfnOutputProps() { Value = urlPrefix + indexCollectionMethod.Resource.Path });
            new CfnOutput(this, "O Search Collection Lambda:", new CfnOutputProps() { Value = urlPrefix + searchCollectionMethod.Resource.Path });
        }
    }
}