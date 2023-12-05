using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Amazon.SQS;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using OpenAI.Extensions;
using OpenAI.Interfaces;
using OpenAI.Managers;
using Stripe;
using Supabase;
using Utils.IO.Server;
using Utils.IO.Server.Extensions;
using Utils.IO.Server.Queues;
using Utils.IO.Server.Services;

var builder = WebApplication.CreateBuilder(args);
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.Configure<HostOptions>(hostOptions =>
{
    hostOptions.BackgroundServiceExceptionBehavior =
        BackgroundServiceExceptionBehavior.Ignore;
    hostOptions.ShutdownTimeout = TimeSpan.FromSeconds(60);
});

var customConfig = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .Build();

// Bind the custom configuration section to a strongly typed object
var awsConfig = new AWSConfiguration();
customConfig.GetSection("AWS").Bind(awsConfig);
builder.Services.AddSingleton<AWSConfiguration>(awsConfig);
var stripeConfig = new Utils.IO.Server.StripeConfiguration();
customConfig.GetSection("Stripe").Bind(stripeConfig);
builder.Services.AddSingleton<Utils.IO.Server.StripeConfiguration>(stripeConfig);
var supabaseConfig = new SupabaseConfiguration();
customConfig.GetSection("Supabase").Bind(supabaseConfig);
builder.Services.AddSingleton<SupabaseConfiguration>(supabaseConfig);
var gptConfig = new GptConfiguration();
customConfig.GetSection("Gpt").Bind(gptConfig);
builder.Services.AddSingleton<GptConfiguration>(gptConfig);

var options = new SupabaseOptions
{
    AutoRefreshToken = true,
    AutoConnectRealtime = true,
    // SessionHandler = new SupabaseSessionHandler() <-- This must be implemented by the developer
};

//builder.Services.AddTransient<IGptProcessor, GptProcessor>();

builder.Services.AddOpenAIService(settings => 
{ 
    settings.ApiKey = gptConfig.ApiKey; 
    settings.Organization = gptConfig.Organization; 
});
builder.Services.AddAWSService<IAmazonS3>(new AWSOptions
{
    Region = Amazon.RegionEndpoint.USEast2
});
builder.Services.AddAWSService<IAmazonDynamoDB>(new AWSOptions
{
    Region = Amazon.RegionEndpoint.USEast2
});
builder.Services.AddAWSService<IAmazonSQS>(new AWSOptions
{
    Region = Amazon.RegionEndpoint.USEast2
});
builder.Services.AddScoped<ISqsPublisher, SqsPublisher>();
builder.Services.AddTransient<IS3Service, S3Service>();
builder.Services.AddTransient<IDynamoDBService, DynamoDBMutateService>();
builder.Services.AddTransient<OpenAIService>();
builder.Services.AddSingleton(stripeConfig);
builder.Services.AddSingleton(supabaseConfig);
builder.Services.AddSingleton(provider => new Supabase.Client(supabaseConfig.Url, supabaseConfig.AnonKey, options));
/*
builder.Services.AddSingleton<ISqsMessageChannel, SqsMessageChannel>();
builder.Services.AddSingleton<ISqsMessageDeleter, SqsMessageDeleter>();
builder.Services.AddSingleton<ISqsMessageQueue, SqsMessageQueue>();
*/
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Utils IO Server",
        Version = "v1",
    });
});
builder.Services.AddHealthChecks();
builder.Services.AddHostedService<GptProcessingQueue>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSwaggerUI",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
var app = builder.Build();
app.Services.SaveSwaggerJson();
app.MapHealthChecks("/health");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowSwaggerUI");
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Utils IO Server");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
