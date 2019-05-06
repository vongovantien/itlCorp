using ITL.NetCore.Common;
using System;

namespace ITL.NetCore.Connection.Utility
{
    public class DataColumnError
    {
        public string _dataField = "";
        public string _error = "";

        public string _defaultText = "";
        public virtual bool Validate(object dr) { return true; }

        protected void SetNormal(string DataField, string Error)
        {
            _dataField = DataField;
            _error = Error;
            _defaultText = Error;
        }
    }

    public class DataColumnBlankError : DataColumnError
    {
        public DataColumnBlankError(string DataField, string Error)
        {
            SetNormal(DataField, Error);
        }

        public override bool Validate(object dr)
        {
            object value = ObjectUtility.GetValue(dr, _dataField);

            if (value is Guid)
                return ((Guid)value != Guid.Empty);

            return (value != null && !string.IsNullOrEmpty(value.ToString()));
        }
    }

    public class DataColumnKeysExistError : DataColumnError
    {
        private Func<object, bool> fu;

        public DataColumnKeysExistError(string DataField, string Error, Func<object, bool> pre)
        {
            SetNormal(DataField, Error);
            fu = pre;
        }

        public override bool Validate(object dr)
        {
            return !fu.Invoke(dr);
        }
    }

    public class DataColumnDynamicRulesError : DataColumnError
    {
        private Func<object, bool> fu;

        public DataColumnDynamicRulesError(string DataField, string Error, Func<object, bool> pre)
        {
            SetNormal(DataField, Error);
            fu = pre;
        }

        public override bool Validate(object dr)
        {
            return fu.Invoke(dr);
        }
    }
}