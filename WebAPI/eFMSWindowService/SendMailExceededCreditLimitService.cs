using eFMSWindowService.Helpers;
using eFMSWindowService.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace eFMSWindowService
{
    partial class SendMailExceededCreditLimitService : ServiceBase
    {
        Timer _timer;
        DateTime _scheduleTime;
        /// <summary>
        /// Send mail Vượt Hạn Mức Công Nợ
        /// </summary>
        public SendMailExceededCreditLimitService()
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
                FileHelper.WriteToFile("SendMailExceededCreditLimit", "Service send mail exceeded credit limit is recall at " + DateTime.Now);
                using (eFMSTestEntities db = new eFMSTestEntities())
                {
                    string subject = "Thông báo Khách hàng VƯỢT HẠN MỨC CÔNG NỢ";
                    string headerBody = @"<strong>Dear All,</strong> </br></br>"
                                      + @"<p>Dưới đây là danh sách Khách hàng <strong style='color: red;'>vượt hạn mức công nợ</strong> do anh/chị phụ trách:</p>"
                                      + @"<p><i>Below is the list of your customers which has been <strong>exceeded credit limit:</strong></i></p>";

                    string footerBody = @"</br></br>"
                                      + @"<p>* Nếu hạn mức tín dụng vượt quá 120% thì phần mềm sẽ tự động khóa booking</p>"
                                      + @"<p><i>* If the credit limit has been exceeded 120%, the system will be block booking automatically</i></p>"
                                      + @"<p>Anh/chị vui lòng liên hệ Khách hàng đề nghị thanh toán ngay phần công nợ <strong>sắp/ đã vượt hạn mức</strong> để tránh ảnh hưởng đến việc nhận booking mới từ khách hàng theo quy định Công ty.</p>"
                                      + @"<p><i>Please contact your customer to request settle the outstanding which has been <strong>exceeded credit limit</strong> immediately in order to avoid affecting their new booking according to the Company policy.</i></p>"
                                      + @"</br></br>Many thanks and Best Regards,";

                    string tableBody = @"<table style='width: 100%; border: 1px solid #dddddd; border-collapse: collapse;'>"
                                     + @"<tr>"
                                     + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>STT <br/> <i style='font-weight: normal'>No.</i></th>"
                                     + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Chi nhánh <br/> <i style='font-weight: normal'>Branch</i></th>"
                                     + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Khách hàng <br/> <i style='font-weight: normal'>Customer</i></th>"
                                     + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Hạn mức được cấp <br/> <i style='font-weight: normal'>Credit Limit</i></th>"
                                     + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>% Hạn mức công nợ hiện tại <br/> <i style='font-weight: normal'>% Current Credit</i></th>"
                                     + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Số công nợ hiện tại <br/> <i style='font-weight: normal'>Current Debts</i></th>"
                                     + @"</tr>"
                                     + @"[content]"
                                     + @"</table>";

                    var exceededCreditLimits = new List<ExceededCreditLimit>
                    {
                        new ExceededCreditLimit{ Office = "ITL HCM CORP", Customer = "HO GUOM", CreditLimit = 10000, CurrentCredit = 150, CurrentDebts = 19056000 },
                        new ExceededCreditLimit{ Office = "ITL DAD CORP", Customer = "TAN THIEN DIA", CreditLimit = 20000, CurrentCredit = 570, CurrentDebts = 1190000 },
                        new ExceededCreditLimit{ Office = "ITL HAN CORP", Customer = "VINATEX", CreditLimit = 30000, CurrentCredit = 90, CurrentDebts = 19005400 },
                        new ExceededCreditLimit{ Office = "ITL HAN CORP", Customer = "HO GUOM", CreditLimit = 40000, CurrentCredit = 70, CurrentDebts = 1912000 },
                        new ExceededCreditLimit{ Office = "ITL HAN CORP", Customer = "TRI VIET", CreditLimit = 50000, CurrentCredit = 80, CurrentDebts = 1096000 },
                        new ExceededCreditLimit{ Office = "ITL DAD CORP", Customer = "VIETJET AIR", CreditLimit = 60000, CurrentCredit = 90, CurrentDebts = 189000 },
                        new ExceededCreditLimit{ Office = "ITL HCM CORP", Customer = "DELTA CARGO", CreditLimit = 70000, CurrentCredit = 100, CurrentDebts = 790000 },
                        new ExceededCreditLimit{ Office = "ITL HCM CORP", Customer = "TRANSVIET", CreditLimit = 80000, CurrentCredit = 120, CurrentDebts = 196000 },
                        new ExceededCreditLimit{ Office = "ITL DAD CORP", Customer = "HO GUOM", CreditLimit = 90000, CurrentCredit = 130, CurrentDebts = 19000670 },
                        new ExceededCreditLimit{ Office = "ITL DAD CORP", Customer = "TH-Sky Pacific", CreditLimit = 100000, CurrentCredit = 160, CurrentDebts = 13190000 },
                        new ExceededCreditLimit{ Office = "ITL DAD CORP", Customer = "TH-Sky Pacific", CreditLimit = 110000, CurrentCredit = 280, CurrentDebts = 13590000 },
                    };

                    var exceededCreditsMore70 = exceededCreditLimits.Where(x => x.CurrentCredit >= 70 && x.CurrentCredit < 100).OrderBy(o => o.CurrentCredit).ToList();
                    var exceededCreditsMore100 = exceededCreditLimits.Where(x => x.CurrentCredit >= 100 && x.CurrentCredit < 120).OrderBy(o => o.CurrentCredit).ToList();
                    var exceededCreditsMore120 = exceededCreditLimits.Where(x => x.CurrentCredit >= 120).ToList().OrderBy(o => o.CurrentCredit).ToList();

                    StringBuilder content = new StringBuilder();
                    var no = 1;

                    if (exceededCreditsMore70.Count > 0)
                    {
                        content.Append(GetContentTableGroupCreditLimit(exceededCreditsMore70, 70, no, out no));
                    }

                    if (exceededCreditsMore100.Count > 0)
                    {
                        content.Append(GetContentTableGroupCreditLimit(exceededCreditsMore100, 100, no, out no));
                    }

                    if (exceededCreditsMore120.Count > 0)
                    {
                        content.Append(GetContentTableGroupCreditLimit(exceededCreditsMore120, 120, no, out no));
                    }

                    content.Append(@"<tr>");
                    content.Append(@"<tr colspan='6'>&nbsp;</tr>");
                    content.Append(@"</tr>");

                    tableBody = tableBody.Replace("[content]", content.ToString());
                    string body = headerBody + tableBody + footerBody;
                    body = string.Format("<div style='font-family: Calibri; font-size: 12pt; color: #004080'>{0}</div>", body);
                    List<string> mail = new List<string> { "andy.hoa@itlvn.com" };
                    List<string> emailBCCs = CommonData.EmailBCCs;
                    if (exceededCreditLimits != null && exceededCreditLimits.Count > 0
                        && (exceededCreditsMore70.Count > 0 || exceededCreditsMore100.Count > 0 || exceededCreditsMore120.Count > 0)
                        && mail != null && mail.Count > 0)
                    {
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
            }
        }

        private StringBuilder GetContentTableGroupCreditLimit(List<ExceededCreditLimit> exceededCreditLimits, int rangeCurrentCredit, int no, out int _no)
        {
            StringBuilder content = new StringBuilder();
            if (exceededCreditLimits.Count > 0)
            {
                content.Append(@"<tr>");
                content.Append(@"<tr colspan='6'>&nbsp;</tr>");
                content.Append(@"</tr>");
                content.Append(@"<tr style='background-color: #dddddd;'>");
                content.Append(@"<td colspan='6'>&nbsp;&nbsp;<b>Hạn mức >= " + rangeCurrentCredit + "% /</b> <i>Credit limit >= " + rangeCurrentCredit + "%</i></td>");
                content.Append(@"</tr>");
                foreach (var item in exceededCreditLimits)
                {
                    content.Append(@"<tr>");
                    content.Append(@"<td style='width: 6%; border: 1px solid #dddddd; border-collapse: collapse; text-align: center;'>" + no + "</td>");
                    content.Append(@"<td style='width: 20%; border: 1px solid #dddddd; border-collapse: collapse;'>&nbsp;&nbsp;" + item.Office + "</td>");
                    content.Append(@"<td style='width: 20%; border: 1px solid #dddddd; border-collapse: collapse;'>&nbsp;&nbsp;" + item.Customer + "</td>");
                    content.Append(@"<td style='width: 18%; border: 1px solid #dddddd; border-collapse: collapse; text-align: right;'>" + string.Format("{0:n0}", item.CreditLimit) + "</td>");
                    content.Append(@"<td style='width: 18%; border: 1px solid #dddddd; border-collapse: collapse; text-align: center;'>" + string.Format("{0:n0}", item.CurrentCredit) + " %</td>");
                    content.Append(@"<td style='width: 18%; border: 1px solid #dddddd; border-collapse: collapse; text-align: right;'>" + string.Format("{0:n0}", item.CurrentDebts) + "</td>");
                    content.Append(@"</tr>");
                    no = no + 1;
                }
            }
            _no = no;
            return content;
        }

        public new void Stop()
        {
            FileHelper.WriteToFile("SendMailExceededCreditLimit", "Service send mail exceeded credit limit is stopped at " + DateTime.Now);
            _timer.Stop();
            _timer.Dispose();
        }

        protected override void OnStop()
        {
            this.Stop();
        }
    }

    class ExceededCreditLimit
    {
        public string Office { get; set; }
        public string Customer { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? CurrentCredit { get; set; }
        public decimal? CurrentDebts { get; set; }
    }
}
