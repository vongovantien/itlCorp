using eFMSWindowService.Helpers;
using eFMSWindowService.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace eFMSWindowService
{
    partial class AlertCustomerContractCashWithOutstandingDebit : ServiceBase
    {
        Timer _timer;
        DateTime scheduledTime;
        public AlertCustomerContractCashWithOutstandingDebit()
        {
            InitializeComponent();
            SetSchedule(DateTime.MinValue);
        }

        protected override void OnStart(string[] args)
        {
            this.Start();
        }

        public void Start()
        {
            FileHelper.WriteToFile("SendMailContractCashWithOutstandingDebit", "[SendMailContractCashWithOutstandingDebit] [START]:" + DateTime.Now);
            scheduledTime = ScheduleSetting.GetValidDate();
            Timer_Elapsed();
        }

        private void SetSchedule(DateTime _dtOption)
        {
            List<ScheduleTime> _scheduled = new List<ScheduleTime>();
            _scheduled.Add(new ScheduleTime() { DateOfWeek = DayOfWeek.Monday, Time = "03:00", DateTimeOption = _dtOption });
            _scheduled.Add(new ScheduleTime() { DateOfWeek = DayOfWeek.Thursday, Time = "03:00", DateTimeOption = _dtOption });
            ScheduleSetting.SetDateTimeSchedule(_scheduled);
        }

        private void ChangeIntervalTime(DateTime currentTime)
        {
            SetSchedule(currentTime);
            scheduledTime = ScheduleSetting.GetValidDate();
            this.Timer_Elapsed();
        }

        private void Timer_Elapsed()
        {
            try
            {
                _timer = new Timer(new TimerCallback(SchedularCallback));
                if(scheduledTime == null)
                {
                    SetSchedule(DateTime.MinValue);
                }
                var isDiff = ScheduleSetting.IsTimeToRun(scheduledTime);
                if (!isDiff)
                {
                    //If Scheduled Time is passed set Schedule for the next day.
                    var dueTime = ScheduleSetting.GetDueTime(scheduledTime);

                    //Change the Timer's Due Time.
                    _timer.Change(dueTime, Timeout.Infinite);
                }
                else
                {
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri(ConfigurationManager.AppSettings["API_URL"]);
                    client.Timeout = TimeSpan.FromMinutes(5);
                    var response = client.GetAsync(URLSetting.Accounting.ContractCashWithOutstandingDebitUrl).Result;
                    var result = response.Content.ReadAsStringAsync().Result;
                    FileHelper.WriteToFile("SendMailContractCashWithOutstandingDebit", "[SendMailContractCashWithOutstandingDebit] [CALL_API]:" + DateTime.Now.ToString() + " - Result:" + result.ToString());
                    ChangeIntervalTime(DateTime.Now);
                }

            }
            catch (Exception ex)
            {
                FileHelper.WriteToFile("SendMailContractCashWithOutstandingDebit", "[ERROR][Timer_Elapsed]:" + ex.Message);
                throw ex;
            }
        }

        private void SchedularCallback(object e)
        {
            FileHelper.WriteToFile("SendMailContractCashWithOutstandingDebit", "[SendMailContractCashWithOutstandingDebit] [RECALL AT]:" + DateTime.Now);
            this.Timer_Elapsed();
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
            this.Stop();
        }
        public new void Stop()
        {
            FileHelper.WriteToFile("SendMailContractCashWithOutstandingDebit", "[SendMailContractCashWithOutstandingDebit] [STOP]:" + DateTime.Now);
            _timer.Dispose();
        }
    }
}
