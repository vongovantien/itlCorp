using eFMSWindowService.Helpers;
using eFMSWindowService.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceProcess;
using System.Timers;


namespace eFMSWindowService
{
    partial class UpdateOverDuePaymentService : ServiceBase
    {
        Timer _timer;
        DateTime _scheduleTime;
        public UpdateOverDuePaymentService()
        {
            InitializeComponent();
            _scheduleTime = DateTime.Today.AddDays(1); 
        
        }

        protected override void OnStart(string[] args)
        {
            if (ConfigurationManager.AppSettings["LogUpdateOverDueService"] == "1")
                this.Start();
        }

        public void Start()
        {
            FileHelper.WriteToFile("OverduePaymentService", "[LogUpdateOverDueService] [START]:" + DateTime.Now);
            // Tạo 1 timer từ libary System.Timers
            _timer = new Timer();
            // Execute mỗi ngày vào lúc 0h sáng
            _timer.Interval = _scheduleTime.Subtract(DateTime.Now).TotalSeconds * 1000;         
            //_timer.Interval = 5000; 
            // Những gì xảy ra khi timer đó dc tick
            _timer.Elapsed += Timer_Elapsed;
            // Enable timer
            _timer.Enabled = true;
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                using (eFMSTestEntities db = new eFMSTestEntities())
                {
                    List<sp_GetOverDuePayment> over_15 = await db.Database.SqlQuery<sp_GetOverDuePayment>("[dbo].[sp_CalculateInvoice15DaysOverDueReceivable]").ToListAsync();
                    List<sp_GetOverDuePayment> over_15_30 = await db.Database.SqlQuery<sp_GetOverDuePayment>("[dbo].[sp_CalculateInvoice15To30DaysOverDueReceivable]").ToListAsync();
                    List<sp_GetOverDuePayment> over_30 = await db.Database.SqlQuery<sp_GetOverDuePayment>("[dbo].[sp_CalculateInvoice30DaysOverDueReceivable]").ToListAsync();

                    string log_15 = JsonConvert.SerializeObject(over_15);
                    string log_15_30 = JsonConvert.SerializeObject(over_15_30);
                    string log_30 = JsonConvert.SerializeObject(over_30);

                    FileHelper.WriteToFile("OverduePaymentService", "[LogUpdateOverDue15Service] [Updated]:" + DateTime.Now + "\n" + "=======" + "\n" + log_15 + "\n" + "======= ");
                    FileHelper.WriteToFile("OverduePaymentService", "[LogUpdateOverDue15_30Service] [Updated]:" + DateTime.Now + "\n" + "=======" + "\n" + log_15_30 + "\n" + "======= ");
                    FileHelper.WriteToFile("OverduePaymentService", "[LogUpdateOverDue30Service] [Updated]:" + DateTime.Now + "\n" + "=======" + "\n" + log_30 + "\n" + "======= ");
                }

                if (_timer.Interval != 24 * 60 * 60 * 1000)
                {
                    _timer.Interval = 24 * 60 * 60 * 1000;
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteToFile("OverduePaymentService", "[LogUpdateOverDueService] [ERROR]:" + DateTime.Now + "\n " + ex.ToString());
                throw;
            }
        }

        protected override void OnStop()
        {
            FileHelper.WriteToFile("OverduePaymentService", "[LogUpdateOverDueService] [STOP]:" + DateTime.Now);
            _timer.Stop();
            _timer.Dispose();
        }
    }
}
