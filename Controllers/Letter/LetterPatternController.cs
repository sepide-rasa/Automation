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
    public class LetterPatternController : Controller
    {
        //
        // GET: /Pattern/

        public ActionResult Index()
        {//بارگذاری صفحه اصلی 
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 37))
            {
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }

        public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblPatternSelect("", "", 30, 1, "").ToList().ToDataSourceResult(request);
            return Json(q);
        }


        public ActionResult Save(Models.sp_tblPatternSelect Pattern, string[] _checked)
        {
            try
            {

                Models.AutomationEntities p = new Models.AutomationEntities();
                if (Pattern.fldDesc == null)
                    Pattern.fldDesc = "";
                if (Pattern.fldID == 0)
                {//ثبت رکورد جدید
                    System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldID", typeof(int));
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 46))
                    {
                        p.sp_tblPatternInsert(_id,Pattern.fldType, 1, Pattern.fldDesc, "");
                        if (_checked != null)
                        {
                            for (int i = 0; i < _checked.Count(); i++)
                            {
                                p.sp_tblLetterPattern_GroupInsert( Convert.ToInt64(_id.Value),Convert.ToInt32(_checked[i]), Convert.ToInt32(Session["UserId"]), "");
                            }
                        }
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
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 47))
                    {
                        p.sp_tblPatternUpdate(Pattern.fldID, Pattern.fldType, 1, Pattern.fldDesc, "");
                        p.sp_tblLetterPattern_GroupDelete(Convert.ToInt32(Pattern.fldID), Convert.ToInt32(Session["UserId"]));
                        if (_checked != null)
                        {
                            for (int i = 0; i < _checked.Count(); i++)
                            {
                                p.sp_tblLetterPattern_GroupInsert(Pattern.fldID,Convert.ToInt32(_checked[i]),  Convert.ToInt32(Session["UserId"]), "");
                            }
                        }
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

        public ActionResult Reload(string field, string value, int top, int searchtype)
        {//جستجو
            string[] _fiald = new string[] { "fldType" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblPatternSelect(_fiald[Convert.ToInt32(field)], searchtext, top, 1, "").ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string id)
        {//حذف یک رکورد
            try
            {
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 48))
                {
                    Models.AutomationEntities Car = new Models.AutomationEntities();
                    if (Convert.ToInt32(id) != 0)
                    {
                        Car.sp_tblLetterPattern_GroupDelete(Convert.ToInt32(id), Convert.ToInt32(Session["UserId"]));
                        Car.sp_tblPatternDelete(Convert.ToInt32(id), Convert.ToInt32(Session["UserId"]), "");
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
        public JsonResult Details(int id)
        {//نمایش اطلاعات جهت رویت کاربر
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblPatternSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                var _checked = p.sp_tblLetterPattern_GroupSelect("fldPatternId", q.fldID.ToString(), 0).ToList();
                int[] checkedNodes = new int[_checked.Count];
                for (int i = 0; i < _checked.Count; i++)
                {
                    checkedNodes[i] = Convert.ToInt32(_checked[i].fldUserGroupId);
                }
                return Json(new
                {
                    fldID = q.fldID,
                    fldType = q.fldType,
                    fldDesc = q.fldDesc,
                    checkedNodes = checkedNodes
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

    }
}
