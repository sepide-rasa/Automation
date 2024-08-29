using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace Automation.Controllers.Tools
{
    public class Dashburd_LetterController : Controller
    {
        //
        // GET: /Dashburd_Letter/

        public ActionResult Index(int CommisionId)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblBoxSelect("fldComisionID", CommisionId.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => k.fldBoxTypeID == 1).FirstOrDefault();
            Session["BoxId"] = q.fldID;
            return PartialView();
        }

        public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_LetterSelectInboxNotRead(Convert.ToInt32(Session["BoxId"])).ToList().ToDataSourceResult(request);
            Session.Remove("BoxId");
            return Json(q);
        }

        public ActionResult Reload(string field, string value, int top, int searchtype)
        {//جستجو
            string[] _fiald = new string[] { "fldFamily", "fldName", "fldMelliCode" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblStaffSelect(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }

    }
}
