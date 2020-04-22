using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Common
{
    public static class ReportUltity
    {
        public static string ReplaceHtmlBaseForPreviewReport(string html)
        {
            if (string.IsNullOrEmpty(html)) return html;
            if (html.Contains("<strong>"))
            {
                html = html.Replace("<strong>", "<b>");
            }
            if (html.Contains("</strong>"))
            {
                html = html.Replace("</strong>", "</b>");
            }
            if (html.Contains("<em>"))
            {
                html = html.Replace("<em>", "<i>");
            }
            if (html.Contains("</em>"))
            {
                html = html.Replace("</em>", "</i>");
            }
            return html;
        }

        public static string ReplaceNullAddressDescription(string description)
        {
            if (string.IsNullOrEmpty(description)) return description;
            if (description.Contains("null"))
            {
                description = description.Replace("null", "");
            }
            return description;
        }
    }
}
