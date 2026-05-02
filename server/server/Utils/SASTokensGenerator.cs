using System;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace server.Utils
{
    public static class SASTokensGenerator
    {
        /// <summary>
        /// Synchronous wrapper for compatibility. Prefer using GenerateSasTokenAsync and await.
        /// </summary>
        [Obsolete("Use GenerateSasTokenAsync and await it. This synchronous wrapper may block threads.")]
        public static string GenerateSasToken(string blobServicePath, string containerName, string blobName, string? versionId = null, int expiryHours =1)
        {
            return GenerateSasTokenAsync(blobServicePath, containerName, blobName, versionId, expiryHours).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Generates a User Delegation SAS for the specified blob that grants read permissions.
        /// Returns a full URL including the SAS query string.
        /// </summary>
        public static async Task<string> GenerateSasTokenAsync(string blobServicePath, string containerName, string blobName, string? versionId = null, int expiryHours =1)
        {
            if (string.IsNullOrWhiteSpace(blobServicePath)) throw new ArgumentException("blobServicePath is required", nameof(blobServicePath));
            if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentException("containerName is required", nameof(containerName));
            if (string.IsNullOrWhiteSpace(blobName)) throw new ArgumentException("blobName is required", nameof(blobName));
            if (expiryHours <=0) expiryHours =1;

            var serviceUri = new Uri(blobServicePath.TrimEnd('/'));
            // extract storage account name from host: "{account}.blob.core.windows.net"
            string accountName = serviceUri.Host.Split('.')[0];

            var blobServiceClient = new BlobServiceClient(serviceUri, new DefaultAzureCredential());

            // get user delegation key for the given lifetime
            Response<UserDelegationKey> userDelegationKey = await blobServiceClient.GetUserDelegationKeyAsync(
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddHours(expiryHours)
            );

            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(expiryHours)
            };

            if (!string.IsNullOrEmpty(versionId))
            {
                sasBuilder.BlobVersionId = versionId;
            }

            // read permission sufficient for downloading
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasToken = sasBuilder.ToSasQueryParameters(userDelegationKey.Value, accountName).ToString();

            return $"{blobClient.Uri}?{sasToken}";
        }
    }
}
