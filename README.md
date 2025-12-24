# Welcome to your CDK C# project!
This POC project is to demonstrate how to create an AWS Serverless project to provide
1. S3 Service
1. DynamoDB Service
1. Rekognition Service

These services are configured through the use of that awkcdk toolset.  With the proper installation of Docker, it would be possible to publish directly to AWS from MS VS.

This project is setup to be the only project and associated handle, roles, lambda functions, table and S3 buckets, like any other api.

# Definitions
1. cdkapp/src
This class identifies how to lambda functions will be created and the associated permissions. As of this writting I have not found a way to assign the permission on Rekognition and must be assigned manually through the AWS Console IAM utility.
1. cdkapp/Lambdas
this project currenly has on class name funtions.cs.  This class contains the lambda functions that wil be used the the AWS API.

#Setup
Open a CMD or Powershell window.  sYou can either
1.run separate commands
	1. cdk synth
	1. cdk bootstrap
	1. cdk deploy 
1. run awkdeploy.bat to perform the above commands.  Observe the window for any errors.
1. if you modify the Functions class or cdkappstack class
	1. rebuild project in MS VS
	1. re-execute one of the above command sections to publish the changes to AWS Console. You may perform a cdk deploy as often as you like, but no changes will be performed.
1. When finished, 
	1. run cdk destory to remove almost all entries of the cdkapp from aws.
	1. Open browser
		1. Navigate to AWS Console and login
		1. check the following do not exists
			1. API utility does not contain cpkapi
			1. S3 utility does not contain poc-mys3bucket????
			1. DynamoDb utility does not contain poc-user table
			1. Lambda utility the following functions are deleted
				1. simpleLambda
				1. StatusCollection
				1. CreateCollection
				1. DeleteCollection
				1. IndexCollection
				1. SearchCollection
				1. readDynamoDB
				1. writeDynamoDb
				1. s3ListLambda
				1. s3UploadLambda
		1. Delete entries from
			1. CloudFormation
				1. CdkappStack
				1. CDKToolKits
			2. IAM Roles
				1. enter cdk in search box, will identify all cdkapp roles
				1. select all and perform delete
		1. S3 utility
			1. select and delete the cdk-hnb-assets bucket assigned to your project.  If this file exists you will not be allowed to redeploy project.