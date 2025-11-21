using System.Reflection;
using Calcpad.WebApi.Configs;
using Calcpad.WebApi.Models.Base;
using Calcpad.WebApi.Utils.Web;
using Calcpad.WebApi.Utils.Web.Filters;
using Calcpad.WebApi.Utils.Web.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi;
using MongoDB.Bson;

// set the current directory to the base directory
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = WebApplication.CreateBuilder(args);

// add environment
var storageConfig = new AppSettings<StorageConfig>(builder.Configuration).Value;
Environment.SetEnvironmentVariable(storageConfig.Environment, storageConfig.Root);

// Add services to the container.
var services = builder.Services;

// replace the default logging with log4net
services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddLog4Net();
});

//  Add http logging middleware
services.AddHttpLogging(logging =>
{
    logging.LoggingFields =
        HttpLoggingFields.RequestProperties
        | HttpLoggingFields.RequestHeaders
        | HttpLoggingFields.ResponseHeaders;
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

// httpClient
services.AddHttpClient();

// Add hyphen-case routing
services.SetupSlugifyCaseRoute();

services.AddMongoDB(builder.Configuration);

// 添加 HttpContextAccessor，以供 service 获取当前请求的用户信息
services.AddHttpContextAccessor();

// log4net: https://github.com/huorswords/Microsoft.Extensions.Logging.Log4Net.AspNetCore/blob/develop/samples/Net8.0/WebApi/log4net.config
// ref：https://github.com/huorswords/Microsoft.Extensions.Logging.Log4Net.AspNetCore/blob/develop/samples/Net8.0/WebApi/Program.cs
// map the log level from logging to log4net
builder.AttachLevelToLog4Net();

// 配置 jwt 验证
var tokenParams = new AppSettings<TokenParamsConfig>(builder.Configuration).Value;
services.AddJWTAuthentication(tokenParams.SecurityKey);

// 关闭参数自动检验
services.Configure<ApiBehaviorOptions>(o =>
{
    o.SuppressModelStateInvalidFilter = true;
});

// 加载本机服务
services.AddServices();

var mvcBuilder = builder.Services.AddControllers(options =>
{
    options.Filters.Add(new KnownExceptionFilter());
    options.Filters.Add(new TokenExpiredFilter());
});
mvcBuilder.AddNewtonsoftJson(x =>
{
    x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    x.SerializerSettings.Converters.Add(new ObjectIdNewtonsoftConverter());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

services.AddSwaggerGen(
    new OpenApiInfo()
    {
        Title = "Calcpad API",
        Contact = new OpenApiContact()
        {
            Name = "uyoufu",
            Url = new Uri("https://uyoufu.uzoncloud.com"),
            Email = "260827400@qq.com"
        }
    }
);

var app = builder.Build();

app.UseDefaultFiles();

// 设置 public 目录为静态文件目录
var publicPath = $"{storageConfig.Root}/public";
Directory.CreateDirectory(publicPath);
app.UseStaticFiles(
    new StaticFileOptions()
    {
        FileProvider = new PhysicalFileProvider(Path.GetFullPath(publicPath)),
        RequestPath = "/public"
    }
);

// 跨域
app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
