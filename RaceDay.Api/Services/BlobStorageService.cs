using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services
{//start of namespace

    //what the controllers use to store and fetch images, so they never talk to Azure directly
    public interface IBlobStorageService
    {//start of interface

        //uploads an image into the given folder and returns the full Azure web address of the stored image
        Task<string> UploadImageAsync(IFormFile file, string folder);

        //opens a stored image from its saved Azure address so the API can send it to the browser (null if it does not exist)
        Task<(Stream Content, string ContentType)?> GetImageAsync(string blobUrl);

    }//end of interface

    //stores images in the private raceday-images container in Azure Blob Storage
    public class BlobStorageService : IBlobStorageService
    {//start of class

        private const string ContainerName = "raceday-images";
        private readonly BlobContainerClient _container;

        public BlobStorageService(IConfiguration configuration)
        {//start of constructor

            //the connection string comes from user secrets, never from the repository
            string? connectionString = configuration["AzureBlob:ConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString))
            {//start of missing connection string check
                throw new InvalidOperationException("AzureBlob:ConnectionString is not configured.");
            }//end of missing connection string check

            _container = new BlobContainerClient(connectionString, ContainerName);

        }//end of constructor

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {//start of UploadImageAsync

            //unique file name so two uploads never overwrite each other
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string blobPath = $"{folder}/{Guid.NewGuid()}{extension}";

            var blob = _container.GetBlobClient(blobPath);
            using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

            //the full address is what gets saved in the database
            return blob.Uri.AbsoluteUri;

        }//end of UploadImageAsync

        public async Task<(Stream Content, string ContentType)?> GetImageAsync(string blobUrl)
        {//start of GetImageAsync

            //the database stores the full address, so pull the blob name back out of it
            string blobName = new BlobUriBuilder(new Uri(blobUrl)).BlobName;

            var blob = _container.GetBlobClient(blobName);
            if (!await blob.ExistsAsync())
            {//start of not found check
                return null;
            }//end of not found check

            var download = await blob.DownloadStreamingAsync();
            return (download.Value.Content, download.Value.Details.ContentType);

        }//end of GetImageAsync

    }//end of class
}//end of namespace