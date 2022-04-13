using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.ForPartner.DL.Anotations
{
    public class AnotationResult
    {
        public Dictionary<String, List<String>> results = new Dictionary<String, List<String>>();
        public void Add(string field, string message)
        {
            if (results.ContainsKey(field))
            {
                if (results.TryGetValue(field, out List<string> value))
                {
                    results.Remove(field);
                    value.Add(message);
                    results.Add(field, value);
                };
            }
            else
            {
                results.Add(field, new List<String>(message.Split("\r\n")));
            }
        }
        public Boolean Exists()
        {
           
            return results.Count > 0;
        }
    }
}
