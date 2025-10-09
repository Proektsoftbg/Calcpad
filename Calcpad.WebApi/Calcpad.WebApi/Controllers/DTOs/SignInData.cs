using Calcpad.WebApi.Utils.Web.Exceptions;

namespace Calcpad.WebApi.Controllers.DTOs
{
    public class SignInData
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Username))
            {
                throw new KnownException("username is empty");
            }
            if(string.IsNullOrEmpty(Password))
            {
                throw new KnownException("password is empty");
            }
        }
    }
}
