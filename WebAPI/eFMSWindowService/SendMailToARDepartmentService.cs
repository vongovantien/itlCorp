using eFMS.API.Common;
using eFMSWindowService.Helpers;
using eFMSWindowService.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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
            //// For first time, set amount of seconds between current time and schedule time
            //_timer.Enabled = true;
            //_timer.Interval = _scheduleTime.Subtract(DateTime.Now).TotalSeconds * 1000;
            //_timer.Elapsed += Timer_Elapsed;
            var _interval = int.Parse(ConfigurationManager.AppSettings["intervalCurrentStatus"].ToString());
            // Tạo 1 timer từ libary System.Timers
            _timer = new Timer();
            // Execute mỗi 2 minute
            _timer.Interval = _interval;
            // Những gì xảy ra khi timer đó dc tick
            _timer.Elapsed += Timer_Elapsed;
            // Enable timer
            _timer.Enabled = true;
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
                var data = db.sp_GetShipmentInThreeDayToSendARDept();
                var departments = db.catDepartments.Where(x => x.DeptType == "AR" && x.Active == true).ToList();
                if(data != null)
                {
                    string date = DateTime.Today.AddDays(3).ToShortDateString();
                    string subject = "Thông báo danh sách Khách hàng có đơn hàng Local charge cần thu đến ngày <" + date + "></br>";
                    string headerBody = "<strong>Dear AR Team,</strong> </br>Dưới đây là danh sách Khách hàng có đơn hàng Local charge cần thu đến ngày " + date;
                    string footerBody = "Anh/chị vui lòng liên hệ Khách hàng đề nghị thanh toán ngay phí Local charge để tránh ảnh hưởng đến việc nhận hàng theo quy định Công ty.</br></br>Many thanks and Best Regards";
                    
                    foreach (var item in departments)
                    {
                        if (!string.IsNullOrEmpty(item.Email))
                        {
                            int i = 0;
                            var shipments = data.Where(x => x.OfficeID == item.BranchID);
                            string tableBody = "<table><tr><th> STT </th><th> C.Nhánh </th><th> Số Job </th ><th> Số AWB / HAWB </th ><th> ETD date </th ><th> ETA date </th ><th> Số tiền </th ><th> Trạng thái </th >[content]</table>";
                            StringBuilder content = new StringBuilder();
                            foreach(var shipment in shipments)
                            {
                                i = i + 1;
                                content.Append("<tr><td>" + i + "</td>");
                                content.Append("<td>" + shipment.OfficeName + "</td>");
                                content.Append("<td>" + shipment.PartnerName + "</td>");
                                content.Append("<td>" + shipment.MAWB + " / " + shipment.HWBNo + "</td>");
                                content.Append("<td>" + shipment.ETD + "</td>");
                                content.Append("<td>" + shipment.ETA + "</td>");
                                content.Append("<td>0</td>");
                                content.Append("<td>" + shipment.FreightPayment + "</td></tr>");
                            }
                            tableBody = tableBody.Replace("[content]", content.ToString());
                            string toEmails = item.Email;
                            string body = headerBody + tableBody + footerBody;
                            var jobs = data.Where(x => x.OfficeID == item.BranchID);
                            SendMail.Send(subject, body, toEmails, null, null);
                        }
                    }
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
            FileHelper.WriteToFile("SendMailToARDepartment", "Service send mail is stopped a " + DateTime.Now);
            _timer.Stop();
            _timer.Dispose();
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
