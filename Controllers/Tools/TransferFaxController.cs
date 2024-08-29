using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Automation.Controllers.Users;
using System.Drawing;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace Automation.Controllers.Tools
{
    public class TransferFaxController : Controller
    {
        //
        // GET: /TransferFax/

        public ActionResult Index() 
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 110))
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblProgramSettingSelect("", "", 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();

                if (!q.fldDelFax)
                {
                    var s=p.sp_FaxTempSelect("", "",0).ToList();
                    foreach (var Item in s)
                        p.sp_FaxTempDelete(Item.fldId, Convert.ToInt32(Session["UserId"]));
                }

                string _Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //string TiffPath = _Path + "\\FaxDocument";
                string TiffPath =q.fldFaxPath;
                //if (!Directory.Exists(savePath))
                //{
                //    return Json(new { data = "فایلی با نام FaxDocument در شاخه Documents موجود نیست.  ", state = 1 }, JsonRequestBehavior.AllowGet);
                //}

                Session["TiffPath"] = TiffPath;

                DirectoryInfo dirInfo = new DirectoryInfo(TiffPath);
                FileInfo[] Tfiles = dirInfo.GetFiles().Where(file => (file.Attributes & FileAttributes.Hidden) == 0).ToArray();

                //string[] Tfiles = System.IO.Directory.GetFiles(TiffPath);

                //Instantiate a Pdf object
                
                int i = 0;
                //navigate through the files and them in the pdf file
                foreach (var myFile in Tfiles)
                {
                    Aspose.Pdf.Generator.Pdf pdf1 = new Aspose.Pdf.Generator.Pdf();
                    i++;
                    string pp = Path.GetExtension(myFile.FullName).Replace(".", "");
                    string FileName = myFile.FullName.Substring(0);
                    string FileName1 = Path.GetFileNameWithoutExtension(myFile.FullName);

                    FileStream fs = new FileStream(myFile.FullName, FileMode.Open, FileAccess.Read);
                    byte[] tmpBytes = new byte[fs.Length];
                    fs.Read(tmpBytes, 0, Convert.ToInt32(fs.Length));

                    MemoryStream mystream = new MemoryStream(tmpBytes);

                    if (pp != "pdf")
                    {
                        Bitmap b = new Bitmap(mystream);
                        //Create a new section in the Pdf document
                        Aspose.Pdf.Generator.Section sec1 = new Aspose.Pdf.Generator.Section(pdf1);

                        // Set margins so image will fit, etc.
                        sec1.PageInfo.Margin.Top = 5;
                        sec1.PageInfo.Margin.Bottom = 5;
                        sec1.PageInfo.Margin.Left = 5;
                        sec1.PageInfo.Margin.Right = 5;

                        sec1.PageInfo.PageWidth = (b.Width / b.HorizontalResolution) * 72;
                        sec1.PageInfo.PageHeight = (b.Height / b.VerticalResolution) * 72;

                        //Add the section in the sections collection of the Pdf document
                        pdf1.Sections.Add(sec1);

                        //Create an image object
                        Aspose.Pdf.Generator.Image image1 = new Aspose.Pdf.Generator.Image(sec1);

                        //Add the image into paragraphs collection of the section
                        sec1.Paragraphs.Add(image1);
                        image1.ImageInfo.ImageFileType = Aspose.Pdf.Generator.ImageFileType.Tiff;

                        // set IsBlackWhite property to true for performance improvement
                        image1.ImageInfo.IsBlackWhite = false;
                        //Set the ImageStream to a MemoryStream object
                        image1.ImageInfo.ImageStream = mystream;
                        //Set desired image scale
                        image1.ImageScale = 0.95F;

                        //Save the Pdf
                        pdf1.Save(Server.MapPath(@"~\Uploaded\" + FileName1.ToString() + ".pdf"));
                    }
                    else
                    {
                        System.IO.File.Copy(FileName, Server.MapPath(@"~\Uploaded\" + FileName1.ToString() + ".pdf"));
                    }
                    fs.Close();

                    if (q.fldDelFax)//فکس ها از پوشه اصلی پاک شوند
                        System.IO.File.Delete(FileName);
                }
                

                string savePath = Server.MapPath(@"~\Uploaded\");
                string[] files = System.IO.Directory.GetFiles(savePath);


                foreach (string fileName in files)
                {
                    MemoryStream stream = new MemoryStream(System.IO.File.ReadAllBytes(fileName));
                    byte[] _File = stream.ToArray();
                    string FileName1 = Path.GetFileNameWithoutExtension(fileName);
                    p.sp_FaxTempInsert(FileName1, _File, Convert.ToInt32(Session["UserId"]), "");
                    System.IO.File.Delete(fileName);
                }
                //Session.Remove("savePath");


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
            var q = m.sp_FaxTempSelect("", "", 0).Select(k => new { k.fldId,k.fldTitle}).ToList().ToDataSourceResult(request);
            return Json(q);
        }
        public ActionResult Reload()
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q1 = p.sp_FaxTempSelect("", "", 0).Select(k1 => new { k1.fldId, k1.fldTitle }).ToList();
                return Json(q1, JsonRequestBehavior.AllowGet);

        }
        //public string listImage()
        //{
        //    Models.AutomationEntities p = new Models.AutomationEntities();
        //    var F = p.sp_FaxTempSelect("", "", 0).ToList(); ;
        //    string Pdf = "";

        //    foreach (var item in F)
        //    {
        //        string p1 = Url.Content("~/TransferFax/GeneratePDF/" + item.fldId);

        //        Pdf += "<input type='checkbox' value='" + item.fldId + "' id='" + item.fldId + "' /></br><object id='pdfbox' type='application/pdf' data='" + p1 + "'></object></br>";
        //    }
        //    return (Pdf);

        //}

        public ActionResult GeneratePDF(int? id)
        {
            try
            {
                //byte[] report_file = null;

                Models.AutomationEntities p = new Models.AutomationEntities();
                var Content = p.sp_FaxTempSelect("fldId", id.ToString(), 0).FirstOrDefault();

                if (Content != null)
                    return File(Content.fldFile, "application/pdf");

                else
                    return null;
            }
            catch (Exception x)
            {
                return Json(x.Message.ToString(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Delete(string id)
        {//حذف یک رکورد
            try
            {
                    Models.AutomationEntities Car = new Models.AutomationEntities();
                    if (Convert.ToInt32(id) != 0)
                    {
                        Car.sp_FaxTempDelete(Convert.ToInt32(id), Convert.ToInt32(Session["UserId"]));
                        return Json(new { data = "حذف با موفقیت انجام شد.", state = 0 });

                    }
                    else
                    {
                        return Json(new { data = "رکوردی برای حذف انتخاب نشده است.", state = 1 });
                    }
                
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
    }
}
