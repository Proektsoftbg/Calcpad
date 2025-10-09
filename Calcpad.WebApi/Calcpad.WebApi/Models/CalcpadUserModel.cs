using Calcpad.WebApi.Models.Base;

namespace Calcpad.WebApi.Models
{
    public class CalcpadUserModel : MongoDoc
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public List<string> Roles { get; set; } = [];
    }
}
