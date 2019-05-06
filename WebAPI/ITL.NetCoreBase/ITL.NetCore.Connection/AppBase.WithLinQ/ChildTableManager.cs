using ITL.NetCore.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ITL.NetCore.Connection.LinQ
{
    public interface IChildTableManager
    {
        void Delete(object drMaster);
        bool HaveChildren(object drMaster);
        bool MasterDeleteOK(object drMaster);
    }

    public class ChildTableManager<T> : IChildTableManager
        where T : class, new()
    {
        public List<string> _masterField = new List<string>();
        public List<string> _childField = new List<string>();
        public bool _allowDeleteChild = false;
        protected DataAutomation<T> _dExec;
        private object _fuDelete = null;
        private object _fuExist = null;

        public ChildTableManager(DbContext context, string MasterField, string ChildField, bool AllowDeleteChild)
        {
            _dExec = new DataAutomation<T>(context);
            _masterField = MasterField.Split(',').ToList();
            _childField = ChildField.Split(',').ToList();
            _allowDeleteChild = AllowDeleteChild;
            GenDeleteFunc();
            GenExistFunc();
        }

        private void GenDeleteFunc()
        {
            Expression<Func<object, HandleState>> preDelete
                = drMaster => _dExec.Delete(_masterField.Select(f => ObjectUtility.GetValue(drMaster, f)).ToArray()
                                            , _childField.ToArray());
            _fuDelete = TUtility.ExpressionToFunc(preDelete);
        }

        private void GenExistFunc()
        {
            Expression<Func<object, bool>> preExist
                = drMaster => drMaster != null && _dExec.Exist(_masterField.Select(f => ObjectUtility.GetValue(drMaster, f)).ToArray()
                                            , _childField.ToArray());
            _fuExist = TUtility.ExpressionToFunc(preExist);
        }

        public void Delete(object drMaster)
        {
            if (_allowDeleteChild && _fuDelete != null)
                ObjectUtility.ExecFunction(_fuDelete, drMaster);
        }

        public bool HaveChildren(object drMaster)
        {
            bool ChildExist = false;

            if (_fuExist != null)
                ChildExist = (bool)ObjectUtility.ExecFunction(_fuExist, drMaster);
            return ChildExist;
        }

        public bool MasterDeleteOK(object drMaster)
        {
            return (_allowDeleteChild || (!_allowDeleteChild && !HaveChildren(drMaster)));
        }
    }
}