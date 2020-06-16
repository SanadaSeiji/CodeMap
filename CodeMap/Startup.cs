using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.Extensions.Logging;
using Prometheus;
using System;
using CodeMap.Models;
using System.Collections.Generic;

namespace CodeMap
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var config = GetEnvironmentVariables();
            EnvironmentVarValidation(config);
            

            services.AddOptions();
            services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            }));

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("CorsPolicy"));
            });

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "CodeMap", Version = "v1" });
                c.DescribeAllEnumsAsStrings();
            });

            services.AddMvc()
            .AddJsonOptions(
                options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


            services.AddSingleton(config);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();

            app.UseMetricServer();
            app.UseHttpMetrics();

            loggerFactory.AddConsole(LogLevel.Information);

            app.UseCors("CorsPolicy");
            app.UseAuthentication();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CodeMap");
            });
            app.UseExceptionHandler("/api/Error");
            // This must be before usemvc
            app.UseMvc();
        }

        private Config GetEnvironmentVariables()
        {
            return new Config()
            {
                AccessKey = Environment.GetEnvironmentVariable("ACCESS"), // or using local var, Configuration[AWS:ACCESS]
                SecretKey = Environment.GetEnvironmentVariable("SECRET"),
                Region = Environment.GetEnvironmentVariable("REGION"),
                Bucket = Environment.GetEnvironmentVariable("BUCKET"),
                DynamorTable = Environment.GetEnvironmentVariable("DYNAMOTABLE"),
                Token = Environment.GetEnvironmentVariable("TOKEN"),
                ServiceBase = Environment.GetEnvironmentVariable("SERVICEBASE"),
                GitlabGroups = new List<string>() { "groupNameOnGit" }
            };
        }

        private void EnvironmentVarValidation(Config config)
        {
            CheckForNullOrEmpty("ACCESS", config.AccessKey);
            CheckForNullOrEmpty("SECRET", config.SecretKey);
            CheckForNullOrEmpty("BUCKET", config.Bucket);
            CheckForNullOrEmpty("DYNAMOTABLE", config.DynamorTable);
            CheckForNullOrEmpty("TOKEN", config.Token);
            CheckForNullOrEmpty("SERVICEBASE", config.ServiceBase);
        }

        private void CheckForNullOrEmpty(string varName, string varValue)
        {
            if (string.IsNullOrEmpty(varValue))
                throw new Exception("Missing environment variable "+varName);
        }
    }
}
