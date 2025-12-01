using Calcpad.WebApi.Controllers.Base;
using Calcpad.WebApi.Controllers.DTOs;
using Calcpad.WebApi.Models.Base;
using Calcpad.WebApi.Services.Token;
using Calcpad.WebApi.Utils.Encrypt;
using Calcpad.WebApi.Utils.Web.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Calcpad.WebApi.Controllers
{
    public class UserController(
        ILogger<UserController> logger,
        MongoDBContext db,
        TokenService tokenService
    ) : ControllerBaseV1
    {
        /// <summary>
        /// sign in to get the token
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("sign-in")]
        public async Task<ResponseResult<string>> SignIn([FromBody] SignInData data)
        {
            data.Validate();

            var passwordMd5 = data.Password.ToMD5();
            var existUser = await db
                .CalcpadUsers.AsQueryable()
                .Where(x => x.Username == data.Username)
                .Where(x => x.Password == passwordMd5)
                .FirstOrDefaultAsync();

            if (existUser == null)
            {
                return string.Empty.ToFailResponse("username or password is incorrect");
            }

            var token = tokenService.CreateToken(data.Username, existUser);
            return token.ToSuccessResponse();
        }
    }
}
