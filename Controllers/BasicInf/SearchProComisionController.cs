using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace Automation.Controllers.BasicInf
{
    [Authorize]
    public class SearchProComisionController : Controller
    {
        //
        // GET: /SearchProComision/

        public ActionResult Index(int id)
        {
            ViewBag.state = id;
            return PartialView();
        }
        public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var d=m.sp_GetDate().FirstOrDefault().fldDateTime;
            var q = m.sp_tblCommisionSelect("EndDateNashode", "",0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldSign == true).ToList().ToDataSourceResult(request);
            return Json(q);
        }

        public ActionResult Reload(string field, string value, int top, int searchtype)
        {//جستجو
            Models.AutomationEntities m = new Models.AutomationEntities();
            var d = m.sp_GetDate().FirstOrDefault().fldDateTime;
            string[] _fiald = new string[] { "fldStaffName_Date" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            var q = m.sp_tblCommisionSelect(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldSign == true).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        } 

    }
}
