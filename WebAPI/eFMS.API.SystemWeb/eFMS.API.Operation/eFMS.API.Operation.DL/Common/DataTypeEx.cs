using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Operation.DL.Common
{
    public class DataTypeEx
    {
        public static string GetStageStatus(StageEnum stageEnum)
        {
            string result = string.Empty;
            switch (stageEnum)
            {
                case StageEnum.InSchedule:
                    result = OperationConstants.InSchedule;
                    break;
                case StageEnum.Processing:
                    result = OperationConstants.Processing;
                    break;
                case StageEnum.Pending:
                    result = OperationConstants.Pending;
                    break;
                case StageEnum.Done:
                    result = OperationConstants.Done;
                    break;
                case StageEnum.Overdue:
                    result = OperationConstants.Overdue;
                    break;
                case StageEnum.Deleted:
                    result = OperationConstants.Deleted;
                    break;
            }
            return result;
        }
    }
}
