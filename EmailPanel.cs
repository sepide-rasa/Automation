using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;

namespace Automation
{
    public class EmailPanel
    {
        private static readonly Timer _timer = new Timer(OnTimerElapsed);
        private static readonly EmailjobHost _jobHost = new EmailjobHost();

        public static void Start()
        {
            _timer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(300000));//5دقیقه
        }

        private static void OnTimerElapsed(object sender)
        {
            _jobHost.DoWork(() =>
            {
        
            });
        }
    }
}