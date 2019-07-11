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
                    result = Constants.InSchedule;
                    break;
                case StageEnum.Processing:
                    result = Constants.Processing;
                    break;
                case StageEnum.Pending:
                    result = Constants.Pending;
                    break;
                case StageEnum.Done:
                    result = Constants.Done;
                    break;
                case StageEnum.Overdue:
                    result = Constants.Overdue;
                    break;
                case StageEnum.Deleted:
                    result = Constants.Deleted;
                    break;
            }
            return result;
        }
    }
}
