using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Configuration;
using System.Reflection;

namespace Automation
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*allaxd}", new { allaxd = @".*\.axd(/.*)?" });
            routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }

        protected void Application_Start()
        {
            RouteTable.Routes.MapHubs();
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            EmailPanel.Start();

            var settings = ConfigurationManager.ConnectionStrings["AutomationEntities"];
            var fi = typeof(ConfigurationElement).GetField(
                          "_bReadOnly",
                          BindingFlags.Instance | BindingFlags.NonPublic);
            fi.SetValue(settings, false);
            //settings.ConnectionString = @"metadata=res://*/Models.Model1.csdl|res://*/Models.Model1.ssdl|res://*/Models.Model1.msl;provider=System.Data.SqlClient;provider connection string= """ +
            //    @"data source=.\sql2017;initial catalog=AutomationTest;user id=rasasystem;password=rasasystem!@#;multipleactiveresultsets=True;application name=EntityFramework""";
            settings.ConnectionString = @"metadata=res://*/Models.Model1.csdl|res://*/Models.Model1.ssdl|res://*/Models.Model1.msl;provider=System.Data.SqlClient;provider connection string= """ +
               @"data source=.;Initial Catalog=Automation;User ID=rasasystem;Password=rasasystem!@#;multipleactiveresultsets=True;application name=EntityFramework""";

            var settings1 = ConfigurationManager.ConnectionStrings["AutomationConnectionString"];
            var fi1 = typeof(ConfigurationElement).GetField(
                          "_bReadOnly",
                          BindingFlags.Instance | BindingFlags.NonPublic);
            fi1.SetValue(settings1, false);
            //settings1.ConnectionString = @"Data Source=.\sql2017;initial catalog=AutomationTest;user id=rasasystem;password=rasasystem!@#";
            settings1.ConnectionString = @"Data source=.;Initial Catalog=Automation;User ID=rasasystem;Password=rasasystem!@#;";
        }

        protected void Session_End(object sender, EventArgs e)
        {
            if (Session["UserId"] != null)
            {
                Models.OnlineUser.RemoveOnlineUser(Session["UserId"].ToString());

            }
        }
    }
}