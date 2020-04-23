﻿using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Common
{
    public class DataTypeEx
    {
        public static string GetJobStatus(JobStatus jobEnum)
        {
            string result = string.Empty;
            switch (jobEnum)
            {
                case JobStatus.InSchedule:
                    result = TermData.InSchedule;
                    break;
                case JobStatus.Processing:
                    result = TermData.Processing;
                    break;
                case JobStatus.Pending:
                    result = TermData.Pending;
                    break;
                case JobStatus.Finish:
                    result = TermData.Finish;
                    break;
                case JobStatus.Overdued:
                    result = TermData.Overdue;
                    break;
                case JobStatus.Canceled:
                    result = TermData.Canceled;
                    break;
            }
            return result;
        }
        public static string GetStageStatus(StageEnum stageEnum)
        {
            string result = string.Empty;
            switch (stageEnum)
            {
                case StageEnum.InSchedule:
                    result = TermData.InSchedule;
                    break;
                case StageEnum.Processing:
                    result = TermData.Processing;
                    break;
                case StageEnum.Pending:
                    result = TermData.Pending;
                    break;
                case StageEnum.Done:
                    result = TermData.Done;
                    break;
                case StageEnum.Overdue:
                    result = TermData.Overdue;
                    break;
                case StageEnum.Deleted:
                    result = TermData.Deleted;
                    break;
            }
            return result;
        }
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

        public static TransactionTypeEnum GetEnumType(string type)
        {
            TransactionTypeEnum result = 0;
            if(type == TermData.InlandTrucking)
            {
                result = TransactionTypeEnum.InlandTrucking;
            }
            else if(type == TermData.AirExport)
            {
                result = TransactionTypeEnum.AirExport;
            }
            else if(type == TermData.AirImport)
            {
                result = TransactionTypeEnum.AirImport;
            }
            else if (type == TermData.SeaConsolExport)
            {
                result = TransactionTypeEnum.SeaConsolExport;
            }
            else if (type == TermData.SeaConsolImport)
            {
                result = TransactionTypeEnum.SeaConsolImport;
            }
            else if(type == TermData.SeaFCLExport)
            {
                result = TransactionTypeEnum.SeaFCLExport;
            }
            else if(type == TermData.SeaFCLImport)
            {
                result = TransactionTypeEnum.SeaFCLImport;
            }
            else if (type == TermData.SeaLCLExport)
            {
                result = TransactionTypeEnum.SeaLCLExport;
            }
            else if(type == TermData.SeaLCLImport)
            {
                result = TransactionTypeEnum.SeaLCLImport;
            }
            return result;
        }
    }
}
