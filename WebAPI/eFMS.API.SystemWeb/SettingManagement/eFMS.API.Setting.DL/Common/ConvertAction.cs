using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Common
{
    public class ConvertAction
    {
        public static string ConvertLinqAction(EntityState state)
        {
            string result = string.Empty;
            switch (state)
            {
                case EntityState.Added:
                    result = Constants.Insert;
                    break;
                case EntityState.Modified:
                    result = Constants.Update;
                    break;
                case EntityState.Deleted:
                    result = Constants.Delete;
                    break;
            }
            return result;
        }
    }
}
