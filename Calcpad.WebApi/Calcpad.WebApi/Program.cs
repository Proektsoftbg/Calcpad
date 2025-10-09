using Calcpad.WebApi.Configs;
using Calcpad.WebApi.Models.Base;
using Calcpad.WebApi.Utils.Web;
using Calcpad.WebApi.Utils.Web.Filters;
using Calcpad.WebApi.Utils.Web.Swagger;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using System.Reflection;

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
    logging.LoggingFields = HttpLoggingFields.RequestProperties
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

// ��� HttpContextAccessor���Թ� service ��ȡ��ǰ������û���Ϣ
services.AddHttpContextAccessor();

// log4net: https://github.com/huorswords/Microsoft.Extensions.Logging.Log4Net.AspNetCore/blob/develop/samples/Net8.0/WebApi/log4net.config
// ref��https://github.com/huorswords/Microsoft.Extensions.Logging.Log4Net.AspNetCore/blob/develop/samples/Net8.0/WebApi/Program.cs
// map the log level from logging to log4net
builder.AttachLevelToLog4Net();

// ���� jwt ��֤
var tokenParams = new AppSettings<TokenParamsConfig>(builder.Configuration).Value;
services.AddJWTAuthentication(tokenParams.SecurityKey);

// �رղ����Զ�����
services.Configure<ApiBehaviorOptions>(o =>
{
    o.SuppressModelStateInvalidFilter = true;
});

// ���ر�������
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
builder.Services.AddSwaggerGen(c =>
{
    c.MapType<ObjectId>(() => new OpenApiSchema { Type = "string", Format = "hexadecimal" });

    var xmlFile = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
    if(File.Exists(xmlFile))
    {
        c.IncludeXmlComments(xmlFile);
    }
    c.OperationFilter<DotNETSwaggerFilter>();
});

var app = builder.Build();

app.UseDefaultFiles();
// ���� public Ŀ¼Ϊ��̬�ļ�Ŀ¼
var publicPath = $"{storageConfig.Root}/public";
Directory.CreateDirectory(publicPath);
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.GetFullPath(publicPath)),
    RequestPath = "/public"
});

// ����
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
