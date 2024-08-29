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
    public class ProgramSettingController : Controller
    {
        //
        // GET: /ProgramSetting/

        public ActionResult Index()
        {//بارگذاری صفحه اصلی  
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 39))
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
            var q = m.sp_tblProgramSettingSelect("", "", 30, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().ToDataSourceResult(request);
            return Json(q);
        }

        public ActionResult Save(Models.ProgramSetting ProgramSetting)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();

                byte[] image = null;
                    if (ProgramSetting.fldLogo != null)
                        image = Automation.Helper.ClsCommon.Base64ToImage(ProgramSetting.fldLogo);
                //if (ProgramSetting.fldID == 0)
                //{//ثبت رکورد جدید
                    
                //    p.sp_tblProgramSettingInsert(ProgramSetting.fldName, ProgramSetting.fldEmailAddress, ProgramSetting.fldEmailPassword, image, 1, ProgramSetting.fldDesc, "");

                //    return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });

                //}
                //else
                //{//ویرایش رکورد ارسالی   
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 53))
                    {
                        p.sp_tblProgramSettingUpdate(ProgramSetting.fldID, ProgramSetting.fldName, ProgramSetting.fldEmailAddress, ProgramSetting.fldEmailPassword, image, ProgramSetting.fldRecieveServer, ProgramSetting.fldRecievePort, ProgramSetting.fldSendServer, ProgramSetting.fldSendPort, ProgramSetting.fldSSL, ProgramSetting.fldDelFax, ProgramSetting.fldIsSigner, ProgramSetting.fldFaxPath, Convert.ToInt32(Session["UserId"]), ProgramSetting.fldDesc, Session["UserPass"].ToString());
                        p.sp_Email_InstallConfiguration();
                        p.sp_EmailProfileAndAccount_Create(ProgramSetting.fldEmailAddress, "", "", ProgramSetting.fldSendServer, ProgramSetting.fldSendPort, ProgramSetting.fldEmailAddress, ProgramSetting.fldEmailPassword, ProgramSetting.fldSSL);
                        return Json(new { data = "ویرایش با موفقیت انجام شد.", state = 0 });
                    }
                    else
                    {
                        Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                        return RedirectToAction("error", "Metro");
                    }
                }
            //}
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

        public ActionResult Reload(string field, string value, int top, int searchtype)
        {//جستجو
            string[] _fiald = new string[] { "fldEmailAddress"};
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblProgramSettingSelect(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string id)
        {//حذف یک رکورد
            try
            {
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 54))
                {
                    Models.AutomationEntities Car = new Models.AutomationEntities();
                    if (Convert.ToInt32(id) != 0)
                    {
                        Car.sp_tblProgramSettingDelete(Convert.ToInt32(id), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
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
                var q = p.sp_tblProgramSettingSelect("fldID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                return Json(new
                {
                    fldID = q.fldID,
                    fldName = q.fldName,  
                    fldEmailAddress=q.fldEmailAddress,
                    fldEmailPassword=q.fldEmailPassword,
                    fldRecievePort= q.fldRecievePort,
                    fldRecieveServer=q.fldRecieveServer,
                    fldSendPort=q.fldSendPort,
                    fldSendServer=q.fldSendServer,
                    fldSSL=q.fldSSL,
                    fldDelFax=q.fldDelFax,
                    fldIsSigner = q.fldIsSigner,
                    fldFaxPath=q.fldFaxPath,
                    fldDesc = q.fldDesc

                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

        public FileContentResult Image(int id)
        {//برگرداندن عکس 
            Models.AutomationEntities p = new Models.AutomationEntities();

            var pic = p.sp_tblProgramSettingSelect("fldID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            if (pic != null)
            {
                if (pic.fldLogo != null)
                {
                    return File((byte[])pic.fldLogo, "jpg"); 
                }
            }
            return null;

        }
    }
}
