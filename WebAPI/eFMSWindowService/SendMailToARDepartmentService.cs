using eFMSWindowService.Helpers;
using eFMSWindowService.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace eFMSWindowService
{
    partial class SendMailToARDepartmentService : ServiceBase
    {
        System.Timers.Timer _timer;
        DateTime _scheduleTime;

        public SendMailToARDepartmentService()
        {
            InitializeComponent();
            _timer = new System.Timers.Timer();
            _scheduleTime = DateTime.Today.AddHours(8); // Schedule to run once a day at 8:00 a.m.
        }

        protected override void OnStart(string[] args)
        {
            // For first time, set amount of seconds between current time and schedule time
            _timer.Enabled = true;
            _timer.Interval = _scheduleTime.Subtract(DateTime.Now).TotalSeconds * 1000;
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
        }

        /// <summary>
        /// Export: SEL, SEF, AE ------- Import: SIF, SIL, AI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            FileHelper.WriteToFile("SendMailToARDepartment", "Service send mail is recall at " + DateTime.Now);
            using (eFMSTestEntities db = new eFMSTestEntities())
            {
                var departments = db.catDepartments.Where(x => x.DeptType == "AR");
                var csTransactionsExport = db.csTransactions.Where(x => ((x.TransactionType == "AE" || x.TransactionType == "SEF" || x.TransactionType == "SEL") 
                                                                        && x.PaymentTerm == "Prepaid")
                                                                        && (DateTime.Now - x.ETD).Value.Days == 3
                                                                    );
                var csTransactionsImport = db.csTransactions.Where(x => ((x.TransactionType == "AI" || x.TransactionType == "SIF" || x.TransactionType == "SIL")
                                                                        && x.PaymentTerm == "Collect")
                                                                        && (DateTime.Now - x.ETD).Value.Days == 3
                                                                    );
                var transactions = csTransactionsExport;
                if(csTransactionsExport == null)
                {
                    transactions = csTransactionsImport;
                }
                else
                {
                    if(csTransactionsImport != null)
                    {
                        transactions = csTransactionsExport.Union(csTransactionsImport);
                    }
                }
                if(transactions != null)
                {
                    //var query = (from job in transactions
                    //             join dept in departments on job.OfficeID equals dept.BranchID);
                }
            }
        }
        public new void Stop()
        {
            FileHelper.WriteToFile("SendMailToARDepartment", "Service send mail is stopped at " + DateTime.Now);
            _timer.Stop();
            _timer.Dispose();
        }
        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }
    }
    public class ShipmentToSendArĐept
    {
        public string OfficeId { get; set; }
        public string CustomerName { get; set; }
        public string JobId { get; set; }
        public string AWB_HAWB { get; set; }
        public DateTime Etd { get; set; }
        public DateTime Eta { get; set; }
        public decimal Total { get; set; }
        public string PaymentTerm { get; set; }
    }
}
