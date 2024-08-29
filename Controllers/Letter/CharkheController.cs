using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Automation.Controllers
{
    [Authorize]
    public class CharkheController : Controller
    {
        //
        // GET: /Charkhe/

        public ActionResult Index(int id)
        {
            ViewBag.LetterId = id;
            return PartialView();
        }

        public JsonResult _RolsTree(int? id,int LetterId)
        {
            try
            {
                var p = new Models.AutomationEntities();
                if (id != null)
                {
                    var rols = (from k in p.sp_Charkhe(LetterId,id)
                                select new
                                {
                                    id = k.id,
                                    Name = k.SenderName,
                                    hasChildren = p.sp_Charkhe(LetterId, id).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var rols = (from k in p.sp_Charkhe(LetterId, null)
                                select new
                                {
                                    id = k.id,
                                    Name = k.SenderName,
                                    hasChildren = p.sp_Charkhe(LetterId, null).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception x)
            {
                return null;
            }
        }

    }
}
