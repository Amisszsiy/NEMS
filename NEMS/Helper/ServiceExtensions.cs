namespace NEMS.Helper
{
    public static class ServiceExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IClockService, Clock>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
