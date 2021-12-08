using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AutoMapper;
using Calcpad.web.Mapping;
using Calcpad.web.Data;
using Calcpad.web.Data.Services;
using Calcpad.web.Services;
using Calcpad.web.Data.Models;
using Calcpad.web.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Calcpad.web.Data.Seed;

namespace Calcpad.web
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
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
                    Configuration.GetConnectionString("CalcpadDBConnection")));
            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>(),
                               AppDomain.CurrentDomain.GetAssemblies());

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddControllersWithViews(options =>
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
                options.SuppressXFrameOptionsHeader = false;
            });
            services.AddRazorPages();
            services.AddSignalR();
            services.AddTransient<IParserService, ParserService>();
            services.AddTransient<ICalculatorService, CalculatorService>();
            services.AddTransient<IOpenXmlService, OpenXmlService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IWorksheetService, WorksheetService>();
            services.AddScoped<ITopicService, TopicService>();
            services.AddScoped<IArticleService, ArticleService>();
            services.AddSingleton<IReCaptchaService, ReCapthaService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                using (var serviceScope = app.ApplicationServices.CreateScope())
                {
                    var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    dbContext.Database.Migrate();
                    new ApplicationDbContextSeeder().SeedAsync(dbContext, serviceScope.ServiceProvider).GetAwaiter().GetResult();
                }
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
 
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "area",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "slug",
                    pattern: "{controller}/{id:int:min(1)}/{slug}",
                    defaults: new { action = "Index" });

                endpoints.MapControllerRoute(
                    name: "id",
                    pattern: "{controller}/{id:int:min(1)}",
                    defaults: new { action = "Index" });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();

                endpoints.MapHub<CalculatorHub>("/calculate");
            });
        }
    }
}
