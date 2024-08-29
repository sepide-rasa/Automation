using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
namespace Automation.Controllers
{
    [Authorize]
    public class DraftController : Controller
    {
        //
        // GET: /Draft/

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

            var q = m.sp_LetterSelectDraftDate("DateDESC", time.AddDays(-(staff.fldLetterLoadNum)), time, (Session["BoxId"]).ToString(),"").ToList().ToDataSourceResult(request);
            Session.Remove("BoxId");
            return Json(q);
        }

        public ActionResult Reload(string Type, int BoxId, string Start, string End)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_LetterSelectDraftDate(Type, MyLib.Shamsi.Shamsi2miladiDateTime(Start), MyLib.Shamsi.Shamsi2miladiDateTime(End), BoxId.ToString(),"").ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Details(int id)
        {//نمایش اطلاعات جهت رویت کاربر
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                //ویرایش جدول ارجاعات
                
                int fldLetterTypeId = 0;
                
                var Letter = p.sp_tblLetterSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (Letter != null)
                {
                    fldLetterTypeId = Letter.fldLetterTypeID;
                }
                return Json(new
                {//ویرایش کل نامه
                    //ویرایش جدول ارجاعات
                    fldLetterTypeId = fldLetterTypeId
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
    
    }
}
