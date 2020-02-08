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
            WriteToFile("Service update status authorization is stopped at " + DateTime.Now);
            _aTimer.Stop();
            _aTimer.Dispose();
        }

        private void _aTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            WriteToFile("Service update status authorization is recall at " + DateTime.Now);
            using (eFMSTestEntities db = new eFMSTestEntities())
            {
                var result = db.Database.SqlQuery<int>("[dbo].[sp_UpdateInactiveStatusAuthorization]").FirstOrDefault();
                WriteToFile(DateTime.Now + " - Total number of affected rows: " + result);
            }
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Service update status authorization is started at " + DateTime.Now);
            this.Start();
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
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_UpdateStatusAuthorization" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
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
