using eFMSWindowService.Helpers;
using eFMSWindowService.Models;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Timers;

namespace eFMSWindowService
{
    partial class UpdateStatusAuthorization : ServiceBase
    {
        Timer _aTimer;
        public UpdateStatusAuthorization()
        {
            InitializeComponent();
            _aTimer = new Timer(30000);
        }

        public void Start()
        {
            _aTimer.Start();
            _aTimer.Enabled = true;
            // Execute mỗi 1 day
            var tillNextInterval = int.Parse(ConfigurationManager.AppSettings["intervalAuthorization"].ToString());
            _aTimer.Interval = tillNextInterval;
            _aTimer.Elapsed += _aTimer_Elapsed;
        }

        //Custom method to Stop the timer
        public new void Stop()
        {
            FileHelper.WriteToFile("ServiceLog_UpdateStatusAuthorization", "Service update status authorization is stopped at " + DateTime.Now);
            _aTimer.Stop();
            _aTimer.Dispose();
        }

        private void _aTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            FileHelper.WriteToFile("ServiceLog_UpdateStatusAuthorization", "Service update status authorization is recall at " + DateTime.Now);
            using (eFMSTestEntities db = new eFMSTestEntities())
            {
                var result = db.Database.SqlQuery<int>("[dbo].[sp_UpdateInactiveStatusAuthorization]").FirstOrDefault();
                FileHelper.WriteToFile("ServiceLog_UpdateStatusAuthorization", DateTime.Now + " - Total number of affected rows: " + result);
            }
        }

        protected override void OnStart(string[] args)
        {
            FileHelper.WriteToFile("ServiceLog_UpdateStatusAuthorization", "Service update status authorization is started at " + DateTime.Now);
            this.Start();
        }

        protected override void OnStop()
        {
            this.Stop();
        }
    }
}
