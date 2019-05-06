using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ITL.NetCore.Common.HandleValidation
{
    public class HandleValidation
    {

        private List<Validation> lstError = new List<Validation>();
        private object objValidation;
        private List<object> lstObjValidation = new List<object>();
        public bool IsValid => this.lstError.Count == 0;
        public Dictionary<string, IEnumerable<string>> Errors => this.GetErrors();

        public HandleValidation() { }

        public HandleValidation(object objValidation)
        {
            if (objValidation != null && (objValidation is IEnumerable<object> || objValidation is List<object>))
                this.lstObjValidation = (objValidation as IEnumerable<object>).ToList();
            else
                this.objValidation = objValidation;
        }

        public void AddError(object item, string strKey, IEnumerable<string> iError)
        {
            this.lstError.Add(new Validation(item, strKey, iError));
        }

        public void AddError(object item, string strKey, string strError)
        {
            this.AddError(item, strKey, new[] { strError });
        }

        public void AddError(string strKey, IEnumerable<string> iError)
        {
            this.AddError(null, strKey, iError);
        }

        public void AddError(string strKey, string strError)
        {
            this.AddError(null, strKey, strError);
        }

        public Dictionary<string, IEnumerable<string>> GetErrors()
        {
            Dictionary<string, IEnumerable<string>> dicError = new Dictionary<string, IEnumerable<string>>();
            
            foreach (Validation error in this.lstError)
            {
                string strKey = error.Key;
                if (this.lstObjValidation != null && this.lstObjValidation.Count() > 0 && error.Item != null)
                {
                    strKey = String.Format("{0}.{1}", this.lstObjValidation.IndexOf(error.Item), error.Key);
                }

                dicError.Add(strKey, error.Errors);
            }
            return dicError;
        }

    }    
}
