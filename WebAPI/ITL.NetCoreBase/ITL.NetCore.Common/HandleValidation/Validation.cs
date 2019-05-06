using System;
using System.Collections.Generic;
using System.Text;

namespace ITL.NetCore.Common.HandleValidation
{
    public class Validation
    {
        public object Item { get; set; }
        public string Key { get; set; }
        public IEnumerable<string> Errors { get; set; }

        public Validation() { }
        public Validation(object item, string key, IEnumerable<string> iError)
        {
            this.Item = item;
            this.Key = key;
            this.Errors = iError;
        }
    }
}
