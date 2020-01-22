using eFMSWindowService.Models;
using System;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Timers;
using System.Configuration;

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
            WriteToFile("Service is started at " + DateTime.Now);

            var _interval = int.Parse(ConfigurationManager.AppSettings["intervalCurrentStatus"].ToString());
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
                WriteToFile(DateTime.Now + " - Total number of affected rows: " + result);
            }                
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Service update job status is stopped at " + DateTime.Now);
            this.Start();
        }

        public new void Stop()
        {
            WriteToFile("Service update job status is stopped at " + DateTime.Now);
            timer.Stop();
            timer.Dispose();
        }

        protected override void OnStop()
        {
            this.Stop();
        }

        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_UpdateCurrentStatusOfJob_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}
