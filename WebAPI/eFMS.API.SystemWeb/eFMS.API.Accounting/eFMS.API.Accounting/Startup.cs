using AutoMapper;
using eFMS.API.Accounting.Infrastructure;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common.Globals;
using eFMS.API.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace eFMS.API.Accounting
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
            services.AddCors(o => o.AddPolicy("AllowOrigin", builder => {
                builder
                .SetIsOriginAllowed((host) => true)
                .WithOrigins("https://localhost:4200");

            }));
            services.AddAutoMapper();
            services.AddSession();
            services.AddMvc().AddDataAnnotationsLocalization().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddMvcCore().AddVersionedApiExplorer(o => o.GroupNameFormat = "'v'VVV").AddAuthorization();
            services.AddMemoryCache();
            services.AddInfrastructure<LanguageSub>(Configuration);
            ServiceRegister.Register(services);
            services.AddCustomSwagger();
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
            app.UseCors("AllowOrigin");
            app.UseCors("AllowAllOrigins");
            app.UseAuthentication();
            app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            app.UseSession();
            app.UseMvc();
            app.UseStaticFiles();
            //app.UseRequestLocalization();
        }
    }
}
