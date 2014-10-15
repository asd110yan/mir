using System;
using System.Web.Mvc;
using log4net;
using log4net.Config;
using YX.Mir.BLL;
using YX.Mir.Common;
using YX.Mir.Model.Models;
using System.Web.Routing;

[assembly: XmlConfigurator( Watch = true )]
namespace YX.Mir.Web.Controllers {
	public class ErrorAttribute : HandleErrorAttribute , IExceptionFilter {
		public static ILog Log = null;

		void IExceptionFilter.OnException( ExceptionContext filterContext ) {
			//为了防止数据库连接不上无法记录日志，本地也进行记录
			Log = LogManager.GetLogger( filterContext.GetType() );
			Log.Error( filterContext.Exception.Message , filterContext.Exception );

			var errBll = new ErrorlogBLL();
			//将错误日志插入数据库
			var log = new ErrorLog();
			log.ErrorMsg = WebHelper.GetErrMsg( filterContext.Exception );
			log.ErrorPage = filterContext.HttpContext.Request.Url.ToString();
			log.OperaterId = OnlineUser.OnLineUserID;
			log.Time = DateTime.Now;
			try {
				errBll.Add( log );
			}
			catch ( Exception ex ) {
				Log.Error( WebHelper.GetErrMsg( ex ) );
			}

		}
	}

	public class CheckLoginAttribute : AuthorizeAttribute , IAuthorizationFilter {
		void IAuthorizationFilter.OnAuthorization( AuthorizationContext filterContext ) {
			string allowCtrl = "home/login,home/getadmin".ToLower();
			string[] arrCtrl = allowCtrl.Split( ',' );
			string currentRoute = filterContext.RouteData.Values["controller"].ToString().ToLower();
			string currentAction = filterContext.RouteData.Values["action"].ToString().ToLower();
			string current = currentRoute + "/" + currentAction;
			foreach ( string str in arrCtrl ) {
				if ( str == current )
					return;
			}
			if ( OnlineUser.OnLineUser == null ) {
				filterContext.Result = new RedirectToRouteResult(
					new RouteValueDictionary( new {
						Controller = "home" ,
						Action = "login" ,
						ReturnUrl = filterContext.HttpContext.Request.RawUrl
					} ) );
			}

		}


	}



	public class ActionExecuteAttribute : ActionFilterAttribute , IActionFilter {
		private static readonly System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
		void IActionFilter.OnActionExecuted( ActionExecutedContext filterContext ) {
			watch.Stop();
			filterContext.HttpContext.Response.Write( string.Format( "执行时间:{0}" , watch.ElapsedMilliseconds ) );

		}

		void IActionFilter.OnActionExecuting( ActionExecutingContext filterContext ) {
			watch.Start();
		}


	}

	[Error]
	[CheckLogin]
	public class BaseController : Controller {

	}
}