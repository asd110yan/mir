using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using YX.Mir.BLL;
using YX.Mir.Common;
using YX.Mir.Model.Models;

namespace YX.Mir.Web.Controllers {
	public class Icon {
		public string Path { get; set; }

		public string Name { get; set; }
	}

	public class MenuController : BaseController {
		//
		// GET: /Menu/

		private readonly MenuBLL mBll = new MenuBLL();
		private readonly MenuGroupBLL mgBll = new MenuGroupBLL();


		public ActionResult Index() {
			return View();
		}

		[HttpPost]
		public JsonResult AddMenuGroup(string name, int sort, string icon, int pid = 0) {
			var msg = new MsgBase();
			try {
				MenuGroup model = mgBll.Get(name);
				if (model != null) {
					msg.Message = "当前菜单组名称已经存在";
					msg.Code = 2;
					return Json(msg);
				}
				model = new MenuGroup();
				model.MGName = name;
				model.Icon = icon;
				model.Sort = sort;
				model.ParentId = pid;
				int i = mgBll.Add(model);
				msg.Message = i > 0 ? MsgBase.SuccessMessage : MsgBase.FailMessage;
				msg.Code = i > 0 ? 1 : 0;
			}
			catch (Exception) {
				msg.Code = -1;
				msg.Message = MsgBase.ErrMessage;
			}
			return Json(msg);
		}


		[HttpPost]
		public JsonResult EditMenuGroup(string name, int sort, string icon, int id, int pid = 0) {
			var msg = new MsgBase();
			try {
				MenuGroup oldModel = mgBll.Get(id);

				if (oldModel != null) {
					if (name != oldModel.MGName) {
						MenuGroup model = mgBll.Get(name);
						if (model != null) {
							msg.Message = "当前菜单组名称已经存在";
							msg.Code = 2;
							return Json(msg);
						}
					}
					if (pid == oldModel.MGId) {
						msg.Message = "父菜单组不能为自身";
						msg.Code = 3;
						return Json(msg);
					}
					List<MenuGroup> lstMg = mgBll.GetList(oldModel.MGId);
					foreach (MenuGroup mg in lstMg) {
						if (pid == mg.MGId) {
							msg.Message = "上级菜单组不能为子菜单组";
							msg.Code = 4;
							return Json(msg);
						}
					}
					oldModel.MGName = name;
					oldModel.Icon = icon;
					oldModel.Sort = sort;
					oldModel.ParentId = pid;
					int i = mgBll.Update(oldModel);
					msg.Message = i > 0 ? MsgBase.SuccessMessage : MsgBase.FailMessage;
					msg.Code = i > 0 ? 1 : 0;
				}
			}
			catch (Exception) {
				msg.Code = -1;
				msg.Message = MsgBase.ErrMessage;
			}
			return Json(msg);
		}

		public ActionResult MGDlg(string pName, int pId = 0, int id = 0) {
			ViewBag.ParentId = pId;
			ViewBag.ParentName = pName;
			ViewData["SORT"] = mgBll.GetMaxSort();
			if (id != 0) {
				MenuGroup model = mgBll.Get(id);
				bool isNull = model != null ? false : true;
				ViewBag.Icon = !isNull ? model.Icon : string.Empty;
				ViewBag.Name = !isNull ? model.MGName : string.Empty;
				ViewData["SORT"] = !isNull ? model.Sort : 0;
			}
			return View("~/views/home/menu/mgdlg.cshtml");
		}

		[HttpPost]
		public JsonResult DeleteMenuGroup(int id) {
			var msg = new MsgBase();
			try {
				int i = mgBll.Delete(id);
				msg.Message = i > 0 ? MsgBase.SuccessMessage : MsgBase.FailMessage;
				msg.Code = i > 0 ? 1 : 0;
			}
			catch (Exception) {
				msg.Code = -1;
				msg.Message = MsgBase.ErrMessage;
			}
			return Json(msg);
		}

		[HttpPost]
		public JsonResult DeleteMenu(string ids) {
			var msg = new MsgBase();
			int count = 0;
			string[] arr = ids.Split(',');
			try {
				foreach (string id in arr) {
					int i = mBll.Delete(int.Parse(id));
					if (i > 0) {
						count++;
					}
				}
				msg.Message = count > 0
					? MsgBase.SuccessMessage + string.Format(",共删除{0}条信息", count)
					: MsgBase.FailMessage;
				msg.Code = count > 0 ? 1 : 0;
			}
			catch (Exception) {
				msg.Code = -1;
				msg.Message = MsgBase.ErrMessage;
			}
			return Json(msg);
		}

		public ContentResult GetMenuGroupTree(int id = 0) {
			List<MenuGroup> lstGroups = mgBll.GetList(id);
			var sb = new StringBuilder();
			sb.Append("[");
			//if (id == 0)
			//{
			//    sb.Append("{");
			//    sb.AppendFormat("\"id\":{0},\"name\":\"{1}\",\"isParent\":true", 0, "菜单组");
			//    sb.Append("}");
			//}
			if (lstGroups != null && lstGroups.Count > 0) {

				//if (id == 0)
				//{
				//    sb.Append(",");
				//}
				int count = 0;
				foreach (MenuGroup mg in lstGroups) {
					sb.Append("{");
					sb.AppendFormat("\"id\":{0},\"pid\":{1},\"name\":\"{2}\",\"isParent\":{3},\"icon\":\"{4}\"", mg.MGId,
						mg.ParentId, mg.MGName, mgBll.GetList(mg.MGId).Count == 0 ? "false" : "true", mg.Icon);
					sb.Append("}");
					if (count != lstGroups.Count - 1) {
						sb.Append(",");
					}
					count++;
				}
			}
			sb.Append("]");
			return Content(sb.ToString());
		}

		public ActionResult MDlg(int id = 0) {
			ViewData["SORT"] = mBll.GetMaxSort();
			if (id != 0) {
				Menu menu = mBll.Get(id);
				if (menu != null) {
					ViewBag.MenuName = menu.MenuName;
					ViewBag.MGId = menu.MGId;
					MenuGroup mg = mgBll.Get(id);
					ViewBag.MenuGroupName = mg != null ? mg.MGName : string.Empty;
					ViewData["SORT"] = menu.Sort;
					ViewBag.Icon = menu.Icon;
					ViewBag.Page = menu.Page;
				}
			}
			return View("~/views/home/menu/mdlg.cshtml");
		}

		[HttpPost]
		public JsonResult AddMenu(string name, int sort, string icon, string page, int pId = 0) {
			var msg = new MsgBase();
			try {
				Menu model = mBll.Get(name);
				if (model != null) {
					msg.Message = "当前菜单名称已经存在";
					msg.Code = 2;
					return Json(msg);
				}
				model = new Menu();
				model.MenuName = name;
				model.Icon = icon;
				model.Sort = sort;
				model.Page = page;
				model.MGId = pId;
				int i = mBll.Add(model);
				msg.Message = i > 0 ? MsgBase.SuccessMessage : MsgBase.FailMessage;
				msg.Code = i > 0 ? 1 : 0;
			}
			catch (Exception) {
				msg.Code = -1;
				msg.Message = MsgBase.ErrMessage;
			}
			return Json(msg);
		}

		[HttpPost]
		public JsonResult EditMenu(string name, int sort, string icon, string page, int id, int pId = 0) {
			var msg = new MsgBase();
			try {
				Menu oldModel = mBll.Get(id);
				if (oldModel != null) {
					if (oldModel.MenuName != name) {
						Menu model = mBll.Get(name);
						if (model != null) {
							msg.Message = "当前菜单名称已经存在";
							msg.Code = 2;
							return Json(msg);
						}
					}
					oldModel.MenuName = name;
					oldModel.Icon = icon;
					oldModel.Sort = sort;
					oldModel.Page = page;
					oldModel.MGId = pId;
					int i = mBll.Update(oldModel);
					msg.Message = i > 0 ? MsgBase.SuccessMessage : MsgBase.FailMessage;
					msg.Code = i > 0 ? 1 : 0;
				}
			}
			catch (Exception) {
				msg.Code = -1;
				msg.Message = MsgBase.ErrMessage;
			}
			return Json(msg);
		}

		public JsonResult GetMenuList(int rows, int page, int? mgId) {
			try {
				int total;
				List<Menu> lstMenu = mBll.GetList(rows, page, mgId, out total);
				var query = from m in lstMenu
							join mg in mgBll.GetAllList()
								on m.MGId equals mg.MGId
								into result
							from mg in result.DefaultIfEmpty()
							select new {
								m.MenuID,
								m.MenuName,
								MGName = mg != null ? mg.MGName : string.Empty,
								m.Sort,
								m.Icon,
								m.Page
							};

				return Json(new { total, rows = query });
			}
			catch (Exception) {
				return Json(new { total = 0, rows = "{}" });
			}
		}
	}
}