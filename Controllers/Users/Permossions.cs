using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Controllers.Users
{
    public class Permossions
    {
        public static bool haveAccess(int userId, int RolId)
        {
            Models.AutomationEntities p = new Models.AutomationEntities();
            var q = p.sp_tblPermissionSelect("HaveAcces", RolId, userId, "").Any();
            return q;
        }   
    }
}