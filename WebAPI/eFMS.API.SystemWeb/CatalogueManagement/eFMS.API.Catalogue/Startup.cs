using AutoMapper;
using eFMS.API.Catalogue.Infrastructure;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace eFMS.API.Catalogue
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper();
            services.AddDistributedRedisCache(options =>
            {
                options.InstanceName = "Catalogue";
                options.Configuration = Configuration.GetConnectionString("Redis");
            });
            services.AddSession();
            services.AddAuthorize(Configuration);
            services.AddMvc().AddDataAnnotationsLocalization().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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
            services.AddApiVersioning(config =>
            {
                config.ReportApiVersions = true;
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.ApiVersionReader = new HeaderApiVersionReader("api-version");
            });

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddCulture(Configuration);
            services.AddSwagger(Configuration);
            services.Configure<Settings>(options =>
            {
                options.MongoConnection
                    = Configuration.GetSection("ConnectionStrings:MongoConnection").Value;
                options.MongoDatabase
                    = Configuration.GetSection("ConnectionStrings:Database").Value;
                options.RedisConnection
                    = Configuration.GetSection("ConnectionStrings:Redis").Value;
                options.eFMSConnection
                    = Configuration.GetSection("ConnectionStrings:eFMSConnection").Value;
            });
            //DbHelper.DbHelper.ConnectionString = ConfigurationExtensions.GetConnectionString(Configuration, "eFMSConnection");
            //DbHelper.DbHelper.MongoDBConnectionString = ConfigurationExtensions.GetConnectionString(Configuration, "mongoDB");
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
            app.UseAuthentication();
            app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            app.UseSession();
            app.UseMvc();
            //app.UseRequestLocalization();
        }
    }
}
