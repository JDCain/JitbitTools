using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace Jitbit.WebJob
{
    public class Functions
    {
        [Singleton]
        public static void CalJob(
            [TimerTrigger("00:15:00", RunOnStartup = true, UseMonitor = true)] TimerInfo timerInfo)
        {
        }

    }
}
