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
    partial class SendMailExpiredAgreementService : ServiceBase
    {
        Timer _timer;
        DateTime _scheduleTime;
        /// <summary>
        /// Send mail Họp đồng hết hạn và hợp đồng sắp hết hạn
        /// </summary>
        public SendMailExpiredAgreementService()
        {
            InitializeComponent();
            _scheduleTime = DateTime.Today.AddDays(1).AddHours(8);
        }

        public void Start()
        {
            FileHelper.WriteToFile("SendMailExpiredAgreement", "[SendMailExpiredAgreementService] [START]:" + DateTime.Now);
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
            this.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                FileHelper.WriteToFile("SendMailExpiredAgreement", "[RECALL] at " + DateTime.Now);
                // Get data and send email
                StartGetAndSendEmail();
                if (_timer.Interval != 24 * 60 * 60 * 1000)
                {
                    _timer.Interval = 24 * 60 * 60 * 1000;
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteToFile("SendMailExpiredAgreement", "[ERROR][Timer_Elapsed]:" + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// Send mail Họp đồng hết hạn và hợp đồng sắp hết hạn
        /// </summary>
        private void StartGetAndSendEmail()
        {
            using (eFMSTestEntities db = new eFMSTestEntities())
            {
                var dtExpiredContract = db.Database.SqlQuery<sp_GetExpiredContract_Result>("[dbo].[sp_GetExpiredContract]").ToList();
                var dtExpireSoon = db.Database.SqlQuery<sp_GetContractExpiredWithin15And30Days_Result>("[dbo].[sp_GetContractExpiredWithin15And30Days]").ToList();
                // Send mail contracts have been xpired
                SendEmailExpiredAgreement(db, dtExpiredContract);
                // Send mail contracts have expire soon
                SendMailExpireContractWithinDays(db, dtExpireSoon);
            }
        }

        /// <summary>
        /// Get template and send mail contract have been expired
        /// </summary>
        private void SendEmailExpiredAgreement(eFMSTestEntities db, List<sp_GetExpiredContract_Result> dt)
        {
            var dtGrp = dt.Where(x => !string.IsNullOrEmpty(x.SaleManId)).GroupBy(x => new
            {
                x.SaleManId
            });

            foreach (var item in dtGrp)
            {
                string subject = "Thông báo Khách hàng ĐÃ HẾT HẠN HỢP ĐỒNG – Contracts have EXPIRED";

                string dear = item.FirstOrDefault().SaleManName != null ? $"<strong>Dear {item.FirstOrDefault().SaleManName},</strong> </br></br>" : "<strong>Dear All,</strong> </br></br>";

                string headerBody = @"<p>Dưới đây là danh sách Khách hàng <strong style='color: red;'>đã hết hạn hợp đồng</strong> do anh/chị phụ trách:</br>"
                                  + @"<i>Below is the list of your customer contracts which have <strong>expired:</strong></i></p>";

                string footerBody = @"</br></br>"
                                  + @"<p>Anh/chị vui lòng liên hệ khách hàng đề nghị <strong>gia hạn hoặc làm mới hợp đồng</strong> để tránh ảnh hưởng đến việc nhận booking mới từ khách hàng theo quy định Công ty.</br>"
                                  + @"<i>Please contact your customer to request <strong>extend or renew the contract</strong> immediately in order to avoid affecting their new booking according to the Company policy.</i></p>"
                                  + @"</br></br><i>Many thanks and Best Regards,</i></br></br>"
                                  + "<b> eFMS System, </b>"
                                  + "</br>"
                                  + "<p><img src = 'https://api-efms.itlvn.com/ReportPreview/Images/logo-eFMS.png' /></p> " + " </div>";


                string tableBody = @"<table style='width: 100%; border: 1px solid #dddddd; border-collapse: collapse;'>"
                                 + @"<tr>"
                                 + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>STT <br/> <i style='font-weight: normal'>No.</i></th>"
                                 + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Khách hàng <br/> <i style='font-weight: normal'>Customer</i></th>"
                                 + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Mã hợp đồng <br/> <i style='font-weight: normal'>Contract no</i></th>"
                                 + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Ngày hiệu lực hợp đồng <br/> <i style='font-weight: normal'>Effect date</i></th>"
                                 + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Ngày hết hạn hợp đồng <br/> <i style='font-weight: normal'>Expired date</i></th>"
                                 + @"</tr>"
                                 + @"[content]"
                                 + @"</table>";

                var contractList = new List<ExpiredContractClass>();
                contractList = item.Where(x => !string.IsNullOrEmpty(x.PartnerName)).Select(x => new ExpiredContractClass
                {
                    PartnerName = x.PartnerName,
                    ContractNo = x.ContractNo,
                    EffectiveDate = x.EffectiveDate,
                    ExpiredDate = x.ExpiredDate
                }).ToList();

                StringBuilder content = new StringBuilder();
                content.Append(GetContentTableGroupOfContract(contractList, string.Empty));

                content.Append(@"<tr>");
                content.Append(@"<tr colspan='5'>&nbsp;</tr>");
                content.Append(@"</tr>");

                tableBody = tableBody.Replace("[content]", content.ToString());
                string body = dear + headerBody + tableBody + footerBody;
                body = string.Format("<div style='font-family: Calibri; font-size: 12pt; color: #004080'>{0}</div>", body);
                var mailTo = new List<string> { item.FirstOrDefault().SaleManEmail };
                var mailCC = new List<string>();
                mailCC.AddRange(string.Join(";", item.Select(x => x.AccountantEmail))?.Split(';'));
                mailCC.AddRange(string.Join(";", item.Select(x => x.AREmail))?.Split(';'));
                mailCC = mailCC.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                var configBCC = ConfigurationManager.AppSettings["SendMailBCC"];
                List<string> emailBCCs = configBCC.Split(',').ToList<string>();
                if (contractList.Count > 0)
                {
                    var s = SendMailHelper.Send(subject, body, mailTo, null, mailCC, emailBCCs);

                    #region --- Ghi Log Send Mail ---
                    var logSendMail = new sysSentEmailHistory
                    {
                        SentUser = SendMailHelper._emailFrom,
                        Receivers = string.Join("; ", mailTo),
                        Subject = subject,
                        Sent = s,
                        SentDateTime = DateTime.Now,
                        Body = body,
                        CCs = string.Join(";", mailCC),
                        BCCs = string.Join("; ", emailBCCs)
                    };
                    var hsLogSendMail = db.sysSentEmailHistories.Add(logSendMail);
                    var hsSc = db.SaveChanges();
                    #endregion --- Ghi Log Send Mail ---
                }
            }
        }

        /// <summary>
        /// Get template and send mail contracts have expire soon
        /// </summary>
        private void SendMailExpireContractWithinDays(eFMSTestEntities db, List<sp_GetContractExpiredWithin15And30Days_Result> dt)
        {
            var dtGrp = dt.Where(x => !string.IsNullOrEmpty(x.SaleManId)).GroupBy(x => new
            {
                x.SaleManId
            });

            foreach (var item in dtGrp)
            {
                string subject = "Thông báo Khách hàng SẮP HẾT HẠN HỢP ĐỒNG -  Contract has been EXPIRED SOON";

                string dear = item.FirstOrDefault().SaleManName != null ? $"<strong>Dear {item.FirstOrDefault().SaleManName},</strong> </br></br>" : "<strong>Dear All,</strong> </br></br>";

                string headerBody = @"<p>Dưới đây là danh sách Khách hàng <strong style='color: red;'>sắp hết hạn hợp đồng</strong> do anh/chị phụ trách:</br>"
                                  + @"<i>Below is the list of your customers which has been <strong>expired soon:</strong></i></p>";

                string footerBody = @"</br></br>"
                                  + @"<p>Anh/chị vui lòng liên hệ khách hàng đề nghị <strong>gia hạn hoặc làm mới hợp đồng</strong> để tránh ảnh hưởng đến việc nhận booking mới từ khách hàng theo quy định Công ty.</br>"
                                  + @"<i>Please contact your customer to request <strong>extend or renew the contract</strong> immediately in order to avoid affecting their new booking according to the Company policy.</i></p>"
                                  + @"</br></br><i>Many thanks and Best Regards,</i></br></br>"
                                  + "<b> eFMS System, </b>"
                                  + "</br>"
                                  + "<p><img src = 'https://api-efms.itlvn.com/ReportPreview/Images/logo-eFMS.png' /></p> " + " </div>";

                string tableBody = @"<table style='width: 100%; border: 1px solid #dddddd; border-collapse: collapse;'>"
                                 + @"<tr>"
                                 + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>STT <br/> <i style='font-weight: normal'>No.</i></th>"
                                 + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Khách hàng <br/> <i style='font-weight: normal'>Customer</i></th>"
                                 + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Mã hợp đồng <br/> <i style='font-weight: normal'>Contract no</i></th>"
                                 + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Ngày hiệu lực hợp đồng <br/> <i style='font-weight: normal'>Effect date</i></th>"
                                 + @"<th style='border: 1px solid #dddddd; border-collapse: collapse;'>Ngày hết hạn hợp đồng <br/> <i style='font-weight: normal'>Expired date</i></th>"
                                 + @"</tr>"
                                 + @"[content]"
                                 + @"</table>";

                var contractList = new List<ExpiredContractClass>();
                contractList = item.Where(x => !string.IsNullOrEmpty(x.PartnerName)).Select(x => new ExpiredContractClass
                {
                    PartnerName = x.PartnerName,
                    ContractNo = x.ContractNo,
                    EffectiveDate = x.EffectiveDate,
                    ExpiredDate = x.ExpiredDate
                }).ToList();

                StringBuilder content = new StringBuilder();
                var expireWithin15d = contractList.Where(x => x.ExpiredIn == 15).ToList();
                var expireWithin30d = contractList.Where(x => x.ExpiredIn == 30).ToList();
                if (expireWithin30d.Count > 0)
                {
                    content.Append(GetContentTableGroupOfContract(expireWithin30d, "NearExpired", 30));
                }
                if (expireWithin15d.Count > 0)
                {
                    content.Append(GetContentTableGroupOfContract(expireWithin15d, "NearExpired", 15));
                }

                content.Append(@"<tr>");
                content.Append(@"<tr colspan='5'>&nbsp;</tr>");
                content.Append(@"</tr>");

                tableBody = tableBody.Replace("[content]", content.ToString());
                string body = dear + headerBody + tableBody + footerBody;
                body = string.Format("<div style='font-family: Calibri; font-size: 12pt; color: #004080'>{0}</div>", body);
                // Mail to
                var mailTo = new List<string> { item.FirstOrDefault().SaleManEmail };
                // Mails cc
                var mailCC = new List<string>();
                mailCC.AddRange(string.Join(";", item.Select(x => x.AccountantEmail))?.Split(';'));
                mailCC.AddRange(string.Join(";", item.Select(x => x.AREmail))?.Split(';'));
                mailCC = mailCC.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                // Bcc
                var configBCC = ConfigurationManager.AppSettings["SendMailBCC"];
                List<string> emailBCCs = configBCC.Split(',').ToList<string>();
                if (contractList.Count > 0)
                {
                    var s = SendMailHelper.Send(subject, body, mailTo, null, mailCC, emailBCCs);

                    #region --- Ghi Log Send Mail ---
                    var logSendMail = new sysSentEmailHistory
                    {
                        SentUser = SendMailHelper._emailFrom,
                        Receivers = string.Join("; ", mailTo),
                        Subject = subject,
                        Sent = s,
                        SentDateTime = DateTime.Now,
                        Body = body,
                        CCs = string.Join("; ", mailCC),
                        BCCs = string.Join("; ", emailBCCs)
                    };
                    var hsLogSendMail = db.sysSentEmailHistories.Add(logSendMail);
                    var hsSc = db.SaveChanges();
                    #endregion --- Ghi Log Send Mail ---
                }
            }
        }

        /// <summary>
        /// Get table content
        /// </summary>
        /// <param name="contracts"></param>
        /// <param name="typeOfExpired"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        private StringBuilder GetContentTableGroupOfContract(List<ExpiredContractClass> contracts, string typeOfExpired, int day = 0)
        {
            StringBuilder content = new StringBuilder();
            if (contracts.Count > 0)
            {
                content.Append(@"<tr>");
                content.Append(@"<tr colspan='5'>&nbsp;</tr>");
                content.Append(@"</tr>");
                content.Append(@"<tr style='background-color: #dddddd;'>");
                if (typeOfExpired == "NearExpired")
                {
                    content.Append(@"<td colspan='5'>&nbsp;&nbsp;<b><i>Hợp đồng <= " + day + " ngày/ Contract <= " + day + " days</i></b></td>");
                }
                else
                {
                    content.Append(@"<td colspan='5'>&nbsp;&nbsp;<b><i>Hợp đồng hết hạn</i></b></td>");
                }
                content.Append(@"</tr>");
                int no = 0;
                foreach (var item in contracts)
                {
                    no++;
                    content.Append(@"<tr>");
                    content.Append(@"<td style='width: 6%; border: 1px solid #dddddd; border-collapse: collapse; text-align: center;'>" + no + "</td>");
                    content.Append(@"<td style='width: 30%; border: 1px solid #dddddd; border-collapse: collapse;'>&nbsp;&nbsp;" + item.PartnerName + "</td>");
                    content.Append(@"<td style='width: 24%; border: 1px solid #dddddd; border-collapse: collapse;'>&nbsp;&nbsp;" + item.ContractNo + "</td>");
                    content.Append(@"<td style='width: 20%; border: 1px solid #dddddd; border-collapse: collapse; text-align: center;'>" + item.EffectiveDate?.ToString("dd/MM/yyy") + "</td>");
                    content.Append(@"<td style='width: 20%; border: 1px solid #dddddd; border-collapse: collapse; text-align: center;'>" + item.ExpiredDate?.ToString("dd/MM/yyy") + "</td>");
                    content.Append(@"</tr>");
                }
            }
            return content;
        }

        public new void Stop()
        {
            FileHelper.WriteToFile("SendMailExpiredAgreement", "[SendMailExpiredAgreementService] [STOP]:" + DateTime.Now);
            _timer.Stop();
            _timer.Dispose();
        }

        protected override void OnStop()
        {
            this.Stop();
        }
    }

    class ExpiredContractClass
    {
        public string PartnerName { get; set; }
        public string ContractNo { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public int? ExpiredIn { get; set; }
        public string SaleManEmail { get; set; }
        public string AccountantEmail { get; set; }
        public string AREmail { get; set; }
    }
}
