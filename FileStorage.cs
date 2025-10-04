using Azure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System.IO;
using System.Threading.Tasks;

namespace QueueFunction
{ 
public interface IFileStorageService
    {
        Task UploadFileAsync(string directoryName, string fileName, Stream fileStream);
    }

    public class AzureFileShareStorageService : IFileStorageService
    {
        private readonly ShareClient _shareClient;

        public AzureFileShareStorageService(string connectionString, string shareName)
        {
            _shareClient = new ShareClient(connectionString, shareName);
            _shareClient.CreateIfNotExists();
        }

        public async Task UploadFileAsync(string directoryName, string fileName, Stream fileStream)
        {
       
            var directoryClient = _shareClient.GetDirectoryClient(directoryName);
            await directoryClient.CreateIfNotExistsAsync();

    
            var fileClient = directoryClient.GetFileClient(fileName);

      
            await fileClient.CreateAsync(fileStream.Length);

     
            await fileClient.UploadRangeAsync(
                new HttpRange(0, fileStream.Length),
                fileStream);
        }
    }

}

