using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace Automation.Controllers.Main
{
    [Authorize]
    public class TrashController : Controller
    {
        //
        // GET: /Trash/

        public ActionResult Index(int id)//id=BoxId
        {
            ViewBag.BoxId = id;
            Session["BoxId"] = id;
            return PartialView();
        }

        public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var t = m.sp_GetDate().FirstOrDefault();
            var time = t.fldDateTime.Date;

            var user = m.sp_tblUserSelect("fldId", Session["UserId"].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var staff = m.sp_tblStaffSelect("fldId", user.fldStaffID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();

            var q = m.sp_LetterSelectTrashDate("DateDESC", time.AddDays(-(staff.fldLetterLoadNum)), time, Convert.ToInt32(Session["BoxId"])).ToList().ToDataSourceResult(request);
            Session.Remove("BoxId");
            return Json(q);
        }

        public ActionResult Reload(string Type, int BoxId, string Start, string End)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_LetterSelectTrashDate(Type,MyLib.Shamsi.Shamsi2miladiDateTime(Start), MyLib.Shamsi.Shamsi2miladiDateTime(End), BoxId).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }

    }
}
