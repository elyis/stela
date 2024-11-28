using Stela_file_server.Core.Entities.Request;

namespace Stela_file_server.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}