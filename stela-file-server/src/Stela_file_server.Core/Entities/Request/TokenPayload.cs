namespace Stela_file_server.Core.Entities.Request
{
    public class TokenPayload
    {
        public Guid AccountId { get; set; }
        public string Role { get; set; }
    }
}