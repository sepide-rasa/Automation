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
    public class UserController : Controller
    {
        //
        // GET: /UserSetting/

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 36))
            {
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }
        public JsonResult GetCascadeGroup()
        {
            Models.AutomationEntities q = new Models.AutomationEntities();
            var w = q.sp_tblUserGroupSelect("", "", 0).Select(c => new { fldID = c.fldID, fldName = c.fldTitle });
            return Json(w, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblUserSelect("", "", 30, 1, "").ToList().ToDataSourceResult(request);
            return Json(q);
        }
        ////public JsonResult GetInf(int idStaff)
        ////{

        ////    Models.AutomationEntities m = new Models.AutomationEntities();
        ////    var q = m.sp_tblStaffSelect("fldId", idStaff.ToString(), 1, 1, "").FirstOrDefault();
        ////    return Json(new
        ////    {
        ////        StaffName = q.fldName + " " + q.fldFamily,
        ////    }, JsonRequestBehavior.AllowGet);

        ////}
        public JsonResult Reset(int id)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblUserSelect("fldid", id.ToString(), 0, 1, "").FirstOrDefault();
                if (q != null)
                {

                    p.sp_UserPassUpdate(id, q.fldUserName.GetHashCode().ToString());
                    return Json(new { data = "تغییر رمز با موفقیت انجام شد.", state = 0 });
                }
                return Json(new { data = "کاربری با این مشخصات وجود ندارد.", state = 1 });

            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }


        public ActionResult Save(Models.User User, string[] _checked)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (User.fldDesc == null)
                    User.fldDesc = "";
                if (User.fldID == 0)
                {//ثبت رکورد جدید
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 43))
                    {
                        System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldID", typeof(int));
                        p.sp_tblUserInsert(_id, User.fldStaffID, User.fldUserName, User.fldPassword.GetHashCode().ToString(), Convert.ToBoolean(User.fldActive_Deactive), Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                        if (_checked != null)
                        {
                            for (int i = 0; i < _checked.Count(); i++)
                            {
                                p.sp_tblUser_GroupInsert(Convert.ToInt32(_checked[i]), Convert.ToInt64(_id.Value), Convert.ToInt32(Session["UserId"]), "");
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
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 44))
                    {
                        p.sp_tblUserUpdate(User.fldID, User.fldStaffID, User.fldUserName, User.fldPassword, Convert.ToBoolean(User.fldActive_Deactive), Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                        var k = p.sp_tblUser_GroupSelect("fldUserSelectID", User.fldID.ToString(), 0).ToList();
                        for (int i = 0; i < k.Count; i++)
                            p.sp_tblUser_GroupDelete(k[i].fldID, Convert.ToInt32(Session["UserId"]));
                        if (_checked != null)
                        {
                            for (int i = 0; i < _checked.Count(); i++)
                            {
                                p.sp_tblUser_GroupInsert(Convert.ToInt32(_checked[i]), User.fldID, Convert.ToInt32(Session["UserId"]), "");
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
            string[] _fiald = new string[] { "fldStaffName" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblUserSelect(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string id)
        {//حذف یک رکورد
            try
            {
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 45))
                {
                    Models.AutomationEntities Car = new Models.AutomationEntities();
                    if (Convert.ToInt32(id) != 0)
                    {
                        Car.sp_tblUserDelete(Convert.ToInt32(id), 1, "");
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
                var q = p.sp_tblUserSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                var _checked = p.sp_tblUser_GroupSelect("fldUserSelectID", q.fldID.ToString(), 0).ToList();
                int[] checkedNodes = new int[_checked.Count];
                for (int i = 0; i < _checked.Count; i++)
                {
                    checkedNodes[i] = Convert.ToInt32(_checked[i].fldUserGroupID);
                }
                return Json(new
                {
                    fldID = q.fldID,
                    fldUserName = q.fldUserName,
                    fldPassword = "",
                    fldStaffName = q.fldStaffName,
                    fldActive_Deactive = q.fldActive_Deactive,
                    fldDesc = q.fldDesc,
                    checkedNodes = checkedNodes,
                    fldStaffId = q.fldStaffID
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

    }
}
