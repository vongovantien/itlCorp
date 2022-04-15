using eFMS.API.Common.Globals.Configs;
using eFMS.API.Common.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Common.Helpers
{
    public class LogHelper
    {
        private readonly string exePath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
        private readonly string logName = "eFMS-LOG";

        public LogHelper() { }

        public LogHelper(string logMessage)
        {
            LogWrite(logName, logMessage);
        }

        public LogHelper(string _logName, string logMessage)
        {
            LogWrite(_logName, logMessage);
        }

        public void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nLog Entry : ");
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                txtWriter.WriteLine("  :");
                txtWriter.WriteLine("  :{0}", logMessage);
                txtWriter.WriteLine("-------------------------------");
            }
            catch (Exception)
            {
            }
        }

        public void LogWrite(string logName, string logMessage)
        {
            try
            {
                if (!Directory.Exists(exePath))
                {
                    Directory.CreateDirectory(exePath);
                }
                string filepath = exePath + "\\" + logName + "_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
                if (!System.IO.File.Exists(filepath))
                {
                    // Create a file to write to.   
                    using (StreamWriter sw = System.IO.File.CreateText(filepath))
                    {
                        sw.WriteLine(logMessage);
                    }
                }
                else
                {
                    using (StreamWriter sw = System.IO.File.AppendText(filepath))
                    {
                        sw.WriteLine(logMessage);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public async Task PushWebhook(string url, ResponseExModel model)
        {
            try
            {
                MessageCardWebHook wh = BuildMessageCard(model);
                var data = await HttpClientService.PostAPI(url, wh, null);
            }
            catch (Exception ex)
            {
                LogWrite("Webhook Log", ex.ToString());
            }
        }
        public async Task PushWebhook(string url, MessageCardWebHook model)
        {
            try
            {
                var data = await HttpClientService.PostAPI(url, model, null);
            }
            catch (Exception ex)
            {
                LogWrite("Webhook Log", ex.ToString());
            }
        }

        private MessageCardWebHook BuildMessageCard(ResponseExModel model)
        {
            return new MessageCardWebHook
            {
                Type = "MessageCard",
                ThemeColor = "0076D7",
                Summary = model.Name,
                Sections = new List<MessageCardActivitySection> {
                    new MessageCardActivitySection
                    {
                        ActivityTitle = model.Name,
                        ActivitySubtitle = model.Source,
                        ActivityImage = "https://efms.itlvn.com/en/assets/demo/default/media/img/logo/logo-web.png",
                        Facts = new List<ValueName>
                        {
                            new ValueName
                            {
                                Name = "Code",
                                Value = model.Code.ToString()
                            },
                            new ValueName
                            {
                                Name = "Name",
                                Value = model.Message
                            },
                            new ValueName
                            {
                                Name = "Path",
                                Value = model.Path
                            },
                            new ValueName
                            {
                                Name = "Body",
                                Value = model.Body
                            },
                            new ValueName
                            {
                                Name = "Date",
                                Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
                            },
                            new ValueName
                            {
                                Name = "Status",
                                Value = "False"
                            },
                        }
                    }
                }
            };
        }

    }
}
