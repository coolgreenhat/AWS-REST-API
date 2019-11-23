using System;
using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using Amazon.S3;
using Amazon;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace PostAPI
{
    public class Functions
    {
        static string bucketName = "employeedg";
        static string keyName = "developer";
        private static IAmazonS3 s3client;

        public APIGatewayProxyResponse Post(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Post Request\n");
            // Deserialize the request object from the Json request body
            try
            {
                PostData post = JsonConvert.DeserializeObject<PostData>(request?.Body);
                RequestData req = new RequestData();
            
                if (post != null)
                {
                    
                    using (s3client = new AmazonS3Client(RegionEndpoint.APSoutheast1)) 
                    {
                        Console.WriteLine("Writing An Object To S3...");
                        WriteToBucket.PutS3Object(bucketName,keyName,req); // To Write Data to Bucket
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK, 
                Body = "Data Submitted to S3", 
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };

            return response;
        }
    }
}
