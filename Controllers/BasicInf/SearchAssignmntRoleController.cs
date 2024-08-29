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
    public class SearchAssignmntRoleController : Controller
    {
        //
        // GET: /SearchAssignmntRole/

        public ActionResult Index(int id, int type,int Comissionid)
        {
            ViewBag.state = id;
            ViewBag.Type = type;
            Session["Searchtype"] = type;//1=All 2=staff 3=partner
            Session["ComId"] = Comissionid;
            return PartialView();
        }
        public ActionResult GetAssignmentTypes()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_tblAssignmentTypeSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldType });
            return Json(q, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblAssignmentRoleSelectRecivers("", "", 30, Convert.ToInt32(Session["ComId"]), "").ToList().ToDataSourceResult(request);
            if (Convert.ToInt32(Session["Searchtype"]) == 2)
                q = m.sp_tblAssignmentRoleSelectRecivers("", "", 30, Convert.ToInt32(Session["ComId"]), "").Where(j => j.fldtype == 1).ToList().ToDataSourceResult(request);
            if (Convert.ToInt32(Session["Searchtype"]) == 3)
                q = m.sp_tblAssignmentRoleSelectRecivers("", "", 30, Convert.ToInt32(Session["ComId"]), "").Where(j => j.fldtype == 2).ToList().ToDataSourceResult(request);
            return Json(q);
        }

        public ActionResult Reload(string field, string value, int top, int searchtype)
        {//جستجو
            string[] _fiald = new string[] { "fldReceiverComisionName" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblAssignmentRoleSelectRecivers(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["ComId"]), "").ToList();
            if (Convert.ToInt32(Session["Searchtype"]) == 2)
                q = m.sp_tblAssignmentRoleSelectRecivers(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["ComId"]), "").Where(j => j.fldtype == 1).ToList();
            if (Convert.ToInt32(Session["Searchtype"]) == 3)
                q = m.sp_tblAssignmentRoleSelectRecivers(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["ComId"]), "").Where(j => j.fldtype == 2).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }
    }
}