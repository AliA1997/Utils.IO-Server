using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using OpenAI.ObjectModels.SharedModels;
using System.Text;

namespace Utils.IO.Server.Services
{
    public interface IS3Service
    {
        string GetUrlFromS3(string fileKey);
        Task<PutObjectResponse> WriteFileAndSubmitToS3(string contentBody, string fileKey);
    }
    public class S3Service: IS3Service
    {
        private IAmazonS3 AmazonS3Client { get; }
        private readonly AWSConfiguration AwsConfiguration;

        public S3Service(
            AWSConfiguration awsConfiguration, 
            IAmazonS3 amazonS3Client) 
        {
            AwsConfiguration = awsConfiguration;
            AmazonS3Client = amazonS3Client;
        }

        public string GetUrlFromS3(string fileKey)
        {
            // Set expiration time for the URL
            DateTime expiration = DateTime.Now.AddDays(7);
            var request = new GetPreSignedUrlRequest()
            {
                BucketName = AwsConfiguration.S3BucketName,
                Key = fileKey,
                Expires = expiration
            };

            var s3Url = AmazonS3Client.GetPreSignedURL(request);

            return s3Url;
        }

        public async Task<PutObjectResponse> WriteFileAndSubmitToS3(string contentBody, string fileKey)
        {
            var request = new PutObjectRequest()
            {
                BucketName = AwsConfiguration.S3BucketName,
                Key = fileKey,
                ContentBody = contentBody
            };

            PutObjectResponse response = await AmazonS3Client.PutObjectAsync(request);

            // Check the response for success
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Text uploaded successfully!");
            }
            else
            {
                Console.WriteLine("Error uploading text to S3: " + response.HttpStatusCode);
            }

            return response;
        }
    }
}
