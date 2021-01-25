using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using OMM.Desktop.Data;
using OMM.Desktop.Data.OmmApi;
using OMM.Desktop.Data.OsuDataProvider;
using OMM.Desktop.Data.Settings;
using OsuMemoryDataProvider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OMM.Desktop
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages(options =>
            {
                options.RootDirectory = "/Content";
            });
            services.AddServerSideBlazor();
            services.AddHostedService<OsuDataProvider>();
            services.AddSingleton<IOsuDataService, OsuDataService>();
            services.AddSingleton<OsuDataProvider>();
            services.AddSingleton<OmmApiService>();
            services.AddSingleton<ISettings, SettingsService>();
            services.AddSingleton<CircuitHandler, TrackingCircuitHandler>();
            services.AddHttpClient("OmmApi", client =>
            {
                client.BaseAddress = new Uri(Configuration.GetSection("Omm").GetValue<string>("Url"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISettings settings)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            if (!Directory.Exists(settings.UserSettings.SongFolderPath))
            {
                settings.UserSettings.SongFolderPath = @"C:\";
            }

                app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(settings.UserSettings.SongFolderPath),
                RequestPath = "/Songs"
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
