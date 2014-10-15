using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;
using log4net;
using YX.Mir.BLL;
using YX.Mir.Common;
using YX.Mir.Model.Enum;
using YX.Mir.Model.Models;
using YX.Mir.Web.Controllers;

namespace YX.Mir.Web
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // 路由名称
                "{controller}/{action}/{id}", // 带有参数的 URL
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // 参数默认值
            );

            routes.MapRoute(
            "login", // 路由名称
            "{controller}/{action}/google.com/{returnurl}", // 带有参数的 URL
            new { controller = "Home", action = "Login", returnurl =""} // 参数默认值
        );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
        LoginLogBLL logBll=new LoginLogBLL();
        protected void Session_OnEnd()
        {
            LoginLog log = logBll.Get(OnlineUser.OnLineUserID);
            if (log != null)
            {
                log.Status = (int) UserStatus.离线;
                logBll.Update(log);
            }
        }
    }
}