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
    public class SentController : Controller
    {
        //
        // GET: /Sent/

        public ActionResult Index(int id, int BoxtypeId, int? AtypeId)//id=BoxId
        {
            ViewBag.BoxId = id;
            ViewBag.BoxtypeId = BoxtypeId;
            ViewBag.AtypeId = AtypeId;
            Session["BoxId"] = id;
            Session["BoxtypeId"] = BoxtypeId;
            Session["AtypeId"] = AtypeId;
            return PartialView();
        }

        public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var t = m.sp_GetDate().FirstOrDefault();
            var time = t.fldDateTime.Date;

            var user = m.sp_tblUserSelect("fldId", Session["UserId"].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var staff = m.sp_tblStaffSelect("fldId", user.fldStaffID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();

            var q = m.sp_LetterSelectSentDate("DateDESC", time.AddDays(-(staff.fldLetterLoadNum)), time, (Session["BoxId"]).ToString(),"").ToList().ToDataSourceResult(request);
            if((Session["BoxtypeId"]).ToString()=="999")
                q = m.sp_LetterSelectSentDate("DateDESC", time.AddDays(-(staff.fldLetterLoadNum)), time, (Session["BoxId"]).ToString(), "").Where(l => l.fldAssignmentTypeID == Convert.ToInt32(Session["AtypeId"])&& l.fldAssignmentStatusID!=3).ToList().ToDataSourceResult(request);
            //Session.Remove("BoxId");
            //Session.Remove("BoxtypeId");
            //Session.Remove("AtypeId");
            return Json(q);
        }
        public ActionResult Reload(string Type, int BoxId, string Start, string End, int? BoxtypeId, int? AtypeId)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_LetterSelectSentDate(Type, MyLib.Shamsi.Shamsi2miladiDateTime(Start), MyLib.Shamsi.Shamsi2miladiDateTime(End), BoxId.ToString(),"").ToList();
            if (BoxtypeId == 999)
                q = m.sp_LetterSelectSentDate(Type, MyLib.Shamsi.Shamsi2miladiDateTime(Start), MyLib.Shamsi.Shamsi2miladiDateTime(End), BoxId.ToString(), "").Where(l => l.fldAssignmentTypeID == AtypeId && l.fldAssignmentStatusID != 3).ToList();
            
            return Json(q, JsonRequestBehavior.AllowGet);
        }
        public JsonResult loadTarikh(string Start, string End)
        {//نمایش اطلاعات جهت رویت کاربر
            try
            {
                Models.AutomationEntities m = new Models.AutomationEntities();
                var t = m.sp_GetDate().FirstOrDefault();
                var time = t.fldDateTime.Date;
                var user = m.sp_tblUserSelect("fldId", Session["UserId"].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                var staff = m.sp_tblStaffSelect("fldId", user.fldStaffID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                
                var Ischange = true;
                if (time == MyLib.Shamsi.Shamsi2miladiDateTime(End) && time.AddDays(-(staff.fldLetterLoadNum)) == MyLib.Shamsi.Shamsi2miladiDateTime(Start))
                    Ischange = false;
                return Json(new
                {
                    Ischange = Ischange
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
    }
}
