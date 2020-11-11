using System.Linq;

namespace eFMS.API.Accounting.DL.Common
{
    public class DataTypeEx
    {
        public static string GetTypeAR(ARTypeEnum aRTypeEnum)
        {
            string result = string.Empty;
            switch (aRTypeEnum)
            {
                case ARTypeEnum.TrialOrOffical:
                    result = TermData.AR_TrialOrOffical;
                    break;
                case ARTypeEnum.Guarantee:
                    result = TermData.AR_Guarantee;
                    break;
                case ARTypeEnum.Other:
                    result = TermData.AR_Other;
                    break;
            }
            return result;
        }

        public static string GetServiceNameOfSoa(string serviceTypeId)
        {
            var serviceName = string.Empty;
            if (!string.IsNullOrEmpty(serviceTypeId))
            {
                //Tách chuỗi serviceType thành mảng
                string[] arrayStrServiceTypeId = serviceTypeId.Split(';').Where(x => x.ToString() != string.Empty).ToArray();
                //Xóa các serviceTypeId trùng
                string[] arrayGrpServiceTypeId = arrayStrServiceTypeId.Distinct<string>().ToArray();                
                serviceName = string.Join("; ", arrayGrpServiceTypeId.Select(s => CustomData.Services.Where(x => x.Value == s).FirstOrDefault()?.DisplayName.Trim() ?? string.Empty));
            }
            return serviceName;
        }
    }
}
