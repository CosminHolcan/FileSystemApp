using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace server.Utils
{
    public static class SASTokensGenerator
    {
        public static string GenerateSasToken(string storageConnectionString, string containerName, string blobName, string versionId = "")
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = versionId == "" ? containerClient.GetBlobClient(blobName) : containerClient.GetBlobClient(blobName).WithVersion(versionId);
            DateTimeOffset expiryTime = DateTimeOffset.UtcNow.AddHours(1);
            BlobSasBuilder sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                ExpiresOn = expiryTime
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            Uri sasUri = blobClient.GenerateSasUri(sasBuilder);

            return sasUri.ToString();
        }
    }
}
