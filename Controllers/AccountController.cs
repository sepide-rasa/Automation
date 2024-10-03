using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Automation.Models;
using System.Net.NetworkInformation;
using System.IO;
using DotNetOpenAuth.Messaging;
using System.Net;
namespace Automation.Controllers
{
    
    public class AccountController : Controller
    {

        // من اینجا تغییر دادم بببین
        // GET: /Account/LogOn
        public FileContentResult generateCaptcha()
        {
            System.Drawing.FontFamily family = new System.Drawing.FontFamily("tahoma");
            CaptchaImage img = new CaptchaImage(90, 40, family);
            string text = img.CreateRandomText(5);
            img.SetText(text);
            img.GenerateImage();
            MemoryStream stream = new MemoryStream();
            img.Image.Save(stream,
            System.Drawing.Imaging.ImageFormat.Png);
            Session["captchaText"] = text;
            return File(stream.ToArray(), "jpg");
        }

        public ActionResult LogOn()
        {
            return PartialView();
        }

        
        
        //
        // POST: /Account/LogOn        

        public ActionResult GetYear()
        {
            List<SelectListItem> sal = new List<SelectListItem>();
            for (int i =1392; i <= 1410; i++)
            {
                SelectListItem item = new SelectListItem();
                item.Text = i.ToString();
                item.Value = i.ToString();
                sal.Add(item);
            }
            return Json(sal.Select(p1 => new { fldID = p1.Value, fldName = p1.Text }), JsonRequestBehavior.AllowGet);
        }
        public FileContentResult RandomPic()
        {//برگرداندن عکس 
            Random s = new Random();
            return File((byte[])System.IO.File.ReadAllBytes(Server.MapPath(@"~\Content\Login\" + s.Next(1, 9) + ".jpg")), "jpg");

        }
        public bool ValidateUser(string username, string password)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            string newpass= password.Replace('ي', 'ی');
            newpass = newpass.Replace('ك', 'ک');

            if (username != null & password != null)
            {
                var q = p.sp_tblUserSelect("cheakPass", username, 1, 1, newpass.GetHashCode().ToString()).ToList();
                return q.Any();
            }
            else
                return false;
        }

        [HttpPost]
        public ActionResult LogOn(Models.LogOnModel model, string returnUrl)
        {
            //if (ModelState.IsValid)
            //{
            
            if (model.Captcha != Session["captchaText"].ToString())
            {
                ModelState.AddModelError("", "لطفا کد امنیتی را صحیح وارد نمایید.");
                return PartialView(model);
            }
            if (ValidateUser(model.UserName, model.Password))
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblUserSelect("cheakPass", model.UserName, 1, 1, model.Password.GetHashCode().ToString()).FirstOrDefault();
                if (q.fldActive_Deactive)
                {                    
                    Session["UserId"] = q.fldID;
                    Session["Year"] = MyLib.Shamsi.Miladi2ShamsiString(p.sp_GetDate().FirstOrDefault().fldDateTime).Substring(0, 4);
                    Session["UserPass"] = model.Password.GetHashCode().ToString();
                    string user = model.UserName;
                    Session["UserName"] = user;
                    p.sp_tblInputInfoInsert(q.fldStaffID, p.sp_GetDate().FirstOrDefault().fldDateTime, Request.ServerVariables["REMOTE_HOST"].ToString(), "", Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "شما مجاز به ورود نمی باشید.");
                    return PartialView(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "نام کاربری یا کلمه عبور صحیح نیست.");
                return PartialView(model);
            }
            
            //}

            // If we got this far, something failed, redisplay form
            //return View(model);
        }
        //
        // GET: /Account/LogOff
        
        public ActionResult LogOff()
        {
            if (Session["UserId"] != null)
            {
                Models.OnlineUser.RemoveOnlineUser(Session["UserId"].ToString());
                Session.RemoveAll();
                //Session.Remove("UserId");
                //Session.Remove("UserPass");
                //Session.Remove("UserMnu");
                //Session.Remove("UserState");
                //Session.Remove("UserName");
                //Session.Remove("Location");
                //Session.Remove("area");
                //Session.Remove("office");
            }
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        public ActionResult Register(Models.RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                Membership.CreateUser(model.UserName, model.Password, model.Email, null, null, true, null, out createStatus);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, false /* createPersistentCookie */);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePassword

        [Authorize]
        public ActionResult ChangePassword()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account"); 
            return View();
        }

        //
        // POST: /Account/ChangePassword

        
        [HttpPost]
        [Authorize]
        public ActionResult ChangePassword(Models.ChangePasswordModel model)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account"); 
            try
            {
                if (model.OldPassword.GetHashCode().ToString() == Session["UserPass"].ToString())
                {
                    if (model.NewPassword == model.ConfirmPassword)
                    {
                        Models.AutomationEntities p = new AutomationEntities();
                        //p.sp_UserPassUpdate(Convert.ToInt32(Session["UserId"]), model.NewPassword.GetHashCode().ToString());
                        return Json(new { data = "تغییر رمز با موفقیت انجام شد.", state = 0 });
                    }
                    else
                    {
                        return Json(new { data = "رمز جدید با تکرار آن برابر نیست.", state = 1 });
                    }
                }
                else
                {
                    return Json(new { data = "رمز قدیم نادرست است.", state = 1 });
                }
            }
            catch (Exception x)
            {
                
                return Json(new { data = x.Message, state = 1 });
            }
        }

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
