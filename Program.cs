using System;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

class Program
{
    static async Task Main(string[] args)
    {

        //Read Config
        var clientId = "SampleClientId";
        var tenantId = "SampleTenantId";
        var clientSecret = "SampleClientSecret";
        var storageAccountName = "SampleStorageAccount";
        var containerName = "SampleContainer";

        //Create a BlobServiceClient using the client secret credential
        var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        var blobServiceClient = new BlobServiceClient(new Uri($"https://{storageAccountName}.blob.core.windows.net"), clientSecretCredential);

        //Get the container client
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);


        int choice;
        string input = string.Empty;
        while (true)
        {
            //MENU
            Console.WriteLine("\n1.View Blobs");
            Console.WriteLine("2.Read Blob");
            Console.WriteLine("3.Delete All");
            Console.WriteLine("4.Download Blob");
            Console.WriteLine("5.EXIT");

            input = Console.ReadLine() ?? "1";
            if (!int.TryParse(input, out choice))
            {
                Console.WriteLine("Invalid. Please Enter a number");
                return;
            }

            if (choice == 1)
            {
                Console.WriteLine($"Listing blobs in the container'{containerName}' :");
                await foreach (var blobItem in blobContainerClient.GetBlobsAsync())
                {
                    var blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
                    var properties = await blobClient.GetPropertiesAsync();
                    Console.Write($" - {blobItem.Name}, ");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"Last Modified: {blobItem.Properties.LastModified}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            else if (choice == 2)
            {
                var blobName = "FileName.csv";
                string blobUrl = $"https://{storageAccountName}.blob.core.windows.net/{containerName}/{blobName}";
                var blobClient = new BlobClient(new Uri(blobUrl), clientSecretCredential);
                await ReadBlobAsync(blobClient);
            }
            else if (choice == 3)
            {
                await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync())
                {
                    // Get the blob client
                    BlobClient blobClient = blobContainerClient.GetBlobClient(blobItem.Name);

                    // Delete the blob
                    await blobClient.DeleteIfExistsAsync();
                    Console.WriteLine($"Deleted: {blobItem.Name}");
                }

                Console.WriteLine("All blobs deleted successfully.");
            }
            else if (choice == 4)
            {
                Console.Write("Enter the name of the blob to download: ");
                string blobName = Console.ReadLine();
                string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string localFilePath = Path.Combine(downloadsFolder, blobName);
                var blobClient = blobContainerClient.GetBlobClient(blobName);
                await DownloadBlobAsync(blobClient, localFilePath);
            }
            else if (choice == 5)
            {
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("Invalid Option");
            }
        }

    }
    public static async Task DownloadBlobAsync(BlobClient blobClient, string localFilePath)
    {
        try
        {
            Console.WriteLine($"Downloading blob to {localFilePath}");
            await blobClient.DownloadToAsync(localFilePath);
            Console.WriteLine("Download Completed...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occured, {ex.Message}");
        }
    }
    public static async Task ReadBlobAsync(BlobClient blobClient)
    {

        try
        {
            Console.WriteLine("Start Downloading..");
            BlobDownloadResult result = await blobClient.DownloadContentAsync();
            Console.WriteLine(result.Content.ToString());
        }
        catch (RequestFailedException ex)
        {
            Console.WriteLine($"Error: {ex}");
            throw;
        }
    }


}
