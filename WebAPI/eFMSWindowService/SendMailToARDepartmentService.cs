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
            _scheduleTime = DateTime.Today.AddDays(1).AddHours(8);
        }

        public void Start()
        {
            // Tạo 1 timer từ libary System.Timers
            _timer = new Timer();
            // Execute mỗi ngày vào lúc 8h sáng
            _timer.Interval = _scheduleTime.Subtract(DateTime.Now).TotalSeconds * 1000;
            // _timer.Interval = 30000;
            // Những gì xảy ra khi timer đó dc tick
            _timer.Elapsed += Timer_Elapsed;
            // Enable timer
            _timer.Enabled = true;
        }
        protected override void OnStart(string[] args)
        {
            this.Start();
        }

        /// <summary>
        /// Export: SEL, SEF, AE ------- Import: SIF, SIL, AI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {

                FileHelper.WriteToFile("SendMailToARDepartment", "Service send mail to AR department is recall at " + DateTime.Now);
                using (eFMSTestEntities db = new eFMSTestEntities())
                {
                    var data = db.Database.SqlQuery<sp_GetShipmentInThreeDayToSendARDept_Result>("[dbo].[sp_GetShipmentInThreeDayToSendARDept]").ToList();
                    var departments = db.catDepartments.Where(x => x.Active == true && x.DeptType =="AR").ToList();
                    if (data.Count >0)
                    {
                        string date = DateTime.Today.AddDays(3).ToShortDateString();
                        string subject = "Thông báo danh sách Khách hàng có đơn hàng Local charge cần thu đến ngày <" + date + ">";
                        string headerBody = @"<strong>Dear AR Team,</strong> </br>Dưới đây là danh sách Khách hàng có đơn hàng Local charge cần thu đến ngày " + date;
                        string footerBody = "</br></br>Anh/chị vui lòng liên hệ Khách hàng đề nghị thanh toán ngay phí Local charge để tránh ảnh hưởng đến việc nhận hàng theo quy định Công ty.</br></br>Many thanks and Best Regards";

                        foreach (var item in departments)
                        {
                            if (!string.IsNullOrEmpty(item.Email))
                            {
                                int i = 0;
                                var shipments = data.Where(x => x.OfficeID == item.BranchID);
                                string tableBody = @"<table><tr>"
                                                    + @"<th style=""border: 1px solid black;border-collapse: collapse;""> STT </th>"
                                                    + @"<th style=""border: 1px solid black;border-collapse: collapse;""> C.Nhánh </th>"
                                                    + @"<th style=""border: 1px solid black;border-collapse: collapse;""> Số Job </th >"
                                                    + @"<th style=""border: 1px solid black;border-collapse: collapse;""> Số AWB / HAWB </th >"
                                                    + @"<th style=""border: 1px solid black;border-collapse: collapse;""> ETD date </th >"
                                                    + @"<th style=""border: 1px solid black;border-collapse: collapse;""> ETA date </th >"
                                                    + @"<th style=""border: 1px solid black;border-collapse: collapse;""> Số tiền </th >"
                                                    + @"<th style=""border: 1px solid black;border-collapse: collapse;""> Trạng thái [Prepaid / Collect] </th >[content]</table>";
                                StringBuilder content = new StringBuilder();
                                foreach (var shipment in shipments)
                                {
                                    i = i + 1;
                                    content.Append(@"<tr><td style=""border: 1px solid black;border-collapse: collapse;"">" + i + "</td>");
                                    content.Append(@"<td style=""border: 1px solid black;border-collapse: collapse;"">" + shipment.OfficeName + "</td>");
                                    content.Append(@"<td style=""border: 1px solid black;border-collapse: collapse;"">" + shipment.JobNo + "</td>");
                                    content.Append(@"<td style=""border: 1px solid black;border-collapse: collapse;"">" + shipment.MAWB + " / " + shipment.HWBNo + "</td>");
                                    content.Append(@"<td style=""border: 1px solid black;border-collapse: collapse;"">" + shipment.ETD?.ToShortDateString() + "</td>");
                                    content.Append(@"<td style=""border: 1px solid black;border-collapse: collapse;"">" + shipment.ETA?.ToShortDateString() + "</td>");
                                    content.Append(@"<td style=""border: 1px solid black;border-collapse: collapse;"">0</td>");
                                    content.Append(@"<td style=""border: 1px solid black;border-collapse: collapse;"">" + shipment.FreightPayment + "</td></tr>");
                                }
                                tableBody = tableBody.Replace("[content]", content.ToString());
                                string body = headerBody + tableBody + footerBody;
                                var jobs = data.Where(x => x.OfficeID == item.BranchID);
                                List<string> toEmails = item.Email.Split(';').Where(x => x != null).ToList();
                                if(toEmails.Count > 0)
                                {
                                    var s = SendMailHelper.Send(subject, body, toEmails);
                            }
                        }
                    }
                }
                }
                if (_timer.Interval != 24 * 60 * 60 * 1000)
                {
                    _timer.Interval = 24 * 60 * 60 * 1000;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public new void Stop()
        {
            FileHelper.WriteToFile("SendMailToARDepartment", "Service send mail to AR department is stopped at " + DateTime.Now);
            _timer.Stop();
            _timer.Dispose();
        }
        protected override void OnStop()
        {
            this.Stop();
        }
    }
}
