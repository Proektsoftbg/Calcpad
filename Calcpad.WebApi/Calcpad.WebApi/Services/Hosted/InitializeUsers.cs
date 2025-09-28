using Calcpad.WebApi.Configs;
using Calcpad.WebApi.Models;
using Calcpad.WebApi.Models.Base;
using Calcpad.WebApi.Services.Hosted.Base;
using Calcpad.WebApi.Utils.Encrypt;
using MongoDB.Driver;

namespace Calcpad.WebApi.Services.Hosted
{
    /// <summary>
    /// init users from config
    /// </summary>
    public class InitializeUsers(AppSettings<UsersConfig> users, MongoDBContext db) : IHostedServiceStartup
    {
        public int Order => 0;

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var userModels = users.Value.Select(x => new CalcpadUserModel()
            {
                Username = x.Username,
                Password = x.Password.ToMD5(),
                Roles = x.Roles,
            });

            foreach (var user in userModels)
            {
                await db.Collection<CalcpadUserModel>().UpdateOneAsync(Builders<CalcpadUserModel>.Filter.Eq(x => x.Username, user.Username), Builders<CalcpadUserModel>.Update
                        .SetOnInsert(x => x.Username, user.Username)
                        .SetOnInsert(x => x.Password, user.Password)
                        .SetOnInsert(x => x.Roles, user.Roles),
                    new UpdateOptions() { IsUpsert = true },
                    stoppingToken
                );
            }
        }
    }
}
