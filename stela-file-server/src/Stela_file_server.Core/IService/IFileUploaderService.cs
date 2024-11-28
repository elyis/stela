using Stela_file_server.Core.Entities.Response;

namespace Stela_file_server.Core.IService
{
    public interface IFileUploaderService
    {
        Task<ServiceResponse<string>> UploadFileAsync(string directoryPath, Stream stream, string[] supportedExtensions);
        Task<ServiceResponse<Stream>> GetStreamAsync(string directoryPath, string filename);
    }
}