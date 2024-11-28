using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MimeDetective;
using MimeDetective.Definitions.Licensing;
using Stela_file_server.App.Service;
using Stela_file_server.Core.IService;
using Stela_file_server.Infrastructure.Service;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);


ConfigureServices(builder.Services);
var app = builder.Build();
app = ConfigureApplication(app);
app.Run();

string GetEnvVar(string name) => Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");
void ConfigureServices(IServiceCollection services)
{
    var rabbitMqHostname = GetEnvVar("RABBITMQ_HOSTNAME");
    var rabbitMqUsername = GetEnvVar("RABBITMQ_USERNAME");
    var rabbitMqPassword = GetEnvVar("RABBITMQ_PASSWORD");

    var rabbitMqProfileImageQueue = GetEnvVar("RABBITMQ_PROFILE_IMAGE_QUEUE_NAME");
    var rabbitMqAdditionalServiceImageQueue = GetEnvVar("RABBITMQ_ADDITIONAL_SERVICE_IMAGE_QUEUE_NAME");
    var rabbitMqMemorialImageQueue = GetEnvVar("RABBITMQ_MEMORIAL_IMAGE_QUEUE_NAME");
    var rabbitMqPortfolioMemorialImageQueue = GetEnvVar("RABBITMQ_PORTFOLIO_MEMORIAL_IMAGE_QUEUE_NAME");
    var rabbitMqMaterialImageQueue = GetEnvVar("RABBITMQ_MATERIAL_IMAGE_QUEUE_NAME");

    var jwtSecret = GetEnvVar("JWT_AUTH_SECRET");
    var jwtIssuer = GetEnvVar("JWT_AUTH_ISSUER");
    var jwtAudience = GetEnvVar("JWT_AUTH_AUDIENCE");

    var fileInspector = new ContentInspectorBuilder()
    {
        Definitions = new MimeDetective.Definitions.CondensedBuilder()
        {
            UsageType = UsageType.PersonalNonCommercial
        }.Build()
    }.Build();

    services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
    });

    services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Optimal;
    });

    services.Configure<BrotliCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Optimal;
    });


    services.AddControllers(e =>
    {
        e.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
    });

    ConfigureJwtAuthentication(services, jwtSecret, jwtIssuer, jwtAudience);
    ConfigureSwagger(services);

    services.AddAuthorization();

    services.AddSingleton<IFileUploaderService, LocalFileUploaderService>();
    services.AddSingleton<IJwtService, JwtService>();
    services.AddSingleton<INotifyService, RabbitMqNotifyService>(sp =>
        new RabbitMqNotifyService(
            rabbitMqHostname,
            rabbitMqUsername,
            rabbitMqPassword,
            rabbitMqProfileImageQueue,
            rabbitMqAdditionalServiceImageQueue,
            rabbitMqMemorialImageQueue,
            rabbitMqPortfolioMemorialImageQueue,
            rabbitMqMaterialImageQueue
        ));
    services.AddSingleton(fileInspector);
}

WebApplication ConfigureApplication(WebApplication app)
{
    app.UseResponseCompression();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapGet("/", () => "File server work");

    return app;
}

void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "stela_file_server_api",
            Description = "Api",
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Bearer auth scheme",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });

        options.OperationFilter<SecurityRequirementsOperationFilter>();

        options.EnableAnnotations();
    });
}

void ConfigureJwtAuthentication(IServiceCollection services, string secret, string issuer, string audience)
{
    services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidIssuer = issuer,
            ValidAudience = audience
        });
}