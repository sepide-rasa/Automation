using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Automation.Controllers.Users;
namespace Automation.Controllers.Tools
{
    [Authorize]
    public class MindController : Controller
    {
        //
        // GET: /Mind/

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 96))
            {
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }
        public ActionResult GetComission()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var date = p.sp_GetDate().FirstOrDefault();
            var user = p.sp_tblUserSelect("fldId", Session["UserId"].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var q = p.sp_tblCommisionSelect("fldStaffID", user.fldStaffID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(k => MyLib.Shamsi.Shamsi2miladiDateTime(k.fldEndDate) >= date.fldDateTime).ToList().Select(c => new { fldID = c.fldID, fldName = c.fldStaffName + "(" + c.fldOrganicRoleName + ")" });
            return Json(q, JsonRequestBehavior.AllowGet);
        }
        //public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        //{
        //    Models.AutomationEntities m = new Models.AutomationEntities();
        //    var q = m.sp_tblMindSelect("", "", 0).ToList().ToDataSourceResult(request);
        //    return Json(q);
        //}

        public ActionResult Save(Models.Mind mind)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (mind.fldDesc == null)
                    mind.fldDesc = "";
                if (mind.fldID == 0)
                {//ثبت رکورد جدید
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 103))
                    {
                        p.sp_tblMindInsert(mind.fldCreatedComisionID, MyLib.Shamsi.Shamsi2miladiDateTime(mind.fldMindDate), mind.fldSubject, mind.fldText, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                        return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });
                    }

                    else
                    {
                        Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                        return RedirectToAction("error", "Metro");
                    }
                }
                else
                {//ویرایش رکورد ارسالی
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 104))
                    {
                        p.sp_tblMindUpdate(Convert.ToInt32(mind.fldID), mind.fldCreatedComisionID, MyLib.Shamsi.Shamsi2miladiDateTime(mind.fldMindDate), mind.fldSubject, mind.fldText, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                        return Json(new { data = "ویرایش با موفقیت انجام شد.", state = 0 });
                    }

                    else
                    {
                        Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                        return RedirectToAction("error", "Metro");
                    }
                }
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

        public ActionResult Delete(string id)
        {//حذف یک رکورد
            try
            {
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 105))
                {
                    Models.AutomationEntities Car = new Models.AutomationEntities();
                    if (Convert.ToInt32(id) != 0)
                    {
                        Car.sp_tblMindDelete(Convert.ToInt32(id), 1);
                        return Json(new { data = "حذف با موفقیت انجام شد.", state = 0 });
                    }
                    else
                    {
                        return Json(new { data = "رکوردی برای حذف انتخاب نشده است.", state = 1 });
                    }
                }
                else
                {
                    Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                    return RedirectToAction("error", "Metro");
                }
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
        public ActionResult Reload(string field, string value, int top, int searchtype)
        {//جستجو
            string[] _fiald = new string[] { "fldCreatedComisionID" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();//.Where(k => k.fldCurrentLetter_Id == value)
            var q = m.sp_tblMindSelect(_fiald[Convert.ToInt32(field)], searchtext, top,0).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Details(int id)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblMindSelect("fldId", id.ToString(), 0,0).FirstOrDefault();
                return Json(new
                {

                    fldID = q.fldID,
                    fldCreatedComisionID = q.fldCreatedComisionID,
                    fldMindDate = q.fldMindDate,
                    fldSubject = q.fldSubject,
                    fldText = q.fldText

                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return null;
            }
        }
    }
}
