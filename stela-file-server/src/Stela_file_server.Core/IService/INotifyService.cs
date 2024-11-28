using Stela_file_server.Core.Enums;

namespace Stela_file_server.Core.IService
{
    public interface INotifyService
    {
        void Publish<T>(T message, ContentUploaded content);
    }
}