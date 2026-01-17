using System.Web.Mvc;
using System.Web.Routing;

namespace WebApplicationUnipi
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapMvcAttributeRoutes();
            // Ignore route statements.
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.ashx/{*pathInfo}");



            routes.MapRoute(
                name: "Default", // Route name
                url: "{controller}/{action}/{id}", // URL with parameters
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRoute(
              name: "NoAccess",
              url: "NoAccess",
              defaults: new { controller = "Home", action = "NoAccess" }
          );

            routes.MapRoute(
                name: "Unauthorized",
                url: "Unauthorized",
                defaults: new { controller = "Home", action = "Unauthorized" }
            );

        
        }
    }
}
