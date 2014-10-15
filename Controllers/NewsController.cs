using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using YX.Mir.BLL;
using YX.Mir.Common;
using YX.Mir.Model.Models;

namespace YX.Mir.Web.Controllers {
	public class NewsController : BaseController {
		//
		// GET: /News/
		private readonly CategoryBLL cateBll = new CategoryBLL();
		private readonly GameNewsBLL newsBll = new GameNewsBLL();
		public ActionResult Index() {
			return View();
		}

		public ActionResult NewsCate() {
			return View();
		}

		public string GetCateList() {
			return GetCategoryJson();
		}

		public ActionResult CateDlg( int CateId ) {
			ViewBag.Category = GetCateTree();
			if ( CateId == 0 ) {
				ViewData["SORT"] = cateBll.GetMaxSort();
			}
			Category model = cateBll.GetById( CateId );
			if ( model != null ) {
				ViewBag.Name = model.CateName;
				ViewBag.ParentId = model.ParentId;
				Category pCate = cateBll.GetById( model.ParentId );
				ViewBag.ParentName = pCate != null ? pCate.CateName : string.Empty;
				ViewData["SORT"] = model.Sort;
			}
			return View();
		}

		public ActionResult NewsDlg() {
			return View();
		}

		[HttpPost]
		public JsonResult DeleteCate( int CateId ) {
			var msg = new MsgBase();
			try {
				int i = cateBll.Delete( CateId );
				msg.Message = i > 0 ? MsgBase.SuccessMessage : MsgBase.FailMessage;
				msg.Code = i > 0 ? 1 : 0;
			}
			catch ( Exception ) {
				msg.Code = -1;
				msg.Message = MsgBase.ErrMessage;
			}
			return Json( msg );
		}

		[HttpPost]
		public ActionResult AddCate( string CateName , int Sort , int ParentId = 0 ) {
			MsgBase msg = new MsgBase();
			try {
				Category cate = cateBll.GetByName( CateName.Trim() );
				if ( cate == null ) {
					Category model = new Category() {
						CateName = CateName.Trim() ,
						ParentId = ParentId ,
						Sort = Sort
					};
					int i = cateBll.Add( model );
					msg.Message = i > 0 ? MsgBase.SuccessMessage : MsgBase.FailMessage;
					msg.Code = i > 0 ? 1 : 0;
				}
				else {
					msg.Code = 2;
					msg.Message = "分类已经存在";
				}

			}
			catch ( Exception ) {
				msg.Code = -1;
				msg.Message = MsgBase.ErrMessage;
			}
			return Json( msg );
		}


		[HttpPost]
		public ActionResult EditCate( string CateName , int Sort , int CateId , int ParentId = 0 ) {
			MsgBase msg = new MsgBase();
			try {
				Category model = cateBll.GetById( CateId );
				if ( model != null ) {
					if ( CateName != model.CateName ) {
						Category cate = cateBll.GetByName( CateName.Trim() );
						if ( cate != null ) {
							msg.Code = 2;
							msg.Message = "分类已经存在";
							return Json( msg );
						}

					}
					if ( ParentId == model.CateId ) {
						msg.Message = "上级分类不能为自身";
						msg.Code = 3;
						return Json( msg );
					}
					List<Category> lstSubCate = cateBll.GetList( model.CateId );
					foreach ( Category subCate in lstSubCate ) {
						if ( ParentId == subCate.CateId ) {
							msg.Message = "上级分类不能为子分类";
							msg.Code = 4;
							return Json( msg );
						}
					}
					model.CateName = CateName.Trim();
					model.ParentId = ParentId;
					model.Sort = Sort;
					int i = cateBll.Edit( model );
					msg.Message = i > 0 ? MsgBase.SuccessMessage : MsgBase.FailMessage;
					msg.Code = i > 0 ? 1 : 0;
				}
			}
			catch ( Exception ) {
				msg.Code = -1;
				msg.Message = MsgBase.ErrMessage;
			}
			return Json( msg );
		}

		public string GetCategoryJson() {
			List<Category> lstCate = cateBll.GetList( 0 );
			var sb = new StringBuilder();
			sb.Append( "[" );
			for ( int i = 0 ; i < lstCate.Count ; i++ ) {
				sb.Append( "{" );
				sb.AppendFormat( "\"CateId\":{0}," , lstCate[i].CateId );
				sb.AppendFormat( "\"CateName\":\"{0}\"" , lstCate[i].CateName );
				sb.Append( GetSubCateJson( lstCate[i].CateId ) );
				sb.Append( "}" );
				if ( i != lstCate.Count - 1 ) {
					sb.Append( "," );
				}
			}
			sb.Append( "]" );
			return sb.ToString();
		}

		public string GetSubCateJson( int pId ) {
			var sb = new StringBuilder();
			List<Category> lstSubCate = cateBll.GetList( pId );
			if ( lstSubCate != null && lstSubCate.Count > 0 ) {
				sb.Append( ",\"children\":[" );
				for ( int j = 0 ; j < lstSubCate.Count ; j++ ) {
					sb.Append( "{" );
					sb.AppendFormat( "\"CateId\":{0}," , lstSubCate[j].CateId );
					sb.AppendFormat( "\"CateName\":\"{0}\"" , lstSubCate[j].CateName );
					sb.Append( GetSubCateJson( lstSubCate[j].CateId ) );
					sb.Append( "}" );
					if ( j != lstSubCate.Count - 1 ) {
						sb.Append( "," );
					}
				}
				sb.Append( "]" );
			}
			return sb.ToString();
		}

		public string GetCateTree() {
			List<Category> lstCate = cateBll.GetAllList();
			var sb = new StringBuilder();
			sb.Append( "[" );
			if ( lstCate != null && lstCate.Count > 0 ) {
				//if (id == 0)
				//{
				//    sb.Append(",");
				//}
				int count = 0;
				foreach ( Category cate in lstCate ) {
					sb.Append( "{" );
					sb.AppendFormat( "\"id\":{0},\"pId\":{1},\"name\":\"{2}\"" , cate.CateId ,
						cate.ParentId , cate.CateName );
					sb.Append( "}" );
					if ( count != lstCate.Count - 1 ) {
						sb.Append( "," );
					}
					count++;
				}
			}
			sb.Append( "]" );
			return sb.ToString();
		}

		public ActionResult NewsList() {
			GameNews model=new GameNews();
			model.Type = "0";
			return View( model );
		}

		[HttpPost]
		public JsonResult DeleteNews( string ids ) {
			var msg = new MsgBase();
			int count = 0;
			string[] arr = ids.Split( ',' );
			try {
				foreach ( string id in arr ) {
					int i = newsBll.Delete( int.Parse( id ) );
					if ( i > 0 ) {
						count++;
					}
				}
				msg.Message = count > 0
					? MsgBase.SuccessMessage + string.Format( ",共删除{0}条信息" , count )
					: MsgBase.FailMessage;
				msg.Code = count > 0 ? 1 : 0;
			}
			catch ( Exception ) {
				msg.Code = -1;
				msg.Message = MsgBase.ErrMessage;
			}
			return Json( msg );
		}

		[HttpPost]
		[ValidateInput( false )]
		public ActionResult AddNews( GameNews model ) {

			MsgBase msg = new MsgBase();
			try {
				int i = 0;
				if ( model.ID == 0 ) {
					i = newsBll.Add( model );
				}
				else {
					GameNews news = newsBll.GetById( model.ID );
					if ( news != null ) {
						news.Author = model.Author;
						news.Title = model.Title;
						news.Content = model.Content;
						news.Type = model.Type;
						i = newsBll.Update( news );
					}
				}
				msg.Message = i > 0 ? MsgBase.SuccessMessage : MsgBase.FailMessage;
				msg.Code = i > 0 ? 1 : 0;

			}
			catch ( Exception ) {
				msg.Code = -1;
				msg.Message = MsgBase.ErrMessage;
			}
			return Json( msg );
		}

		public JsonResult GetNewsList( int rows , int page , string title , string author , string type , DateTime? beginTime , DateTime? endTime ) {

			try {
				int total;
				var query = newsBll.GetList( page , rows , title , author , beginTime , endTime , type , out total );
				var lstNews = from news in query
							  join cate in cateBll.GetAllList()
								  on news.Type equals cate.CateId.ToString()
								  into result
							  from cate in result.DefaultIfEmpty()
							  select new {
								  news.ID ,
								  news.Title ,
								  news.Author ,
								  news.Date ,
								  Type = cate != null ? cate.CateName : "未知"
							  }
				;
				return Json( new { total = total , rows = lstNews } );
			}
			catch ( Exception ) {

				return Json( new { total = 0 , rows = "{}" } );
			}

		}

		public JsonResult GetNews( int ID ) {
			GameNews model = newsBll.GetById( ID );
			if ( model != null ) {
				return Json( model , JsonRequestBehavior.AllowGet );
			}
			return Json( "{}" , JsonRequestBehavior.AllowGet );
		}
		public JsonResult GetAllCate() {
			List<Category> lstCate = cateBll.GetAllList();
			lstCate.Insert( 0 , new Category() { CateId = 0 , CateName = "所有" } );
			try {
				return Json( lstCate , JsonRequestBehavior.AllowGet );
			}
			catch ( Exception ) {

				return Json( "[]" , JsonRequestBehavior.AllowGet );
			}
		}
	}
}

