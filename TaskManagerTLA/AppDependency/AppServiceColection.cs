﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagerTLA.BLL.Mapper;
using TaskManagerTLA.BLL.Services.IdentityService;
using TaskManagerTLA.BLL.Services.IdentityService.Interfaces;
using TaskManagerTLA.BLL.Services.TaskService;
using TaskManagerTLA.BLL.Services.TaskService.Interfaces;
using TaskManagerTLA.DAL.EF;
using TaskManagerTLA.DAL.Entities;
using TaskManagerTLA.DAL.Identity.Entities;
using TaskManagerTLA.DAL.Repositories.IdentityRep;
using TaskManagerTLA.DAL.Repositories.Interfaces;
using TaskManagerTLA.DAL.Repositories.TaskRep;
using TaskManagerTLA.DAL.UnitOfWork.IdentityUnitOfWork;
using TaskManagerTLA.DAL.UnitOfWork.IdentityUnitOfWork.Interfaces;
using TaskManagerTLA.DAL.UnitOfWork.TaskUnitOfWork;
using TaskManagerTLA.DAL.UnitOfWork.TaskUnitOfWork.Interfaces;

namespace TaskManagerTLA.MyDependency
{
    public static class AppServiceColection
    {
        public static IServiceCollection AddDataBaseService(this IServiceCollection services)
        {
            // Repository
            services.AddTransient<IRepository<ApplicationUser, string>, UsersRepository>();
            services.AddTransient<IRepository<ApplicationRole, string>, RolesRepository>();
            services.AddTransient<IRepository<AssignedTask, int>, AssignedTaskRepository>();
            services.AddTransient<IRepository<GlobalTask, int>, GlobalTasksRepository>();
            // UnitOfWork
            services.AddTransient<IRoleUnit, RoleUnit>();
            services.AddTransient<IUserUnit, UserUnit>();
            services.AddTransient<IAssignedTaskUnit, AssignedTaskUnit>();
            services.AddTransient<IGlobalTaskUnit, GlobalTaskUnit>();
            return services;
        }

        public static IServiceCollection AddEFService(this IServiceCollection services, IConfiguration configuration)
        {
            string connection = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<TaskContext>(options => options.UseSqlServer(connection));
            services.AddDbContext<IdentityContext>(options => options.UseSqlServer(connection));
            services.AddIdentity<ApplicationUser, ApplicationRole>().AddEntityFrameworkStores<IdentityContext>();
            return services;
        }

        public static IServiceCollection AddIdentityService(this IServiceCollection services)
        {
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IRoleService, RoleService>();
            services.AddTransient<IUserRoleService, UserRoleService>();
            return services;
        }

        public static IServiceCollection AddTaskService(this IServiceCollection services)
        {
            services.AddTransient<IAssignedTaskService, AssignedTaskService>();
            services.AddTransient<IGlobalTaskService, GlobalTaskService>();
            return services;
        }

        public static IServiceCollection AddMappingService(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile));
            return services;
        }
    }
}