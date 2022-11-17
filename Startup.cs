using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.OpenApi.Models;

namespace pgapp
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
            services.AddControllers();
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            string connectionStringUrl = Environment.GetEnvironmentVariable("DATABASE_URL");          

            connectionString = string.IsNullOrEmpty(connectionStringUrl) ? connectionString : connectionStringUrl;
            NpgsqlConnectionStringBuilder npgsqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            NpgsqlConnectionStringBuilder  builder = npgsqlConnectionStringBuilder;
                 
            services.AddDbContext<ApplicationContext>
                (options => options.UseNpgsql(builder.ConnectionString));

       
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ApplicationContext dataContext)
        {
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Run DB migrationsn on app start
            dataContext.Database.Migrate();
        }
    }
}