//using Microsoft.AspNetCore.Builder;
//using Microsoft.Extensions.DependencyInjection;

//namespace AdvancedTask.Infrastructure.Initialization
//{
//    public static class ApplicationBuilderExtensions
//    {
//        public static IApplicationBuilder UseAddAdvancedTask(this IApplicationBuilder app)
//        {
//            var services = app.ApplicationServices;

//            var initializer = services.GetRequiredService<AddAdvancedTaskInitializer>();
//            initializer.Initialize();

//            return app;
//        }
//    }
//}
