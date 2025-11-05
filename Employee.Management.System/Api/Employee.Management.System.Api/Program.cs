using Autofac;
using Autofac.Extensions.DependencyInjection;
using Employee.Management.System.Api.Container;
using Employee.Management.System.Common.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using NLog;
using NLog.Web;
using System.Text;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

    var builder = WebApplication.CreateBuilder(args);

    // Configuration
    var configBuilder = new ConfigurationBuilder()
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();
    builder.Configuration.AddConfiguration(configBuilder.Build());

    // NLog setup
    var logContext = new LogContext("Main");
    logContext.Start();
    var apiLogFilePath = builder.Configuration.GetSection("LogConfig").GetValue<string>("LogFilePath");
    var id = builder.Configuration.GetSection("LogConfig").GetValue<string>("Id");
    var logFilePath = string.IsNullOrEmpty(apiLogFilePath) ? $"{Environment.CurrentDirectory}" : apiLogFilePath;
    GlobalDiagnosticsContext.Set("LogFilePath", logFilePath);
    GlobalDiagnosticsContext.Set("MachineName", Environment.MachineName);
    GlobalDiagnosticsContext.Set("Id", id);
    LogHelper.Information(new LogContext("Startup.Startup"), $"Running environment {builder.Environment.EnvironmentName}");
    LogHelper.Information(new LogContext("Startup.Startup"), $"Log Directory : {logFilePath}");
    LogHelper.Information(new LogContext("Startup.Startup"), $"Log Machine-Id : {Environment.MachineName}-{id}");

    // Add services to the container.
    var allowedOrigins = builder.Configuration.GetValue<string>("AllowedOrigins")?.Split(",");
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
        {
            if (allowedOrigins != null && allowedOrigins?.Length > 0)
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
            else
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
        });
    });

    var secretKey = Encoding.UTF8.GetBytes(builder.Configuration["Authentication:SecretKey"] ?? string.Empty);
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Authentication:Domain"],
                ValidAudience = builder.Configuration["Authentication:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Employee Management System Api",
            Description = "API documentation with pre-set JWT authentication"
        });

        // JWT Bearer setup for Swagger
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
        };

        c.AddSecurityDefinition("Bearer", securityScheme);

        var securityRequirement = new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        };

        c.AddSecurityRequirement(securityRequirement);
    });

    builder.Services.AddSwaggerGenNewtonsoftSupport();

    builder.Services.AddControllers()
        .AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        });

    // Autofac configuration
    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
    builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        containerBuilder.RegisterModule(new AutofacModule(builder.Configuration));
    });

    // Configure NLog as the logging provider
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee Management System Api V1");
        });
    }
    else
    {
        app.UseHsts();
    }

    app.UseCors(MyAllowSpecificOrigins);
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseEndpoints(endpoints =>
    {
        endpoints?.MapControllers();
    });

    app.Run();

    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    logContext.Stop();
    LogHelper.Information(logContext, "Completed");
    NLog.LogManager.Shutdown();
}
catch (Exception ex)
{
    // NLog: catch setup errors
    logger.Error(ex, "Stopped program because of exception");
    throw;
}
