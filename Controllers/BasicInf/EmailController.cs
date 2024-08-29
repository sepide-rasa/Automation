using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Automation.Controllers.Users;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Automation.Controllers.BasicInf
{
    [Authorize]
    public class EmailController : Controller
    {
        //
        // GET: /Email/

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 97))
            {
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }
        public ActionResult EmailStaff()
        {//برگرداندن عکس 
            Models.AutomationEntities p = new Models.AutomationEntities();
            var user = p.sp_tblUserSelect("fldId", Session["UserId"].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var staff = p.sp_tblStaffSelect("fldId", user.fldStaffID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var email = p.sp_tblEmailSelect("fldStaffID", staff.fldID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            int id = 0;
            bool sendTrue_false = false;
            if (email != null)
            {
                id = email.fldID;
                sendTrue_false = email.fldSendTrue_False;
            }
            return Json(new
            {
                fldID = id,
                fldEmailAddress = staff.fldEmailAddress,
                fldSendTrue_False = sendTrue_false,
                fldStaffId = staff.fldID
            }, JsonRequestBehavior.AllowGet);

        }
        //public ActionResult Details()
        //{//برگرداندن عکس 
        //    Models.AutomationEntities p = new Models.AutomationEntities();

        //    var EmailAdress = p.sp_tblStaffUpdateEmail(fldUserId, "", 1).FirstOrDefault();

        //    return Json(new { fldID = EmailAdress.fldID, fldEmailAddress = EmailAdress.fldEmailAddress }, JsonRequestBehavior.AllowGet);

        //}
        public ActionResult Save(Models.Email Email)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (Email.fldDesc == null)
                    Email.fldDesc = "";
                if (Email.fldID == 0)
                {//ثبت رکورد جدید
                        p.sp_tblStaffUpdateEmail(Email.fldStaffID, Email.fldEmailAddress, Convert.ToInt32(Session["UserId"]));
                        p.sp_tblEmailInsert(Email.fldStaffID, Email.fldSendTrue_False, Convert.ToInt32(Session["UserId"]), Email.fldDesc, Session["UserPass"].ToString());
                        return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });
                    
                }
                else
                {//ویرایش رکورد ارسالی
                        p.sp_tblStaffUpdateEmail(Email.fldStaffID, Email.fldEmailAddress, 1);
                        p.sp_tblEmailUpdate(Email.fldID, Email.fldStaffID, Email.fldSendTrue_False, Convert.ToInt32(Session["UserId"]), Email.fldDesc, Session["UserPass"].ToString());
                        return Json(new { data = "ویرایش با موفقیت انجام شد.", state = 0, EmailAdress = Email.fldEmailAddress });
                    
                }
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

        //public JsonResult Details(int id)
        //{//نمایش اطلاعات جهت رویت کاربر
        //    try
        //    {
        //        Models.AutomationEntities p = new Models.AutomationEntities();
        //        var q = p.sp_tblStaffUpdateEmail(
        //        return Json(new
        //        {
        //            fldID = q.fldID,
        //            fldType = q.fldType,
        //            fldDesc = q.fldDesc
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception x)
        //    {
        //        return Json(new { data = x.InnerException.Message, state = 1 });
        //    }
        //}

        bool invalid = false;

        public JsonResult IsValidEmail(string strIn)
        {
            if (String.IsNullOrEmpty(strIn))
                invalid = false;

            else
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", this.DomainMapper, RegexOptions.None);

                invalid = Regex.IsMatch(strIn, @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                                        @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$", RegexOptions.IgnoreCase);
            }
                return Json(new
                {
                    valid = invalid,Email=strIn
                }, JsonRequestBehavior.AllowGet);
        }

        private string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }
    }
}
