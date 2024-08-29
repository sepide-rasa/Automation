using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using Automation.Controllers.Users;

namespace Automation.Controllers
{
    public class NameTableController : Controller
    {
        //
        // GET: /NameTable/

        public ActionResult Index()
        {//بارگذاری صفحه اصلی 
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 113))
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
            var q = m.sp_NameTableSelect().ToList().ToDataSourceResult(request);
            return Json(q);
        }
        public ActionResult FileExport(int id, string start, string end)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            GridView gv = new GridView();
            if (id == 1)
            {
                gv.DataSource = m.sp_tblCommision_LogSelect(start,end).ToList();
            }
            else if (id == 2)
            {
                gv.DataSource = m.sp_tblArchive_LogSelect(start,end).ToList();
            }
            else if (id == 3)
            {
                gv.DataSource = m.sp_tblAssignmentRole_LogSelect(start,end).ToList();
                    
            }
            else if (id == 4)
            {
                gv.DataSource = m.sp_tblOrganicRole_LogSelect(start, end).ToList();
            }
            else if (id == 5)
            {
                gv.DataSource = m.sp_tblStaff_LogSelect(start, end).ToList();
            }
            else if (id == 6)
            {
                gv.DataSource = m.sp_tblOrganizationUnit_LogSelect(start, end).ToList();
            }
            else if (id == 7)
            {
                gv.DataSource = m.sp_tblSecurityType_LogSelect(start, end).ToList();
            }
            else if (id == 8)
            {
                gv.DataSource = m.sp_tblSecretariat_LogSelect(start, end).ToList();
            }
            else if (id == 9)
            {
                gv.DataSource = m.sp_tblProgramSetting_LogSelect(start, end).ToList();
            }
            else if (id == 10)
            {
                gv.DataSource = m.sp_tblUser_LogSelect(start, end).ToList();
            }
            else if (id == 11)
            {
                gv.DataSource = m.sp_tblLetterExcel_LogSelect(start, end).ToList();
            }
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=پیش نمایش.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            Response.ContentEncoding = System.Text.Encoding.Unicode;
            Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            gv.RenderControl(htw);
            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();
            return View();
        }

    }
}
