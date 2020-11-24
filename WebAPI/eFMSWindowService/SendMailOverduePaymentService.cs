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
            _scheduleTime = DateTime.Today.AddDays(1).AddHours(8);
        }

        public void Start()
        {
            // Tạo 1 timer từ libary System.Timers
            _timer = new Timer();
            // Execute mỗi ngày vào lúc 8h sáng
            //_timer.Interval = _scheduleTime.Subtract(DateTime.Now).TotalSeconds * 1000;
            _timer.Interval = 30000;
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
                FileHelper.WriteToFile("SendMailOverduePayment", "Service send mail overdue payment is recall at " + DateTime.Now);
                using (eFMSTestEntities db = new eFMSTestEntities())
                {
                    string subject = "Thông báo Khách hàng CÔNG NỢ QUÁ HẠN";
                    string headerBody = @"<strong>Dear All,</strong> </br></br>"
                                      + @"<p>Dưới đây là danh sách Khách hàng có <strong style='color: red;'>công nợ quá hạn</strong> do anh/chị phụ trách:</p>"
                                      + @"<p><i>Below is the list of your customers which has been <strong>overdue payment:</strong></i></p>";

                    string footerBody = @"</br></br>"
                                      + @"<p>Anh/chị vui lòng liên hệ khách hàng đề nghị thanh toán ngay phần công nợ <strong>quá hạn</strong> để tránh ảnh hưởng đến việc nhận booking mới từ khách hàng theo quy định Công ty.</p>"
                                      + @"<p><i>Please contact your customer to request settle <strong>their overdue payment</strong> immediately in order to avoid affecting their new booking according to the Company policy.</i></p>"
                                      + @"</br></br>Many thanks and Best Regards,";

                    string tableBody = @"<table style='width: 100%; border: 1px solid #dddddd; border-collapse: collapse;'>"
                                     + @"<tr>"
                                     + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Chi nhánh <br/> <i style='font-weight: normal'>Branch</br></th>"
                                     + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Khách hàng <br/> <i style='font-weight: normal'>Customer</br></th>"
                                     + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Non - overdue</th>"
                                     + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Overdue <br/> 01-15 days</th>"
                                     + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Overdue <br/> 15-30 days</th>"
                                     + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Overdue <br/> more 30 days</th>"
                                     + @"</tr>"
                                     + @"[content]"
                                     + @"</table>";

                    StringBuilder content = new StringBuilder();

                    var overduePayments = new List<OverduePayment>
                    {
                        new OverduePayment{ Office = "ITL HCM CORP", Customer = "HO GUOM", NonOverdue = 139484900, Over1To15Day = (decimal)48373700.634, Over30Day = 3948000 },
                        new OverduePayment{ Office = "ITL DAD CORP", Customer = "TAN THIEN DIA", Over1To15Day = (decimal)483737.634, Over15To30Day = (decimal)5847321.5968 },
                        new OverduePayment{ Office = "ITL HAN CORP", Customer = "VINATEX", NonOverdue = 1394849, Over1To15Day = (decimal)1245737.644, Over30Day = 1230000 },
                        new OverduePayment{ Office = "ITL HAN CORP", Customer = "HO GUOM", NonOverdue = 132294849, Over30Day = 3948 },
                        new OverduePayment{ Office = "ITL HAN CORP", Customer = "TRI VIET", NonOverdue = 11233008, Over1To15Day = (decimal)483737.634, Over15To30Day = 4857699, Over30Day = 16094800 },
                        new OverduePayment{ Office = "ITL DAD CORP", Customer = "VIETJET AIR", NonOverdue = 4322887, Over1To15Day = (decimal)1485737.34, Over30Day = (decimal)437789000.5848 },
                        new OverduePayment{ Office = "ITL HCM CORP", Customer = "DELTA CARGO", NonOverdue = 16664394849, Over1To15Day = 542888997},
                        new OverduePayment{ Office = "ITL HCM CORP", Customer = "TRANSVIET", NonOverdue = 1394849, Over1To15Day = 23500000, Over30Day = 399948 },
                        new OverduePayment{ Office = "ITL DAD CORP", Customer = "HO GUOM",  Over1To15Day = 5432556, Over30Day = 773948 },
                        new OverduePayment{ Office = "ITL DAD CORP", Customer = "TH-Sky Pacific", NonOverdue = 13934849, Over1To15Day = (decimal)483737.634, Over30Day = 883948 },
                        new OverduePayment{ Office = "ITL DAD CORP", Customer = "TH-Sky Pacific", Over1To15Day = (decimal)483737.634, Over30Day = 883948 },
                    };

                    foreach (var overduePayment in overduePayments)
                    {
                        content.Append(@"<tr>");
                        content.Append(@"<td style='width: 20%; border: 1px solid #dddddd; border-collapse: collapse;'>&nbsp;&nbsp;" + overduePayment.Office + "</td>");
                        content.Append(@"<td style='width: 20%; border: 1px solid #dddddd; border-collapse: collapse;'>&nbsp;&nbsp;" + overduePayment.Customer + "</td>");
                        content.Append(@"<td style='width: 15%; border: 1px solid #dddddd; border-collapse: collapse; text-align: right;'>" + string.Format("{0:n0}", overduePayment.NonOverdue) + "</td>");
                        content.Append(@"<td style='width: 15%; border: 1px solid #dddddd; border-collapse: collapse; text-align: right;'>" + string.Format("{0:n0}", overduePayment.Over1To15Day) + "</td>");
                        content.Append(@"<td style='width: 15%; border: 1px solid #dddddd; border-collapse: collapse; text-align: right;'>" + string.Format("{0:n0}", overduePayment.Over15To30Day) + "</td>");
                        content.Append(@"<td style='width: 15%; border: 1px solid #dddddd; border-collapse: collapse; text-align: right;'>" + string.Format("{0:n0}", overduePayment.Over30Day) + "</td>");
                        content.Append(@"</tr>");
                    }
                    tableBody = tableBody.Replace("[content]", content.ToString());
                    string body = headerBody + tableBody + footerBody;
                    List<string> mail = new List<string> { "andy.hoa@itlvn.com" };
                    if (overduePayments != null && overduePayments.Count > 0 && mail != null && mail.Count > 0)
                    {
                        var s = SendMailHelper.Send(subject, body, mail);

                        #region --- Ghi Log Send Mail ---
                        var logSendMail = new sysSentEmailHistory
                        {
                            SentUser = SendMailHelper._emailFrom,
                            Receivers = string.Join("; ", mail),
                            Subject = subject,
                            Sent = s,
                            SentDateTime = DateTime.Now,
                            Body = body
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
            }
        }

        public new void Stop()
        {
            FileHelper.WriteToFile("SendMailOverduePayment", "Service send mail overdue payment is stopped at " + DateTime.Now);
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
