using Calcpad.WebApi.Configs;
using Calcpad.WebApi.Models;
using Calcpad.WebApi.Models.Base;
using Calcpad.WebApi.Services.Hosted.Base;
using MongoDB.Driver.Linq;

namespace Calcpad.WebApi.Services.Hosted
{
    public class InitializeAIPrompts(AppSettings<AIConfig> config, MongoDBContext db)
        : IHostedServiceStartup
    {
        public int Order => 0;

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var prompts = config.Value.Prompts;
            if (prompts == null)
                return;

            foreach (var prompt in prompts)
            {
                var existOne = await db.AsQueryable<AIPromptModel>()
                    .Where(x => x.Name == prompt.Name)
                    .FirstOrDefaultAsync();
                if (existOne == null)
                {
                    // create new prompt
                    var newPrompt = new AIPromptModel
                    {
                        IsConfig = true,
                        Name = prompt.Name,
                        Prompt = prompt.Prompt
                    };
                    await db.InsertOneAsync(newPrompt);
                }
                else if (existOne.IsConfig)
                {
                    // update existing prompt
                    existOne.Prompt = prompt.Prompt;
                    await db.AsFluentMongo<AIPromptModel>()
                        .Where(x => x.Id == existOne.Id)
                        .Set(x => x.Prompt, prompt.Prompt)
                        .UpdateOneAsync();
                }
            }
        }
    }
}
