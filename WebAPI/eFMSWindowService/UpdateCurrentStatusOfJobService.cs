using eFMSWindowService.Models;
using System;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Timers;
using System.Configuration;
using eFMSWindowService.Helpers;

namespace eFMSWindowService
{
    partial class UpdateCurrentStatusOfJobService : ServiceBase
    {
        private Timer timer = null;
        public UpdateCurrentStatusOfJobService()
        {
            InitializeComponent();
        }

        public void Start()
        {
            FileHelper.WriteToFile("ServiceLog_UpdateCurrentStatusOfJob", "Service is started at " + DateTime.Now);

            var _interval = int.Parse(ConfigurationManager.AppSettings["intervalCurrentStatus"]);
            // Tạo 1 timer từ libary System.Timers
            timer = new Timer();
            // Execute mỗi 2 minute
            timer.Interval = _interval;          
            // Những gì xảy ra khi timer đó dc tick
            timer.Elapsed += Timer_Elapsed;
            // Enable timer
            timer.Enabled = true;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            using (eFMSTestEntities db = new eFMSTestEntities())
            {
                var result = db.Database.SqlQuery<int>("[dbo].[sp_QueryAndUpdateCurrentStatusOfJob]").FirstOrDefault();
                FileHelper.WriteToFile("ServiceLog_UpdateCurrentStatusOfJob", DateTime.Now + " - Total number of affected rows: " + result);
            }                
        }

        protected override void OnStart(string[] args)
        {
            FileHelper.WriteToFile("ServiceLog_UpdateCurrentStatusOfJob", "Service update job status is stopped at " + DateTime.Now);
            this.Start();
        }

        public new void Stop()
        {
            FileHelper.WriteToFile("ServiceLog_UpdateCurrentStatusOfJob", "Service update job status is stopped at " + DateTime.Now);
            timer.Stop();
            timer.Dispose();
        }

        protected override void OnStop()
        {
            this.Stop();
        }
    }
}
