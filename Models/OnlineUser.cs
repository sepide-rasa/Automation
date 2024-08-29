using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Automation.Models
{
    public static class OnlineUser
    {
        public static List<LogOnModel> userObj = new List<LogOnModel>();
        
        public static void AddOnlineUser(string strConnectionId, string strUserName,string strUserId,string strSessionId)
        {
            LogOnModel user = new LogOnModel();
            user.connectionId = strConnectionId;
            user.UserName = strUserName;
            user.userId = strUserId;
            user.newStatus = true;
            user.sessionId = strSessionId;
            userObj.Add(user);
        }

        public static void RemoveOnlineUser(string strUserId)
        {
            var userRemove =(LogOnModel) userObj.Where(item => item.userId == strUserId).FirstOrDefault();
            userObj.Remove(userRemove);           
 
        }
    }
}
