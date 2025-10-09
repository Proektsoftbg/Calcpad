using Calcpad.WebApi.Configs;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Calcpad.WebApi.Utils.Web.Token
{
    /// <summary>
    /// jwt 授权验证
    /// </summary>
    public static class JWTToken
    {
        /// <summary>
        /// 创建 token
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static string CreateToken(this TokenParamsConfig tokenParam, List<Claim> claims)
        {
            // 和 Startup 中的配置一致
            JwtSecurityToken token = new(
                issuer: tokenParam.Issuer,
                audience: tokenParam.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: tokenParam.GetExpireDate(),
                signingCredentials: new SigningCredentials(tokenParam.SecurityKey, SecurityAlgorithms.HmacSha256)
            );

            string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            return jwtToken;
        }

        /// <summary>
        /// 获取token中的附带的数据
        /// </summary>
        /// <param name="tokenParam"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static JwtPayload GetTokenPayloads(string token)
        {
            // 校验并解析token
            var tokenInfo = new JwtSecurityTokenHandler().ReadJwtToken(token);//validatedToken:解密后的对象
            return tokenInfo.Payload;
        }
    }
}
