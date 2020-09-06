using Cashback.Infra.CrossCutting.Configuration;
using Cashback.Infra.Repository;
using Cashback.Infra.Repository.Configuration;
using Cashback.Service.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cashback.API
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
            services.AddDbContext<PrincipalContext>(opt => opt.UseSqlServer(Configuration.GetConnectionString("Principal")));
            services.AddControllers();
            services.AddTinyMapperConfiguration();
            services.AddHttpClient("balance-api", c =>
            {
                c.BaseAddress = new Uri(Configuration["BalanceAPI:BaseURL"]);
                c.DefaultRequestHeaders.Add("Token", Configuration["BalanceAPI:Token"]);
            });
            services.ConfigureCashbackServices();
            services.ConfigureCashbackRepositories();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = Configuration["TokenJWT:Issuer"],
                            ValidAudience = Configuration["TokenJWT:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey
                            (Encoding.UTF8.GetBytes(Configuration["TokenJWT:Key"]))
                        };
                    });




            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Cashback",
                    Version = "v1",
                    Description = "Cadastro de revendedores e cálculo de cashback",
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Coloque a palavra 'Bearer' seguida de um espaço e o JWT vindo do metodo post de 'Login'. Os POST 'Reseller' e 'Reseller/Login' não precisam de autenticação",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<PrincipalContext>();
                context.Database.Migrate();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            if (Convert.ToBoolean(Configuration["Logging:Persist"]))
                loggerFactory.AddFile(Path.Combine(Configuration["Logging:Path"], "cashback-app-log-{Date}.log"));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Cashback"));
        }
    }
}
