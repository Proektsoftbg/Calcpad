using Calcpad.WebApi.Configs;
using Calcpad.WebApi.Utils.Web.Service;
using Calcpad.WebApi.Utils.Web.Token;
using System.Security.Claims;
using Calcpad.WebApi.Utils.Json;
using Calcpad.WebApi.Models;

namespace Calcpad.WebApi.Services.Token
{
    /// <summary>
    /// Scoped Service, only can be used in the request
    /// </summary>
    /// <param name="tokenParams"></param>
    /// <param name="httpContext"></param>
    public class TokenService(AppSettings<TokenParamsConfig> tokenParams, IHttpContextAccessor httpContext) : IScopedService
    {
        /// <summary>
        /// create a token for the given username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public string CreateToken(string username, CalcpadUserModel user)
        {
            // create token
            var claims = new List<Claim>
            {
                new("userId",user.Id.ToString()),
                new(ClaimTypes.Name, username),
                new(ClaimTypes.Role, "User")
            };

            // add roles
            if (user.Roles != null)
            {
                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var token = tokenParams.Value.CreateToken(claims);
            return token;
        }

        /// <summary>
        /// get token info from the request header
        /// </summary>
        /// <returns></returns>
        public TokenInfo GetTokenInfo()
        {
            if (!httpContext.HttpContext.Request.Headers.TryGetValue("Authorization", out var tokenHeader))
            {
                return new TokenInfo();
            }

            var tokenString = tokenHeader.ToString();
            if (tokenString.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                tokenString = tokenString["Bearer ".Length..].Trim();
            }

            // extract token info from the token
            var tokenPayloads = JWTToken.GetTokenPayloads(tokenString.ToString());
            if (tokenPayloads == null)
            {
                return new TokenInfo();
            }

            var tokenInfo = new TokenInfo
            {
                UserId = new MongoDB.Bson.ObjectId(tokenPayloads["userId"].ToString()!),
                Username = tokenPayloads[ClaimTypes.Name].ToString()!,
                Roles = httpContext.HttpContext.User.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .Distinct()
                    .ToList()
            };
            return tokenInfo;
        }
    }
}
