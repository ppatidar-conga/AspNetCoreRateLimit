using AspNetCoreRateLimit.Redis;
using Ben.Diagnostics;
using Conga.Platform.Licencing;
using Conga.Platform.Licencing.Interfaces;
using Conga.Platform.RateLimiting.LimitCustomization;
//using Customization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System.Collections.Generic;

namespace AspNetCoreRateLimit.Demo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Conga.Platform.Licencing.LicencingRegistration.Register(services, Configuration);
            Conga.Platform.RateLimiting.RateLimitingRegistration.Register(services, Configuration);


            //Registrations(services);

            services.AddMvc((options) =>
            {
                options.EnableEndpointRouting = false;

            }).AddNewtonsoftJson();
           
        }

        private void Registrations(IServiceCollection services)
        {
            services.AddSingleton<ILicencingManager, LicencingManager>();

           
            // configure client rate limiting middleware
            services.Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimiting"));
            services.Configure<ClientRateLimitPolicies>(Configuration.GetSection("ClientRateLimitPolicies"));

            // register stores
           
            var redisOptions = ConfigurationOptions.Parse(Configuration["ConnectionStrings:Redis"]);
            services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(redisOptions));
            #region redis limiting registration
            
            services.AddSingleton<IClientPolicyStore, DistributedCacheClientPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
            services.AddSingleton<IProcessingStrategy, CongaRedisProcessingStrategy>();

            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseBlockingDetection();

            //app.UseIpRateLimiting();
            // app.UseClientRateLimiting();
            app.UseMiddleware<CongaClientRateLimitMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseDefaultFiles(new DefaultFilesOptions 
            { 
                DefaultFileNames = new List<string> { "index.html" } 
            });
            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}