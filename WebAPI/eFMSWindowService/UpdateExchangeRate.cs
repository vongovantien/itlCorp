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
        Timer _timer;
        DateTime _scheduleTime;

        public UpdateExchangeRate()
        {
            InitializeComponent();
            _scheduleTime = DateTime.Today.AddDays(1).AddHours(1);
        }

        public void Start()
        {
             // Tạo 1 timer từ libary System.Timers
            _timer = new Timer();
            // Execute mỗi ngày vào lúc 1h sáng
            _timer.Interval = _scheduleTime.Subtract(DateTime.Now).TotalSeconds * 1000;
            // _timer.Interval = 30000;
            // Những gì xảy ra khi timer đó dc tick
            _timer.Elapsed += Timer_Elapsed;
            // Enable timer
            _timer.Enabled = true;
        }

        //Custom method to Stop the timer
        public new void Stop()
        {
            FileHelper.WriteToFile("ServiceUpdateExchangeRate", "Service update exchange rate is stopped at " + DateTime.Now);
            _timer.Stop();
            _timer.Dispose();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                FileHelper.WriteToFile("ServiceUpdateExchangeRate", "Service update exchange rate is recall at " + DateTime.Now);
                using (eFMSTestEntities db = new eFMSTestEntities())
                {
                    var result = db.Database.SqlQuery<int>("[dbo].[sp_AutoUpdateExchangeRate]").FirstOrDefault();
                    FileHelper.WriteToFile("ServiceUpdateExchangeRate", DateTime.Now + " - Total number of affected rows: " + result);
                }
                if (_timer.Interval != 24 * 60 * 60 * 1000)
                {
                    _timer.Interval = 24 * 60 * 60 * 1000;
                }
            }
            catch (Exception)
            {
                throw;
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
