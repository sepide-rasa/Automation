using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Automation.Controllers.Users;

namespace Automation.Controllers.BasicInf
{
    [Authorize]
    public class BayganiController : Controller
    {
        //
        // GET: /Baygani/

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 76))
            {
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }
        //public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        //{
        //    Models.AutomationEntities m = new Models.AutomationEntities();
        //    var q = m.sp_tblLetterSelect("", "", 30, 1, "").ToList().ToDataSourceResult(request);
        //    return Json(q);
        //}
        public ActionResult Reload(string start, string end,byte type)
        {//جستجو
            //string[] _fiald = new string[] { "fldLetterTypeID" };
            //string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            //string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_SelectLetterNoAndicator(MyLib.Shamsi.Shamsi2miladiDateTime(start), MyLib.Shamsi.Shamsi2miladiDateTime(end), type, Convert.ToInt32(Session["UserId"])).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        } 
    }
}
