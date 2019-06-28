﻿using AutoMapper;
using GeekBurguer.Users.Polly;
using GeekBurguer.Users.Repository;
using GeekBurguer.Users.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GeekBurguer.Users
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment;

		public Startup(IConfiguration configuration, IHostingEnvironment env)
		{
			Configuration = configuration;
			HostingEnvironment = env;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var databasePath = "%DATABASEPATH%";
            var connection = Configuration.GetSection("Sqlite").Value
                .Replace(databasePath, HostingEnvironment.ContentRootPath);

            services.AddEntityFrameworkSqlite()
                .AddDbContext<UsersDbContext>(o => o.UseSqlite(connection));

            services.AddScoped<IFacialService, FacialService>();
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddSingleton<IUserRetrievedService, UserRetrievedService>();
            
            services.AddSingleton<ILogService, LogService>();

			// POlly
			services.AddPollyPolicies();

            //services.AddSwaggerGen(c =>
            //    c.SwaggerDoc("v1", new Info { Title = "GeekBurger Users", Version = "v1" })
            //);

            services.AddSwaggerGen(options =>
            {
                options.CustomOperationIds(e =>
                {
                    return $"{e.ActionDescriptor.RouteValues["controller"]}_{e.HttpMethod}{e.ActionDescriptor.Parameters?.Select(a => a.Name).ToString()}";
                });
                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "GeekBurger Users",
                    Description = "GeekBurger Users",
                    TermsOfService = "None"
                });

                //var xmlWebApiFile = Path.Combine(AppContext.BaseDirectory, $"GeekBurger.Users.xml");
                //if (File.Exists(xmlWebApiFile))
                //{
                //    options.IncludeXmlComments(xmlWebApiFile);
                //}

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env/*, UsersDbContext usersDbContext*/)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "GeekBurger Users")
            );

            app.UseMvc();

            using (var serviceScope = app
                .ApplicationServices
                .GetService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<UsersDbContext>();
                context.Database.EnsureCreated();
            }            
        }
    }
}
