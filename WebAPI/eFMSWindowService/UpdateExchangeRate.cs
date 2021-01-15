using eFMSWindowService.Helpers;
using eFMSWindowService.Models;
using System;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Timers;

namespace eFMSWindowService
{
    public partial class UpdateExchangeRate : ServiceBase
    {
        Timer _aTimer;
        DateTime _scheduleTime;

        public UpdateExchangeRate()
        {
            InitializeComponent();
            _aTimer = new Timer(30000);
            _scheduleTime = DateTime.Today.AddDays(1);
        }

        public void Start()
        {
            _aTimer.Start();
            _aTimer.Enabled = true;
            
            // Execute mỗi 1 hour
            var tillNextInterval = int.Parse(ConfigurationManager.AppSettings["intervalExchangeRate"].ToString());
            _aTimer.Interval = tillNextInterval;
            _aTimer.Elapsed += _aTimer_Elapsed;
        }

        //Custom method to Stop the timer
        public new void Stop()
        {
            FileHelper.WriteToFile("ServiceUpdateExchangeRate", "Service update exchange rate is stopped at " + DateTime.Now);
            _aTimer.Stop();
            _aTimer.Dispose();
        }

        private void _aTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            FileHelper.WriteToFile("ServiceUpdateExchangeRate", "Service update exchange rate is recall at " + DateTime.Now);
            using (eFMSTestEntities db = new eFMSTestEntities())
            {
                var result = db.Database.SqlQuery<int>("[dbo].[sp_AutoUpdateExchangeRate]").FirstOrDefault();
                FileHelper.WriteToFile("ServiceUpdateExchangeRate", DateTime.Now + " - Total number of affected rows: " + result);
            }
        }

        protected override void OnStart(string[] args)
        {
            FileHelper.WriteToFile("ServiceUpdateExchangeRate", "Service update exchange rate is started at " + DateTime.Now);
            this.Start();
        }

        protected override void OnStop()
        {
            this.Stop();
        }
    }
}
