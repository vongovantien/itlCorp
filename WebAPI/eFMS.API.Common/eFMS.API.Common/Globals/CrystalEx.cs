using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace eFMS.API.Common.Globals
{
    public static class CrystalEx
    {
        public static string GetLogoEFMS()
        {
            string folderReportPreview = GetFolderReportPreview().FirstOrDefault();
            string folderResult = string.Empty;
            if (folderReportPreview.Count() > 0)
            {
                folderResult = Directory.GetDirectories(folderReportPreview).Where(s => s.Contains("Images")).FirstOrDefault() + "\\logo-eFMS.png";
            }
            return folderResult;
        }

        public static string GetLogoITL()
        {
            string folderReportPreview = GetFolderReportPreview().FirstOrDefault();
            string folderResult = string.Empty;
            if (folderReportPreview.Count() > 0)
            {
                folderResult = Directory.GetDirectories(folderReportPreview).Where(s => s.Contains("Images")).FirstOrDefault() + "\\logo-ITL.png";
            }
            return folderResult;
        }

        public static string GetFolderDownloadReports()
        {
            string folderReportPreview = GetFolderReportPreview().FirstOrDefault();
            string folderDownloadReport = string.Empty;
            if (folderReportPreview.Count() > 0)
            {
                folderDownloadReport = Directory.GetDirectories(folderReportPreview).Where(s => s.Contains("DownloadReports")).FirstOrDefault();
            }
            return folderDownloadReport;
        }

        public static IEnumerable<string> GetFolderReportPreview()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo directoryInfo = new DirectoryInfo(currentDirectory);
            string partialName = "ReportPreview";
            var pathFolder = directoryInfo.FullName;
            IEnumerable<string> list = Directory.GetDirectories(pathFolder).Where(s => s.Contains(partialName));
            var result = KeepSearchingFolder(list, directoryInfo, pathFolder, partialName);
            return result;
        }

        public static IEnumerable<string> KeepSearchingFolder(IEnumerable<string> list, DirectoryInfo directoryInfo, string pathFolder, string folderNameSearch)
        {
            if (list.Count() > 0)
                return list;
            else
            {
                directoryInfo = new DirectoryInfo(pathFolder);
                if (directoryInfo.Parent == null) return list;
                string pathParent = directoryInfo.Parent.FullName;
                list = Directory.GetDirectories(pathParent).Where(s => s.Contains(folderNameSearch));
                return KeepSearchingFolder(list, directoryInfo, pathParent, folderNameSearch);
            }
        }
    }
}
