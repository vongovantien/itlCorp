using eFMSWindowService.Helpers;
using eFMSWindowService.Models;
using System;
using System.Linq;
using System.ServiceProcess;
using System.Timers;

namespace eFMSWindowService
{
    partial class AutoLockShipmentService : ServiceBase
    {
        System.Timers.Timer _timer;
        DateTime _scheduleTime;
        public AutoLockShipmentService()
        {
            InitializeComponent();
            _scheduleTime = DateTime.Today.AddDays(1).AddSeconds(1);
        }
        public void Start()
        {
            // Tạo 1 timer từ libary System.Timers
            _timer = new Timer();
            // Execute mỗi ngày vào lúc 0h sang
            _timer.Interval = _scheduleTime.Subtract(DateTime.Now).TotalSeconds * 1000;
            // Những gì xảy ra khi timer đó dc tick
            _timer.Elapsed += Timer_Elapsed;
            // Enable timer
            _timer.Enabled = true;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                FileHelper.WriteToFile("AutoLockShipment", "Auto lock shipment is recall at " + DateTime.Now);
                using (eFMSTestEntities db = new eFMSTestEntities())
                {
                    var result = db.Database.SqlQuery<int>("[dbo].[sp_GetShipmentToAutoLock]").FirstOrDefault();
                    FileHelper.WriteToFile("AutoLockShipment", DateTime.Now + " - Total number of affected rows: " + result);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public new void Stop()
        {
            FileHelper.WriteToFile("AutoLockShipment", "Auto lock shipment is stopped at " + DateTime.Now);
            _timer.Stop();
            _timer.Dispose();
        }
        protected override void OnStart(string[] args)
        {
            this.Start();
        }

        protected override void OnStop()
        {
            this.Stop();
        }
    }
}
