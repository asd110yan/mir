using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using YX.Mir.BLL;
using YX.Mir.Common;
using YX.Mir.Model.Enum;
using YX.Mir.Model.Models;

namespace YX.Mir.Web.Controllers {
	public class HomeController : BaseController {
		//
		// GET: /Home/
		private readonly AdminBLL admBll = new AdminBLL();
		private readonly ErrorlogBLL errorlogBll = new ErrorlogBLL();
		private readonly LoginLogBLL logBll = new LoginLogBLL();
		private readonly MenuBLL mBll = new MenuBLL();
		private readonly MenuGroupBLL mgBll = new MenuGroupBLL();
		private readonly WebSiteInfoBLL webSiteInfoBll = new WebSiteInfoBLL();

		public ActionResult Index() {
			ViewBag.MenuTree = "[" + CreateMenuTree() + "]";
			return View();
		}

		public ActionResult Icon() {
			var sb = new StringBuilder();
			var dir = new DirectoryInfo( Server.MapPath( @"~\easyui\themes\icons_custom\" ) );
			FileInfo[] files = dir.GetFiles();
			foreach ( FileInfo file in files ) {
				if ( file.Extension.ToLower() == ".png" ) {
					sb.AppendFormat(
						"<li class=\"left\"> <a  href=\"javascript:void(0)\" onclick=\"choseIcon($(this))\"><img src=\"../../../easyui/themes/icons_custom/{0}\"  /></a></li>" ,
						file.Name );
				}
			}
			ViewBag.Icons = sb.ToString();
			return View( "~/views/home/menu/icon.cshtml" );
		}

		public ActionResult MenuNav() {
			return View();
		}

		public string CreateMenuTree() {
			return GetChildrenString( 0 );
		}

		public string GetChildrenString( int id ) {
			List<MenuGroup> lstParent = mgBll.GetList( id );
			var sb = new StringBuilder();
			if ( lstParent != null && lstParent.Count > 0 ) {
				int _count = 0;
				foreach ( MenuGroup mg in lstParent ) {
					sb.Append( "{" );
					sb.AppendFormat( "name:\"{0}\"" , mg.MGName );
					sb.AppendFormat( ",icon:\"{0}\"" , mg.Icon );
					sb.Append( CreateSubNodeString( mg.MGId ) );
					sb.Append( "}" );
					if ( _count != lstParent.Count - 1 ) {
						sb.Append( "," );
					}
					_count++;
				}
			}
			return sb.ToString();
		}

		public string CreateSubNodeString( int id ) {
			var sb = new StringBuilder();
			//下面的子菜单组
			List<MenuGroup> lstSubMg = mgBll.GetList( id );
			bool hasSubMg = lstSubMg.Count > 0; //是否有子菜单组

			////下面的子菜单
			List<Menu> lstMenu = mBll.GetList( id );
			bool hasMenu = lstMenu.Count > 0; //是否有子菜单

			if ( hasSubMg || hasMenu ) {
				sb.Append( ",open:true,children:[" );

				int count = 0;
				foreach ( MenuGroup subMg in lstSubMg ) {
					sb.Append( "{" );
					sb.AppendFormat( "name:\"{0}\"" , subMg.MGName );
					sb.AppendFormat( ",icon:\"{0}\"" , subMg.Icon );
					sb.Append( CreateSubNodeString( subMg.MGId ) );
					sb.Append( "}" );
					if ( count != lstSubMg.Count - 1 ) {
						sb.Append( "," );
					}

					count++;
				}

				if ( hasMenu ) {
					if ( hasSubMg ) {
						sb.Append( "," );
					}
					int count1 = 0;
					foreach ( Menu menu in lstMenu ) {
						sb.Append( "{" );
						sb.AppendFormat( "name:\"{0}\"" , menu.MenuName );
						sb.AppendFormat( ",icon:\"{0}\"" , menu.Icon );
						sb.AppendFormat( ",page:\"{0}\"" , menu.Page );
						sb.Append( "}" );
						if ( count1 != lstMenu.Count - 1 ) {
							sb.Append( "," );
						}
						count1++;
					}
				}
				sb.Append( "]" );
			}
			return sb.ToString();
		}

		#region 登陆相关

		public ActionResult Login( string ReturnUrl ) {
			ViewBag.ReturnUrl = "/home/index";
			if ( Url.IsLocalUrl( ReturnUrl ) ) {
				ViewBag.ReturnUrl = ReturnUrl;
			}
			return View();
		}

		[HttpPost]
		public ActionResult GetAdmin( string userName , string pwd , string lastTime ) {
			var msg = new MsgBase();
			try {
				Admin adm = admBll.Get( userName , pwd );
				if ( adm != null ) {

					Session["USER"] = adm;

					if ( lastTime.ToLower() != "none" ) {
						//保存cookie
						var user = new OnlineUser();
						user.SetCookie( userName , lastTime );
					}

					LoginLog lastLog = logBll.Get( OnlineUser.OnLineUserID );
					//创建登陆日志对象
					var log = new LoginLog();
					string ip = WebHelper.CurrentIp;
					if ( lastLog != null ) {
						log.LastLoginIpAddress = lastLog.LoginIPAddress;
						log.LastLoginPlace = lastLog.LoginPlace;
						log.LastLoginTime = lastLog.LoginTime;
					}
					log.LoginIPAddress = ip;
					log.LoginPlace = WebHelper.GetPlaceByIp( ip );
					log.OperateId = OnlineUser.OnLineUserID;
					log.Status = (int)UserStatus.在线;
					log.LoginTime = DateTime.Now;

					//执行插入
					logBll.Add( log );
					msg.Code = 1;
				}
				else {
					msg.Code = 0;
					msg.Message = "账号或者密码错误";
				}
			}
			catch ( Exception ex ) {
				msg.Code = -1;
				msg.Message = MsgBase.ErrMessage;
			}
			return Json( msg );
		}

		#endregion

		#region 配置相关

		public ActionResult Config() {
			WebSiteInfo web = webSiteInfoBll.Get();
			return View( "Config/Config" , web??new WebSiteInfo() );
		}

		[HttpPost]
		[ValidateInput( false )]
		/// <summary>
		/// 增加网站配置
		/// </summary>
		/// <returns></returns>
		public ActionResult AddConfig( WebSiteInfo model ) {
			if ( ModelState.IsValid ) {
				try {
					bool result = webSiteInfoBll.Add( model );
					return Content( string.Format( "<script>$.messager.alert('提示信息','{0}','info')</script>" ,
						result ? MsgBase.SuccessMessage : MsgBase.FailMessage ) );
				}
				catch ( Exception ) {
					return
						Content( string.Format( "<script>$.messager.alert('提示信息','{0}','error')</script>" ,
							MsgBase.ErrMessage ) );
				}
			}
			return View( "Config/Config" , model );
		}

		[HttpPost]
		[ValidateInput( false )]
		/// <summary>
		/// 更新网站配置
		/// </summary>
		/// <returns></returns>
		public ActionResult UpdateConfig( WebSiteInfo model ) {
			if ( ModelState.IsValid ) {
				try {
					bool result = webSiteInfoBll.Update( model );
					return Content( string.Format( "<script>$.messager.alert('提示信息','{0}','info')</script>" ,
						result ? MsgBase.SuccessMessage : MsgBase.FailMessage ) );
				}
				catch ( Exception ) {
					return
						Content( string.Format( "<script>$.messager.alert('提示信息','{0}','error')</script>" ,
							MsgBase.ErrMessage ) );
				}
			}
			return View( "Config/Config" , model );
		}

		public ActionResult MenuConfig() {
			return View( "Menu/MenuConfig" );
		}

		public ActionResult MenuGroup() {
			return View( "Menu/MenuGroup" );
		}

		#endregion

		#region 日志相关

		public ActionResult LoginLog() {
			return View( "Log/LoginLog" );
		}


		public ActionResult ExportLoginLog( DateTime? beginTime , DateTime? endTime ) {
			List<LoginLog> lstLog = logBll.GetExcelList( OnlineUser.OnLineUserID , beginTime , endTime );
			var query = from log in lstLog
						join adm in admBll.GetList()
							on log.OperateId equals adm.ID
							into admOnEmpty
						from adm in admOnEmpty.DefaultIfEmpty()
						select new {
							log.LogId ,
							UserName = log.OperateId == -1 ? "未知" : adm.AdminName ,
							log.LoginTime ,
							log.LoginPlace ,
							log.LoginIPAddress
						}
				 ;
			StringBuilder sb = new StringBuilder();
			sb.Append( "<table border='1'>" );
			sb.Append( "<tr>" );
			sb.Append( "<td>登陆人</td>" );
			sb.Append( "<td>登陆时间</td>" );
			sb.Append( "<td>登陆地点</td>" );
			sb.Append( "<td>登陆IP</td>" );
			sb.Append( "</tr>" );
			foreach ( var item in query ) {
				sb.Append( "<tr>" );
				sb.AppendFormat( "<td>{0}</td>" , item.UserName );
				sb.AppendFormat( "<td>{0}</td>" , item.LoginTime );
				sb.AppendFormat( "<td>{0}</td>" , item.LoginPlace );
				sb.AppendFormat( "<td>{0}</td>" , item.LoginIPAddress );
				sb.AppendFormat( "</tr>" );
			}
			sb.Append( "</table>" );
			DirectoryInfo dir = new DirectoryInfo( Server.MapPath( "~\\excel\\" ) );
			if ( !dir.Exists ) {
				dir.Create();
			}
			string file = string.Format( "{0}-{1}-{2}登陆日志.xls" ,
				OnlineUser.OnLineUserName , beginTime , endTime , DateTime.Now.ToString( "yyyyMMddhhmmss" ) );
			string filePath = dir + file;
			using ( FileStream fs = new FileStream( filePath , FileMode.Create ) ) {
				using ( StreamWriter sw = new StreamWriter( fs ) ) {
					sw.Write( sb.ToString() );
				}
			}
			return File( filePath , "application/excel" , file );
		}

		public ActionResult ErrorLog() {
			return View( "Log/ErrorLog" );
		}

		public JsonResult GetLoginLog( int rows , int page , DateTime? beginTime , DateTime? endTime ) {
			try {
				int total;
				List<LoginLog> lstLog = logBll.GetList( OnlineUser.OnLineUserID , page , rows , beginTime , endTime ,
					out total );
				var query = from log in lstLog
							join adm in admBll.GetList()
								on log.OperateId equals adm.ID
								into admOnEmpty
							from adm in admOnEmpty.DefaultIfEmpty()
							select new {
								log.LogId ,
								UserName = log.OperateId == -1 ? "未知" : adm.AdminName ,
								log.LoginTime ,
								log.LoginPlace ,
								log.LoginIPAddress
							}
					;
				return Json( new {
					total ,
					rows = query.ToList()
				} );
			}
			catch ( Exception ) {
				return Json( new {
					total = 0 ,
					rows = "{}"
				} );
			}
		}

		public JsonResult GetErrorLog( int rows , int page , DateTime? beginTime , DateTime? endTime ) {
			try {
				int total;
				List<ErrorLog> lstLog = errorlogBll.GetList( page , rows , beginTime , endTime , out total );
				var query = from error in lstLog
							join adm in admBll.GetList()
								on error.OperaterId equals adm.ID
								into admOnEmpty
							from
								adm in admOnEmpty.DefaultIfEmpty()
							select new {
								error.LogId ,
								error.ErrorMsg ,
								UserName = error.OperaterId == -1 ? "未知" : adm.AdminName ,
								error.ErrorPage ,
								error.Time
							};
				return Json( new {
					total ,
					rows = query.ToList()
				} );
			}
			catch ( Exception ) {
				return Json( new {
					total = 0 ,
					rows = "{}"
				} );
			}
		}

		#endregion
	}
}