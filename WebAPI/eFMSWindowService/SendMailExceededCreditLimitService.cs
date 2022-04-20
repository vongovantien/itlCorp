using eFMSWindowService.Helpers;
using eFMSWindowService.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            _scheduleTime = DateTime.Today.AddDays(1).AddHours(3);
        }

        public void Start()
        {
            FileHelper.WriteToFile("SendMailExceededCreditLimit", "[SendMailExceededCreditLimitService] [START]:" + DateTime.Now);
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
            if (ConfigurationManager.AppSettings["Start_SendMailExceededCreditLimitService"] == "1")
                this.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                FileHelper.WriteToFile("SendMailExceededCreditLimit", "[RECALL] at " + DateTime.Now);
                using (eFMSTestEntities db = new eFMSTestEntities())
                {
                    var dt = db.Database.SqlQuery<sp_GetOverDuePaymentCredit_Result>("[dbo].[sp_GetOverDuePaymentCredit]").ToList();
                    var dtGrp = dt.GroupBy(x => new
                    {
                        x.UserName,
                        x.Email
                    });

                    foreach (var item in dtGrp)
                    {
                        string subject = item.Key.UserName != null ? $"Thông báo Khách hàng VƯỢT HẠN MỨC CÔNG NỢ của sale {item.Key.UserName}" : "Thông báo Khách hàng VƯỢT HẠN MỨC CÔNG NỢ";
                        string dear = item.Key.UserName != null ? $"<strong>Dear {item.Key.UserName},</strong> </br></br>" : "<strong>Dear All,</strong> </br></br>";

                        string headerBody = @"<p>Dưới đây là danh sách Khách hàng <strong style='color: red;'>vượt hạn mức công nợ</strong> do anh/chị phụ trách:</p>"
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
                                         + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Mã số thuế <br/> <i style='font-weight: normal'>Tax code</i></th>"
                                         + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Khách hàng <br/> <i style='font-weight: normal'>Customer</i></th>"
                                         + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Hạn mức được cấp <br/> <i style='font-weight: normal'>Credit Limit</i></th>"
                                         + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>% Hạn mức công nợ hiện tại <br/> <i style='font-weight: normal'>% Current Credit</i></th>"
                                         + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Số công nợ hiện tại <br/> <i style='font-weight: normal'>Current Debts</i></th>"
                                         + @"</tr>"
                                         + @"[content]"
                                         + @"</table>";

                        var exceededCreditLimits = new List<ExceededCreditLimit>();
                        foreach (var it in item)
                        {
                            var o = new ExceededCreditLimit()
                            {
                                TaxCode = it.TaxCode,
                                Office = it.BranchName_EN,
                                Customer = it.PartnerName_EN,
                                CreditLimit = it.CreditLimit != null?it.CreditLimit:0,
                                CurrentCredit = it.CreditRate,
                                CurrentDebts = it.DebitAmount
                            };

                            exceededCreditLimits.Add(o);
                        }

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
                        string body = dear + headerBody + tableBody + footerBody;
                        body = string.Format("<div style='font-family: Calibri; font-size: 12pt; color: #004080'>{0}</div>", body);
                        List<string> mail = new List<string> { item.Key.Email };
                        var configBCC = ConfigurationManager.AppSettings["SendMailBCC"];
                        List<string> emailBCCs = configBCC.Split(',').ToList<string>();
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
                }

                if (_timer.Interval != 24 * 60 * 60 * 1000)
                {
                    _timer.Interval = 24 * 60 * 60 * 1000;
                }
            }
            catch (Exception ex)
            {
                throw ex;
                FileHelper.WriteToFile("SendMailExceededCreditLimit", "[ERROR][Timer_Elapsed]:" + ex.Message);
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
                    content.Append(@"<td style='width: 20%; border: 1px solid #dddddd; border-collapse: collapse;text-align: center;'>" + item.TaxCode + "</td>");
                    content.Append(@"<td style='width: 20%; border: 1px solid #dddddd; border-collapse: collapse;'>&nbsp;&nbsp;" + item.Customer + "</td>");
                    content.Append(@"<td style='width: 18%; border: 1px solid #dddddd; border-collapse: collapse; text-align: right;'>" + string.Format("{0:#,##0.00}", item.CreditLimit) + "</td>");
                    content.Append(@"<td style='width: 18%; border: 1px solid #dddddd; border-collapse: collapse; text-align: center;'>" + string.Format("{0:#,##0.00}", item.CurrentCredit) + " %</td>");
                    content.Append(@"<td style='width: 18%; border: 1px solid #dddddd; border-collapse: collapse; text-align: right;'>" + string.Format("{0:#,##0.00}", item.CurrentDebts) + "</td>");
                    content.Append(@"</tr>");
                    no = no + 1;
                }
            }
            _no = no;
            return content;
        }

        public new void Stop()
        {
            FileHelper.WriteToFile("SendMailExceededCreditLimit", "[SendMailExceededCreditLimitService] [STOP]:" + DateTime.Now);
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
        public string TaxCode { get; set; }
        public string Office { get; set; }
        public string Customer { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? CurrentCredit { get; set; }
        public decimal? CurrentDebts { get; set; }
    }
}
