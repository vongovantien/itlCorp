using System.Collections.Generic;
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
                case ARTypeEnum.NoAgreement:
                    result = TermData.AR_NoAgreement;
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

        public static bool IsNullOrValue(decimal? value, decimal valueToCheck)
        {
            return (value ?? valueToCheck) == valueToCheck;
        }

        /// <summary>
        /// Display company logo when offices in HN, DN, HCM
        /// </summary>
        /// <param name="officeCode"></param>
        /// <returns></returns>
        public static bool IsCommonOffice(string officeCode)
        {
            var validCodeOffice = new List<string>()
            {
                AccountingConstants.OFFICE_CODE_HAN,
                AccountingConstants.OFFICE_CODE_DAD,
                AccountingConstants.OFFICE_CODE_HCM
            };
            return validCodeOffice.Any(z => z == officeCode);
        }
    }
}
