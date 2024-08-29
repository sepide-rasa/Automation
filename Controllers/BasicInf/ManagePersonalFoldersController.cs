using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Automation.Controllers.Users;

namespace Automation.Controllers.BasicInf
{
    [Authorize]
    public class ManagePersonalFoldersController : Controller
    {
        //
        // GET: /ManagePersonalFolders/

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 72))
            {
            return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }

        public JsonResult _FolderTree(int? id, int cId)
        {
            string url = Url.Content("~/Content/images/B");
            try
            {
                var p = new Models.AutomationEntities();
              

                if (id != null)
                {
                    var rols = (from k in p.sp_tblBoxSelect("fldPID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString())
                                where k.fldComisionID == cId
                                select new
                                {
                                    id = k.fldID,
                                    Name = k.fldName,
                                    image = url + k.fldBoxTypeID.ToString() + ".png",
                                    hasChildren = p.sp_tblBoxSelect("fldPID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var rols = (from k in p.sp_tblBoxSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString())
                                where k.fldComisionID == cId
                                select new
                                {
                                    id = k.fldID,
                                    Name = k.fldName,
                                    image = url + k.fldBoxTypeID.ToString() + ".png",
                                    hasChildren = p.sp_tblBoxSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception x)
            {
                return null;
            }
        }

        public ActionResult Save(Models.sp_tblBoxSelect folder)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var com = p.sp_tblBoxSelect("fldID", folder.fldPID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (folder.fldDesc == null)
                    folder.fldDesc = "";
                if (folder.fldID == 0)
                {//ثبت رکورد جدید
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 73))
                    {
                        p.sp_tblBoxInsert(folder.fldName, com.fldComisionID, 7, folder.fldPID, Convert.ToInt32(Session["UserId"]), folder.fldDesc, Session["UserPass"].ToString());
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
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 74))
                    {
                        if (folder.fldBoxTypeID == 7)
                        {
                            p.sp_tblBoxUpdate(folder.fldID, folder.fldName, com.fldComisionID, 7, folder.fldPID, Convert.ToInt32(Session["UserId"]), folder.fldDesc, Session["UserPass"].ToString());
                            return Json(new { data = "ویرایش با موفقیت انجام شد.", state = 0 });
                        }
                        else
                            return Json(new { data = "شما مجاز به ویرایش این پوشه نمی باشید.", state = 0 });
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
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 75))
                {
                    Models.AutomationEntities Car = new Models.AutomationEntities();
                    if (Convert.ToInt32(id) != 0)
                    {
                        var q = Car.sp_tblBoxSelect("fldId", id, 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                        if (q.fldBoxTypeID == 7)
                        {
                            Car.sp_tblBoxDelete(Convert.ToInt32(id), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                            return Json(new { data = "حذف با موفقیت انجام شد.", state = 0 });
                        }
                        else
                            return Json(new { data = "شما مجاز به حذف این پوشه نمی باشید.", state = 1 });
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
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblBoxSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                return Json(new
                {
                    fldName = q.fldName,
                    fldId = q.fldID,
                    fldPId = q.fldPID,
                    fldBoxTypeID = q.fldBoxTypeID,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return null;
            }
        }
    }
}