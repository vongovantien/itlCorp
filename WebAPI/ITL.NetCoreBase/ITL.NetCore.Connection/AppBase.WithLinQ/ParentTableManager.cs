using ITL.NetCore.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ITL.NetCore.Connection.LinQ
{
    public interface IParentTableManager
    {
        bool HaveParent(object drMaster);
    }

    public class ParentTableManager<TMaster> : IParentTableManager 
        where TMaster : class, new()
    {
        public List<string> _masterField = new List<string>();
        public List<string> _childField = new List<string>();
        protected DataAutomation<TMaster> _dParentExec;
        private object _fuParentExist = null;

        public ParentTableManager(DbContext context, string ChildField, string MasterField)
        {
            _dParentExec = new DataAutomation<TMaster>(context);          
            _childField = ChildField.Split(',').ToList();
            _masterField = MasterField.Split(',').ToList();
            GenParentExistFunc();
        }
        private void GenParentExistFunc()
        {
            Expression<Func<object, bool>> preExist
                = dr => (dr != null && (_dParentExec.Exist(_childField.Select(f => ObjectUtility.GetValue(dr, f)).ToArray()
                                            , _masterField.ToArray()) || _childField.All(f => ObjectUtility.GetValue(dr, f) == null)));
            _fuParentExist = TUtility.ExpressionToFunc(preExist);
        }
        public bool HaveParent(object dr)
        {
            bool ParentExist = false;

            if (_fuParentExist != null)
                ParentExist = (bool)ObjectUtility.ExecFunction(_fuParentExist, dr);
            return ParentExist;
        }
    }
}