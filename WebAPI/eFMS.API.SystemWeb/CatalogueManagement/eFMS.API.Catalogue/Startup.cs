using AutoMapper;
using eFMS.API.Catalogue.Infrastructure;
using eFMS.API.Catalogue.Infrastructure.Filters;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Catalogue.Service.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;

namespace SystemManagementAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper();
            services.AddMvc().AddDataAnnotationsLocalization().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddMvcCore().AddVersionedApiExplorer(o => o.GroupNameFormat = "'v'VVV").AddAuthorization();
            services.AddMemoryCache();
            ServiceRegister.Register(services);
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
            //services.AddCustomAuthentication(Configuration);
            services.AddApiVersioning(config =>
            {
                config.ReportApiVersions = true;
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.ApiVersionReader = new HeaderApiVersionReader("api-version");
            });

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddJsonLocalization(opts => opts.ResourcesPath = Configuration["LANGUAGE_PATH"]);
            //Multiple language setting
            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("vi-VN")
            };

            var localizationOptions = new RequestLocalizationOptions()
            {
                DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };

            localizationOptions.RequestCultureProviders = new[]
            {
                 new RouteDataRequestCultureProvider()
                 {
                     RouteDataStringKey = "lang",
                     Options = localizationOptions
                 }
            };

            services.AddSingleton(localizationOptions);
            services.AddSwaggerGen(
                options =>
                {
                    var provider = services.BuildServiceProvider()
                    .GetRequiredService<IApiVersionDescriptionProvider>();

                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerDoc(
                            description.GroupName,
                            new Info()
                            {
                                Title = $"eFMS Catalogue API {description.ApiVersion}",
                                Version = description.ApiVersion.ToString(),
                                Description = "eFMS Catalogue API Document"
                            });
                    }
                    //options.DocumentFilter<SwaggerAddEnumDescriptions>();

                    options.AddSecurityDefinition("oauth2", new OAuth2Scheme
                    {
                        Flow = "implicit", // just get token via browser (suitable for swagger SPA)
                        AuthorizationUrl = "",
                        Scopes = new Dictionary<string, string> { { "apimobile", "Catalogue API" } }
                    });
                    options.DocumentFilter<SwaggerAddEnumDescriptions>();
                    options.OperationFilter<AuthorizeCheckOperationFilter>(); // Required to use access token
                });
        }
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory,
            IHostingEnvironment env, IApiVersionDescriptionProvider provider)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "text/plain";
                        var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
                        if (errorFeature != null)
                        {
                            var logger = loggerFactory.CreateLogger("Global exception logger");
                            logger.LogError(500, errorFeature.Error, errorFeature.Error.Message);
                        }

                        await context.Response.WriteAsync("There was an error");
                    });
                });
            }


            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                string swaggerJsonBasePath = string.IsNullOrWhiteSpace(options.RoutePrefix) ? "." : "..";
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"{swaggerJsonBasePath}/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                }
            });

            app.UseCors("AllowAllOrigins");
            //app.UseCors("CorsPolicy");
            //ConfigureAuth(app);

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            app.UseMvc();
        }
        //protected virtual void ConfigureAuth(IApplicationBuilder app)
        //{
        //    if (Configuration.GetValue<bool>("UseLoadTest"))
        //    {
        //        app.UseMiddleware<ByPassAuthMiddleware>();
        //    }

        //    app.UseAuthentication();
        //}
    }
    //static class CustomExtensionsMethods
    //{
    //    public static IServiceCollection AddCustomMvc(this IServiceCollection services)
    //    {
    //        services.AddCors(options =>
    //        {
    //            options.AddPolicy("CorsPolicy",
    //                builder => builder.AllowAnyOrigin()
    //                .AllowAnyMethod()
    //                .AllowAnyHeader()
    //                .AllowCredentials());
    //        });

    //        services.AddRouting(options => options.LowercaseUrls = true);
    //        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
    //        services.AddScoped<IUrlHelper>(implementationFactory =>
    //        {
    //            var actionContext = implementationFactory.GetService<IActionContextAccessor>().ActionContext;
    //            return new UrlHelper(actionContext);
    //        });
    //        // Lỗi không hiển thị hết data vì có quan hệ các bảng khác
    //        services.AddMvc().AddJsonOptions(options =>
    //        {
    //            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    //        });

    //        services.AddMvcCore().AddVersionedApiExplorer(o => o.GroupNameFormat = "'v'VVV");
    //        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
    //        services.AddApiVersioning(config =>
    //        {
    //            config.ReportApiVersions = true;
    //            config.AssumeDefaultVersionWhenUnspecified = true;
    //            config.DefaultApiVersion = new ApiVersion(1, 0);
    //            config.ApiVersionReader = new HeaderApiVersionReader("api-version");
    //        });
    //        return services;
    //    }

    //    public static IServiceCollection AddCustomDbContext(this IServiceCollection services, IConfiguration configuration)
    //    {
    //        //services.AddDbContext<DNTDataContext>(options => options.UseSqlServer(configuration["ConnectStrings:Default"]));
    //        services.AddEntityFrameworkSqlServer()
    //            .AddDbContext<eFMSDataContext>(options =>
    //            {
    //                options.UseSqlServer(configuration["ConnectionString"],
    //                    sqlServerOptionsAction: sqlOptions =>
    //                    {
    //                        //sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
    //                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
    //                    });
    //            },
    //            ServiceLifetime.Scoped  //Showing explicitly that the DbContext is shared across the HTTP request scope (graph of objects started in the HTTP request)
    //            );

    //        return services;
    //    }

    //    public static IServiceCollection AddCustomSwagger(this IServiceCollection services, IConfiguration configuration)
    //    {
    //        services.AddSwaggerGen(options =>
    //            {
    //                options.DescribeAllEnumsAsStrings();

    //                var provider = services.BuildServiceProvider()
    //                .GetRequiredService<IApiVersionDescriptionProvider>();

    //                foreach (var description in provider.ApiVersionDescriptions)
    //                {
    //                    options.SwaggerDoc(
    //                        description.GroupName,
    //                        new Info()
    //                        {
    //                            Title = $"System Managemet API {description.ApiVersion}",
    //                            Version = description.ApiVersion.ToString(),
    //                            Description = "System Managemet API Document"
    //                        });

    //                }
    //                //Add authentication
    //                options.AddSecurityDefinition("oauth2", new OAuth2Scheme
    //                {
    //                    Type = "oauth2",
    //                    Flow = "implicit",
    //                    AuthorizationUrl = $"{configuration.GetValue<string>("IdentityUrlExternal")}/connect/authorize",
    //                    TokenUrl = $"{configuration.GetValue<string>("IdentityUrlExternal")}/connect/token",
    //                    Scopes = new Dictionary<string, string>()
    //                        {
    //                            { "systemmanagementapi", "Managemet API" }
    //                        }
    //                });
    //                options.OperationFilter<AuthorizeCheckOperationFilter>();
    //            });
    //        return services;
    //    }

    //    public static IServiceCollection AddCustomIntegrations(this IServiceCollection services, IConfiguration configuration)
    //    {
    //        services.AddAutoMapper();
    //        ServiceRegister.Register(services);
    //        services.AddOptions();
    //        return services;
    //    }
    //    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    //    {
    //        // prevent from mapping "sub" claim to nameidentifier.
    //        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

    //        var identityUrl = configuration.GetValue<string>("IdentityUrl");

    //        services.AddAuthentication(options =>
    //        {
    //            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    //        }).AddJwtBearer(options =>
    //        {
    //            options.Authority = identityUrl;
    //            options.RequireHttpsMetadata = false;
    //            options.Audience = "systemmanagementapi";
    //        });

    //        return services;
    //    }
    //}
}
