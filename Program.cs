using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace ImageReconstruction
{
    public class Program
    {


        static async Task Main(string[] args)
        {

            string blobStorageUrl = "https://inversionrecruitment.blob.core.windows.net/find-the-code";
            string localDirectory = "DownloadedImages";

            // Create a directory to store downloaded images if it doesn't exist
            if (!Directory.Exists(localDirectory))
            {
                Directory.CreateDirectory(localDirectory);
            }

            // Download 1200 PNG images from the blob container
            await DownloadImages(blobStorageUrl, localDirectory);

            // Reconstruct the original image
            ReconstructImage(localDirectory);
        }

        static async Task DownloadImages(string blobStorageUrl, string localDirectory)
        {

            string connectionString = "https://inversionrecruitment.blob.core.windows.net"; // TODO: Replace with your actual connection string
            string containerName = "find-the-code"; // TODO: Replace with your container name

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            for (int i = 1; i <= 1200; i++)
            {
                string blobName = $"{i}.png";

                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                using Stream stream = await blobClient.OpenReadAsync();
                string localFilePath = Path.Combine(localDirectory, $"{i}.png");
                using FileStream fileStream = File.Create(localFilePath);
                await stream.CopyToAsync(fileStream);
                Console.WriteLine($"Downloaded image {i}");
            }

            Console.WriteLine("All images downloaded successfully.");
        }

        static void ReconstructImage(string localDirectory)
        {
            Console.WriteLine("Reconstructing the original image...");

            // Assuming the images are named sequentially from 1.png to 1200.png
            // You may need to adjust this logic if the filenames are different
            int tilesWide = 40;
            int tilesHigh = 30;
            int totalTiles = tilesWide * tilesHigh;
            int tileWidth = 100; // Assuming each tile has a width of 100 pixels
            int tileHeight = 100; // Assuming each tile has a height of 100 pixels

            using (var bitmap = new System.Drawing.Bitmap(tileWidth * tilesWide, tileHeight * tilesHigh))
            {
                using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                {
                    for (int i = 1; i <= totalTiles; i++)
                    {
                        int row = (i - 1) / tilesWide;
                        int col = (i - 1) % tilesWide;
                        string imagePath = Path.Combine(localDirectory, $"{i}.png");
                        using (var tile = new System.Drawing.Bitmap(imagePath))
                        {
                            graphics.DrawImage(tile, col * tileWidth, row * tileHeight);
                        }
                    }
                }
                bitmap.Save("ReconstructedImage.png");
            }

            Console.WriteLine("Reconstruction complete. The reconstructed image is saved as 'ReconstructedImage.png'.");
        }
    }
}
