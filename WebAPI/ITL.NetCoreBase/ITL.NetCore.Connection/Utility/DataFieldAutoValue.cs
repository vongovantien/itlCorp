using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITL.NetCore.Connection.Utility
{
    public class DataFieldAutoValue
    {
        private DataFieldAutoValueType _autoType = DataFieldAutoValueType.Value;
        public DataFieldAutoValueType AutoType { get { return _autoType; } }

        public string FieldName = string.Empty;

        private Func<object, bool> _fuCheckAllowAutoValue;
        public object FixValue;
        public Func<object, object> FuGenValue;

        public bool NullOnly;
        public object DefaultNullValue;

        public DataFieldAutoValue() { }

        /// <summary>
        /// Apply fixDateNow
        /// </summary>
        /// <param name="fieldName"></param>
        public DataFieldAutoValue(string fieldName)
        {
            Build(fieldName, null, null, null);
        }

        public DataFieldAutoValue(string fieldName, object fixValue)
        {
            Build(fieldName, fixValue, null, null);
        }

        public DataFieldAutoValue(string fieldName, object fixValue,
            Func<object, bool> preCheckAllowAutoValue)
        {
            Build(fieldName, fixValue, null, preCheckAllowAutoValue);
        }

        public DataFieldAutoValue(string fieldName, Func<object, object> preGenValue)
        {
            Build(fieldName, null, preGenValue, null);
        }

        public DataFieldAutoValue(string fieldName,
            Func<object, object> preGenValue, Func<object, bool> preCheckAllowAutoValue)
        {
            Build(fieldName, null, preGenValue, preCheckAllowAutoValue);
        }

        private void Build(string fieldName, object fixValue,
            Func<object, object> preGenValue, Func<object, bool> preCheckAllowAutoValue)
        {
            FieldName = fieldName;
            FixValue = fixValue;
            FuGenValue = preGenValue;
            _fuCheckAllowAutoValue = preCheckAllowAutoValue;
            //..
            if (fixValue == null && preGenValue == null)
                _autoType = DataFieldAutoValueType.DateTimeNow;
            else if (fixValue != null)
                _autoType = DataFieldAutoValueType.Value;
            else
                _autoType = DataFieldAutoValueType.FuncValue;
        }

        public void SetAutoValue(object row)
        {
            if (row != null &&
                _fuCheckAllowAutoValue != null && !_fuCheckAllowAutoValue.Invoke(row))
                return;
            //..
            object value = null;
            var property = row.GetType().GetProperty(FieldName);
            var oldValue = property.GetValue(row, null);
            if (NullOnly && (!NullOnly || (oldValue != null && !oldValue.Equals(DefaultNullValue) && oldValue.ToString() != string.Empty))) return;
            switch (AutoType)
            {
                case DataFieldAutoValueType.DateTimeNow:
                    value = Cast.TryConvert(property, DateTime.Now);
                    break;
                case DataFieldAutoValueType.Value:
                    value = Cast.TryConvert(property, FixValue);
                    break;
                case DataFieldAutoValueType.FuncValue:
                    value = Cast.TryConvert(property, FuGenValue.Invoke(row));
                    break;
            }

            property.SetValue(row, value, null);
        }
    }
}
