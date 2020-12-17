using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace msal.api
{
    public class Startup
    {
        readonly string _tenantId = "";
        readonly string _clientId = "";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options => SetTokenValidationParameters(options));

            services.AddCors((options =>
            {
                options.AddPolicy(
                    "Client",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            }));

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "msal.api", Version = "v1" });
            });
        }
        private void SetTokenValidationParameters(JwtBearerOptions options)
        {
            try
            {
                // Both App ID URI and client id are valid audiences in the access token
                var authority = $"https://login.microsoftonline.com/{_tenantId}/v2.0";
                var validAudiences = new string[] { _clientId };
                var validateIssuer = true;

                var tokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidAudiences = validAudiences,
                    ValidateIssuer = validateIssuer,
                };

                options.Authority = authority;
                options.TokenValidationParameters = tokenValidationParameters;
            }

            catch (ArgumentNullException ex)
            {
                throw new Exception("Error adding authentication, please check the Web API appsettings.json.", ex);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "msal.api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors("Client");

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
