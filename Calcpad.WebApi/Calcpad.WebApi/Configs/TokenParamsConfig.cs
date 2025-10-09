using System;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Calcpad.WebApi.Configs
{
    /// <summary>
    /// 创建 token 的参数
    /// </summary>
    public class TokenParamsConfig
    {
        /// <summary>
        /// 过期时间
        /// ms
        /// </summary>
        public long Expire { get; set; } = 24 * 60 * 60 * 1000; // 默认 24 小时

        /// <summary>
        /// 过期时间，优先级最高
        /// 程序最终使用该值作为过期时间
        /// </summary>
        public DateTime ExpireDate { get; set; } = DateTime.MinValue;

        public DateTime GetExpireDate()
        {
            if (ExpireDate > DateTime.MinValue)
            {
                return ExpireDate;
            }

            return DateTime.UtcNow.AddMicroseconds(Expire);
        }

        /// <summary>
        /// token 的密钥
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// 签发者
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// 接收者
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        private SymmetricSecurityKey? _key;
        public SymmetricSecurityKey SecurityKey
        {
            get
            {
                if (_key == null)
                {
                    // use SHA256 to hash the secret to get a 256-bit key
                    byte[] keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(Secret));
                    _key = new(keyBytes);
                }
                return _key;
            }
        }
    }
}
