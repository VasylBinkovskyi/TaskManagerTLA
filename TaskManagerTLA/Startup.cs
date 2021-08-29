using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagerTLA.BLL.BisnesLogic;
using TaskManagerTLA.BLL.Interfaces;
using TaskManagerTLA.BLL.Services;
using TaskManagerTLA.DAL.EF;
using TaskManagerTLA.DAL.Identity;
using TaskManagerTLA.DAL.Identity.Interfaces;
using TaskManagerTLA.DAL.Interfaces;
using TaskManagerTLA.DAL.Repositories;


namespace TaskManagerTLA
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            string connection = Configuration.GetConnectionString("DefaultConnection");
            services.AddTransient<IUnitOfWork>(x => new EFUnitOfWork(connection));
            services.AddTransient<ITaskService, TaskService>();

            services.AddDbContext<IdentityContext>(options => options.UseSqlServer(Configuration.GetConnectionString("IdentityConnection")));
            services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<IdentityContext>();
            services.AddTransient<IUnitOfWorkIdentity, UnitOfWorkIdentity>();
            services.AddTransient<IIdentityServices, IdentityServices>();
            services.AddTransient<IHomePageGreeting, HomePageGreeting>();

            services.AddControllersWithViews();

        }

  
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
