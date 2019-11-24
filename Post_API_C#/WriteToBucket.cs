using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PostAPI
{
    public class WriteToBucket
    {
        public static async Task<bool> PutS3Object(string bucket, string key, PostData req)
        {
            try
            {
                using (var client = new AmazonS3Client(RegionEndpoint.APSoutheast1))
                {
                    var request = new PutObjectRequest
                    {
                        BucketName = bucket,
                        Key = key,
                        ContentBody = JsonConvert.SerializeObject(req),
                    };
                    var response = await client.PutObjectAsync(request);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in PutS3Object:" + ex.Message);
                return false;
            }
        }

    }
}
