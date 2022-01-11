using eFMS.API.Catalogue.Infrastructure.Filters;
using eFMS.API.ReportData.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Reflection;

namespace eFMS.API.ReportData.Infrastructure
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddConfigureSetting(this IServiceCollection service, IConfiguration configuration)
        {
            service.Configure<APIs>(options =>
            {
                options.HostStaging = configuration.GetSection("APIs:HostStating").Value;
                options.CatalogueAPI = configuration.GetSection("APIs:CatalogueAPI").Value;
                options.AccountingAPI = configuration.GetSection("APIs:AccountingAPI").Value;
                options.SettingAPI = configuration.GetSection("APIs:SettingAPI").Value;
                options.FileManagementAPI = configuration.GetSection("APIs:FileManagementAPI").Value;
            });
            return service;
        }

        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddUserManager();
            //services.AddTransient<IClaimsTransformation, ClaimsExtender>();

            services.AddMvcCore()
                .AddAuthorization();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Authentication:Authority"];
                options.RequireHttpsMetadata = bool.Parse(configuration["Authentication:RequireHttpsMetadata"]);
                options.Audience = configuration["Authentication:ApiName"];
                options.SaveToken = true;
            });
            return services;
        }
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(
                options =>
                {
                    //options.DescribeAllEnumsAsStrings();
                    var provider = services.BuildServiceProvider()
                    .GetRequiredService<IApiVersionDescriptionProvider>();
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    if (xmlPath != null)
                    {
                        options.IncludeXmlComments(xmlPath);
                    }

                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerDoc(
                            description.GroupName,
                            new Info()
                            {
                                Title = $"eFMS Report Data API {description.ApiVersion}",
                                Version = description.ApiVersion.ToString(),
                                Description = "eFMS Report Data API Document"
                            });
                    }
                    //options.DocumentFilter<SwaggerAddEnumDescriptions>();

                    //options.AddSecurityDefinition("oauth2", new OAuth2Scheme
                    //{
                    //    Flow = "implicit", // just get token via browser (suitable for swagger SPA)
                    //    AuthorizationUrl = "",
                    //    Scopes = new Dictionary<string, string> { { "apimobile", "Report Data API" } }
                    //});
                    //options.OperationFilter<AuthorizeCheckOperationFilter>(); // Required to use access token
                    options.DocumentFilter<SwaggerAddEnumDescriptions>();
                    var security = new Dictionary<string, IEnumerable<string>>{
                        { "Bearer", new string[] { }},
                    };

                    options.AddSecurityDefinition("Bearer", new ApiKeyScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = "header",
                        Type = "apiKey"
                    });
                    options.OperationFilter<AuthorizeCheckOperationFilter>(); // Required to use access token
                });
            return services;
        }

        public static IServiceCollection AddCrossOrigin(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder
                            .WithHeaders("accept", "content-type", "origin", "x-custom-header")
                            .AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });
            return services;
        }
    }
}
