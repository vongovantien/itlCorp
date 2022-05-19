using eFMSWindowService.Helpers;
using eFMSWindowService.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace eFMSWindowService
{
    partial class SendMailOverduePaymentService : ServiceBase
    {
        Timer _timer;
        DateTime _scheduleTime;
        /// <summary>
        /// Send mail Công Nợ Quá Hạn
        /// </summary>
        public SendMailOverduePaymentService()
        {
            InitializeComponent();
            _scheduleTime = DateTime.Today.AddDays(1).AddHours(3);
        }

        public void Start()
        {
            FileHelper.WriteToFile("SendMailOverduePaymentService", "[SendMailOverduePaymentServiceService] [START]:" + DateTime.Now);
            // Tạo 1 timer từ libary System.Timers
            _timer = new Timer();
            // Execute mỗi ngày vào lúc 8h sáng
            _timer.Interval = _scheduleTime.Subtract(DateTime.Now).TotalSeconds * 1000;
            //_timer.Interval = 10000;
            // Những gì xảy ra khi timer đó dc tick
            _timer.Elapsed += Timer_Elapsed;
            // Enable timer
            _timer.Enabled = true;
        }

        protected override void OnStart(string[] args)
        {
            if (ConfigurationManager.AppSettings["Start_SendMailOverduePaymentService"] == "1")
                this.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                FileHelper.WriteToFile("SendMailOverduePaymentService", "[RECALL] at " + DateTime.Now);
                using (eFMSTestEntities db = new eFMSTestEntities())
                {
                    var dt = db.Database.SqlQuery<sp_GetOverDuePayment_Result>("[dbo].[sp_GetOverDuePayment]").ToList();
                    var dtGrp = dt.GroupBy(x => new
                    {
                        x.UserName,
                        x.Email
                    });

                    foreach (var item in dtGrp)
                    {
                        string subject = item.Key.UserName != null ? $"Thông báo Khách hàng CÔNG NỢ QUÁ HẠN của sale {item.Key.UserName}" : "Thông báo Khách hàng CÔNG NỢ QUÁ HẠN ";
                        string dear = item.Key.UserName != null ? $"<strong>Dear {item.Key.UserName},</strong> </br></br>" : "<strong>Dear All,</strong> </br></br>";

                        string headerBody = @"<p>Dưới đây là danh sách Khách hàng có <strong style='color: red;'>công nợ quá hạn</strong> do anh/chị phụ trách:</p>"
                                          + @"<p><i>Below is the list of your customers which has been <strong>overdue payment:</strong></i></p>";

                        string footerBody = @"</br></br>"
                                          + @"<p>Anh/chị vui lòng liên hệ khách hàng đề nghị thanh toán ngay phần công nợ <strong>quá hạn</strong> để tránh ảnh hưởng đến việc nhận booking mới từ khách hàng theo quy định Công ty.</p>"
                                          + @"<p><i>Please contact your customer to request settle <strong>their overdue payment</strong> immediately in order to avoid affecting their new booking according to the Company policy.</i></p>"
                                          + @"</br></br>Many thanks and Best Regards,";

                        string tableBody = @"<table style='width: 100%; border: 1px solid #dddddd; border-collapse: collapse;'>"
                                         + @"<tr>"
                                         + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Mã số thuế <br/> <i style='font-weight: normal'>Tax Code</br></th>"
                                         + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Khách hàng <br/> <i style='font-weight: normal'>Customer</br></th>"
                                         + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Non - overdue</th>"
                                         + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Overdue <br/> 01-15 days</th>"
                                         + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Overdue <br/> 15-30 days</th>"
                                         + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Overdue <br/> more 30 days</th>"
                                         + @"</tr>"
                                         + @"[content]"
                                         + @"</table>";

                        StringBuilder content = new StringBuilder();
                        foreach (var overduePayment in item)
                        {
                            content.Append(@"<tr>");
                            content.Append(@"<td style='width: 20%; border: 1px solid #dddddd; border-collapse: collapse;text-align: left';>" + overduePayment.TaxCode + "</td>");
                            content.Append(@"<td style='width: 20%; border: 1px solid #dddddd; border-collapse: collapse;text-align: left';>" + overduePayment.PartnerName_EN + "</td>");
                            content.Append(@"<td style='width: 15%; border: 1px solid #dddddd; border-collapse: collapse; text-align: right;'>" + (overduePayment.NonOverdue > 0 ? string.Format("{0:#,##0.00}", overduePayment.NonOverdue) : string.Empty) + "</td>");
                            content.Append(@"<td style='width: 15%; border: 1px solid #dddddd; border-collapse: collapse; text-align: right;'>" + string.Format("{0:#,##0.00}", overduePayment.Over1To15Day) + "</td>");
                            content.Append(@"<td style='width: 15%; border: 1px solid #dddddd; border-collapse: collapse; text-align: right;'>" + string.Format("{0:#,##0.00}", overduePayment.Over16To30Day) + "</td>");
                            content.Append(@"<td style='width: 15%; border: 1px solid #dddddd; border-collapse: collapse; text-align: right;'>" + string.Format("{0:#,##0.00}", overduePayment.Over30Day) + "</td>");
                            content.Append(@"</tr>");
                        }
                        tableBody = tableBody.Replace("[content]", content.ToString());
                        string body = dear + headerBody + tableBody + footerBody;
                        body = string.Format("<div style='font-family: Calibri; font-size: 12pt; color: #004080'>{0}</div>", body);
                        List<string> mail = new List<string> { item.Key.Email };
                        var configBCC = ConfigurationManager.AppSettings["SendMailBCC"];
                        List<string> emailBCCs = configBCC.Split(',').ToList<string>();
                        var s = SendMailHelper.Send(subject, body, mail, null, null, emailBCCs);

                        #region --- Ghi Log Send Mail ---
                        var logSendMail = new sysSentEmailHistory
                        {
                            SentUser = SendMailHelper._emailFrom,
                            Receivers = string.Join("; ", mail),
                            Subject = subject,
                            Sent = s,
                            SentDateTime = DateTime.Now,
                            Body = body,
                            BCCs = string.Join("; ", emailBCCs)
                        };
                        var hsLogSendMail = db.sysSentEmailHistories.Add(logSendMail);
                        var hsSc = db.SaveChanges();
                        #endregion --- Ghi Log Send Mail ---
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
                FileHelper.WriteToFile("SendMailOverduePaymentService", "[ERROR][Timer_Elapsed]:" + ex.Message);
            }
        }

        public new void Stop()
        {
            FileHelper.WriteToFile("SendMailOverduePaymentService", "[SendMailOverduePaymentServiceService] [STOP]:" + DateTime.Now);
            _timer.Stop();
            _timer.Dispose();
        }

        protected override void OnStop()
        {
            this.Stop();
        }
    }

    class OverduePayment
    {
        public string Office { get; set; }
        public string Customer { get; set; }
        public decimal? NonOverdue { get; set; }
        public decimal? Over1To15Day { get; set; }
        public decimal? Over15To30Day { get; set; }
        public decimal? Over30Day { get; set; }
    }
}
