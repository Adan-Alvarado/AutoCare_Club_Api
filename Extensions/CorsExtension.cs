namespace AutoCare_Club.Api.Extensions
{
    public static class CorsExtension
    {
        public static IServiceCollection AddCorsConfiguration
        (this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors( opt =>
            {
                var allowURLs = configuration.GetSection("AllowURLS").Get<string[]>();

                if(allowURLs == null)
                {
                    allowURLs = [""];
                }

                opt.AddPolicy("CorsPolicy", builder => builder
                .WithOrigins(allowURLs)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            });

            return services;
        }
    }
}