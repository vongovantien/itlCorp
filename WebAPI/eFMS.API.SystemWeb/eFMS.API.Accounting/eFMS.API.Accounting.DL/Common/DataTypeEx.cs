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
    }
}
