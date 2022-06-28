using eFMSWindowService.Helpers;
using eFMSWindowService.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
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
            _scheduleTime = DateTime.Today.AddDays(1).AddHours(3);

        }
        public void Start()
        {
            FileHelper.WriteToFile("OverduePaymentService", "[LogUpdateOverDueService] [START]:" + DateTime.Now);
            // Tạo 1 timer từ libary System.Timers
            _timer = new Timer();
            // Execute mỗi ngày vào lúc 3h sáng
            _timer.Interval = _scheduleTime.Subtract(DateTime.Now).TotalSeconds * 1000;
            //_timer.Interval = 7000; 
            // Những gì xảy ra khi timer đó dc tick
            _timer.Elapsed += Timer_Elapsed;
            // Enable timer
            _timer.Enabled = true;
        }
        protected override void OnStart(string[] args)
        {
            this.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                //using (eFMSTestEntities db = new eFMSTestEntities())
                //{
                //    List<sp_GetOverDuePayment> over_15 = db.Database.SqlQuery<sp_GetOverDuePayment>("[dbo].[sp_CalculateInvoice15DaysOverDueReceivable]").ToList();
                //    List<sp_GetOverDuePayment> over_15_30 = db.Database.SqlQuery<sp_GetOverDuePayment>("[dbo].[sp_CalculateInvoice15To30DaysOverDueReceivable]").ToList();
                //    List<sp_GetOverDuePayment> over_30 = db.Database.SqlQuery<sp_GetOverDuePayment>("[dbo].[sp_CalculateInvoice30DaysOverDueReceivable]").ToList();

                //    string log_15 = JsonConvert.SerializeObject(over_15);
                //    string log_15_30 = JsonConvert.SerializeObject(over_15_30);
                //    string log_30 = JsonConvert.SerializeObject(over_30);

                //    FileHelper.WriteToFile("OverduePaymentService", "[LogUpdateOverDue15Service] [Updated]:" + DateTime.Now + "\n" + "=======" + "\n" + log_15 + "\n" + "======= ");
                //    FileHelper.WriteToFile("OverduePaymentService", "[LogUpdateOverDue15_30Service] [Updated]:" + DateTime.Now + "\n" + "=======" + "\n" + log_15_30 + "\n" + "======= ");
                //    FileHelper.WriteToFile("OverduePaymentService", "[LogUpdateOverDue30Service] [Updated]:" + DateTime.Now + "\n" + "=======" + "\n" + log_30 + "\n" + "======= ");


                FileHelper.WriteToFile("OverduePaymentService", "[LogUpdateOverDueService] [CALL]:" + DateTime.Now);

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["API_URL"]);
                
                HttpResponseMessage response1 = client.PutAsync("Accounting/api/v1/vi/AccountReceivable/CalculateOverDue1To15", new StringContent(JsonConvert.SerializeObject(new string[] {}), Encoding.UTF8, "application/json")).Result;
                HttpResponseMessage response2 = client.PutAsync("Accounting/api/v1/vi/AccountReceivable/CalculateOverDue15To30", new StringContent(JsonConvert.SerializeObject(new string[] {}), Encoding.UTF8, "application/json")).Result;
                HttpResponseMessage response3 = client.PutAsync("Accounting/api/v1/vi/AccountReceivable/CalculateOverDue30", new StringContent(JsonConvert.SerializeObject(new string[] {}), Encoding.UTF8, "application/json")).Result;

                var dto1 = response1.Content.ReadAsStringAsync().Result;
                var dto2 = response2.Content.ReadAsStringAsync().Result;
                var dto3 = response3.Content.ReadAsStringAsync().Result;

                FileHelper.WriteToFile("OverduePaymentService", "[LogUpdateOverDue1To15Service] [Result]:" + dto1.ToString());
                FileHelper.WriteToFile("OverduePaymentService", "[LogUpdateOverDue15To30Service] [Result]:" + dto2.ToString());
                FileHelper.WriteToFile("OverduePaymentService", "[LogUpdateOverDue30Service] [Result]:" + dto3.ToString());

                // reset lại 1 ngày
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
