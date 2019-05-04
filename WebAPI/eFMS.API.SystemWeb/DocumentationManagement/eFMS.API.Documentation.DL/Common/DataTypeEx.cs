using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Common
{
    public class DataTypeEx
    {
        public static string GetType(TransactionTypeEnum type)
        {
            string result = "";
            switch (type)
            {
                case TransactionTypeEnum.InlandTrucking:
                    result = TermData.InlandTrucking;
                    break;
                case TransactionTypeEnum.AirExport:
                    result = TermData.AirExport;
                    break;
                case TransactionTypeEnum.AirImport:
                    result = TermData.AirImport;
                    break;
                case TransactionTypeEnum.SeaConsolExport:
                    result = TermData.SeaConsolExport;
                    break;
                case TransactionTypeEnum.SeaConsolImport:
                    result = TermData.SeaConsolImport;
                    break;
                case TransactionTypeEnum.SeaFCLExport:
                    result = TermData.SeaFCLExport;
                    break;
                case TransactionTypeEnum.SeaFCLImport:
                    result = TermData.SeaFCLImport;
                    break;
                case TransactionTypeEnum.SeaLCLExport:
                    result = TermData.SeaLCLExport;
                    break;
                case TransactionTypeEnum.SeaLCLImport:
                    result = TermData.SeaLCLImport;
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}
