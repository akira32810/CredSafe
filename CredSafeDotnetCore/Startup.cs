using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CredSafeDotnetCore.Context;
using CredSafeDotnetCore.BusinessClass;
using CredSafeDotnetCore.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

namespace CredSafeDotnetCore
{
    public class Startup
    {
        // public Startup(IConfiguration configuration)
        public static string FileLocation;
      public Startup(IHostingEnvironment env)
        {
              //Configuration = configuration;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

       public IConfigurationRoot Configuration { get;  }
     // public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public  void ConfigureServices(IServiceCollection services)
        {
        //allow cross domain access
    
        services.AddCors(options =>
        {
        options.AddPolicy("CorsPolicy",
            builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials() );
        });
    
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddScoped<CRContext>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            Startup.FileLocation = Configuration.GetSection("Settings").GetSection("FileLocation").Value;

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //call the Cors policy
            app.UseCors("CorsPolicy");

            app.UseMvc();
        }
    }
}
