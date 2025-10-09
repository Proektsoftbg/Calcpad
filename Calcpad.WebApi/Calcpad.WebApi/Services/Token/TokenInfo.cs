using MongoDB.Bson;

namespace Calcpad.WebApi.Services.Token
{
    public class TokenInfo
    {
        public ObjectId UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = [];
    }
}
