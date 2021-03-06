﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Client.Services;

namespace Client
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
            services.AddMvc();

            services.AddScoped<ISensorDataHttpClient, SensorDataHttpClient>();
            services.AddHttpContextAccessor();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie("Cookies", options => options.AccessDeniedPath = "/Authorization/AccessDenied")
                .AddOpenIdConnect("oidc", options =>
                {
                    //treba da se sovpagja so konfiguracijata napravena na Identity serverot
                    options.Authority = "http://localhost:33123/";//da se navede na koj openId conecct provider treba da se "veruva" (osnovnata adresa na identity serverot)
                    options.RequireHttpsMetadata = false;
                    options.ClientId = "sensorclient";
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("sensorsapi");
                    /*shto treba da ni vrati token serverot; 
                     id_token - identification token;
                     code ne e access token, toj se isporachuva na preku serevr side (front chanal??) na app (vo nashiot sluchaj Client)
                     i app koristi back chanal i razmena na code za acces token*/
                    options.ResponseType = "code id_token";
                    //it_token kje se zachuva vo cookie-to
                    options.SaveTokens = true;
                    //se koristi pri razmena na code za acces token so token serverot
                    options.ClientSecret = "secret";
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name"
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Sensor}/{action=Index}/{id?}");
            });
        }
    }
}
