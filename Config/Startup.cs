﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Kicksharp.Services;
using Mock = Kicksharp.Services.Mock;

namespace Kicksharp.Config
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; set; }
        private IHostingEnvironment HostingEnv { get; set;}

        public Startup(IHostingEnvironment env)
        {
            this.HostingEnv = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                // .AddJsonFile("appsettings.json")
                // .AddJsonFile($"appsettings.{env.EnvironmentName}.json")
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddRouting( options => options.LowercaseUrls = true )
                .AddMvc( options => options.Conventions.Add(new FeatureConvention()))
                .AddRazorOptions( options => 
                {
                    // {0} - Action Name
                    // {1} - Controller Name
                    // {2} - Area Name
                    // {3} - Feature Name
                    options.ViewLocationFormats.Clear();
                    options.ViewLocationFormats.Add("/Web/Views/{3}/View.cshtml");
                    options.ViewLocationFormats.Add("/Web/Views/{3}/{0}/View.cshtml");
                    options.ViewLocationFormats.Add("/Web/Views/{3}/{0}.cshtml");

                    options.ViewLocationExpanders.Add(new FeatureViewLocationExpander());
                });

            if (HostingEnv.IsDevelopment())
            {
                InjectMockServices(services, new Mock.Mocker(this.HostingEnv.ContentRootPath));
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();

        }

        private void InjectMockServices (IServiceCollection services, Mock.IMocker mocker)
        {
            services
                .AddSingleton<Mock.IMocker>(mocker)
                .AddTransient<ISettingsService, Mock.SettingsService>();
        }
    }
}
