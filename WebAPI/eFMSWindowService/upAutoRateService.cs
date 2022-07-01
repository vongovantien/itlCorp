﻿using eFMSWindowService.Helpers;
using eFMSWindowService.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace eFMSWindowService
{
    partial class UpAutoRateService : ServiceBase
    {
        Timer _timer;
        DateTime _scheduleTime;
        /// <summary>
        /// Send mail Vượt Hạn Mức Công Nợ
        /// </summary>
        public UpAutoRateService()
        {
            InitializeComponent();
        }

        public void Start()
        {
            _scheduleTime = DateTime.Now.AddMinutes(240);
            // Tạo 1 timer từ libary System.Timers
            TimeSpan timeSpan = _scheduleTime.Subtract(DateTime.Now);
            _timer = new Timer();
            // Execute mỗi ngày vào lúc 8h sáng
            _timer.Interval = timeSpan.TotalMilliseconds;
            //_timer.Interval = 10000;
            // Những gì xảy ra khi timer đó dc tick
            _timer.Elapsed += Timer_Elapsed;
            // Enable timer
            _timer.Enabled = true;
        }

        protected override void OnStart(string[] args)
        {
            FileHelper.WriteToFile("UpAutoRateService", "\n--------------------------------------------------------\n");
            FileHelper.WriteToFile("UpAutoRateService", "[UpAutoRateService] [START]:" + DateTime.Now);
            this.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            FileHelper.WriteToFile("UpAutoRateService", "[UpAutoRateService] [CALL_API]:" + DateTime.Now);
            //string api = "http://localhost:44366/api/v1/en-US/OpsTransaction/AutoRateReplicate";
            //string api = "http://test.api-efms.itlvn.com/Documentation/api/v1/en-US/OpsTransaction/AutoRateReplicate";
            //string api = "https://uat-api-efms.itlvn.com/Documentation/api/v1/en-US/OpsTransaction/AutoRateReplicate";
            //string api = "https://api-efms.itlvn.com/Documentation/api/v1/vi/OpsTransaction/AutoRateReplicate";
            string api = ConfigurationManager.AppSettings["Api_AutoRateReplicateService"];

            string myIP = Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString();
            FileHelper.WriteToFile("UpAutoRateService", string.Format("[UpAutoRateService][HOST_NAME:{0}][IP:{1}][CALL_API]:{2}", Dns.GetHostName(), myIP, api));
            var pool = new WebClient();
            pool.DownloadStringCompleted += (q, s) =>
            {
                FileHelper.WriteToFile("UpAutoRateService", string.Format("[UpAutoRateService][HOST_NAME:{0}][IP:{1}][CALL_API]:{2}", Dns.GetHostName(), myIP, s.Result));

                if (_timer.Interval != 24 * 60 * 60 * 1000)
                {
                    _timer.Interval = 24 * 60 * 60 * 1000;
                }
                this.Start();

            };

            pool.DownloadStringAsync(new Uri(api));
        }

        public new void Stop()
        {
            FileHelper.WriteToFile("UpAutoRateService", "[UpAutoRateService] [STOP]:" + DateTime.Now);
            _timer.Stop();
            _timer.Dispose();
        }

        protected override void OnStop()
        {
            this.Stop();
        }
    }
}
