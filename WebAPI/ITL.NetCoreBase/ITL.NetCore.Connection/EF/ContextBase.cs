using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using ITL.NetCore.Common;
using ITL.NetCore.Common.HandleValidation;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.LinQ;
using ITL.NetCore.Connection.Utility;
using Microsoft.EntityFrameworkCore;

namespace ITL.NetCore.Connection.EF
{
    public class ContextBase<T> : IContextBase<T>
        where T : class, new()
    {
        private DbContext _DC;
        private DbSet<T> _entities;
        private List<IChildTableManager> _childrenMA = new List<IChildTableManager>();
        private List<IParentTableManager> _parentMA = new List<IParentTableManager>();
        private List<string[]> _keyFields = new List<string[]>();
        private List<string[]> _allowColumnEdit = new List<string[]>();
        private List<string[]> _notAllowColumnEdit = new List<string[]>();
        private List<string[]> _trimColumns = new List<string[]>();
        private String _errorMessage = String.Empty;

        public List<DataColumnError> _lValidation = new List<DataColumnError>();
        public List<DataFieldAutoValue> _lAutoValue = new List<DataFieldAutoValue>();

        public string[] PrimaryKey
        {
            get
            {
                var fields = DC.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(t => t.Name).ToArray();
                return fields;
            }
        }

        private DataAutomation<T> _dExec;
        private DataAutomation<T> dExec
        {
            get
            {
                if (_dExec == null)
                {
                    _dExec = new DataAutomation<T>(DC);
                }
                return _dExec;
            }
        }
        public DbSet<T> Entities
        {
            get
            {
                if (_entities == null)
                {
                    if (_DC == null)
                    {
                        _DC = RetrieveDataContext.Invoke();
                    }
                    _entities = _DC.Set<T>();
                }
                return _entities;
            }
        }
        public DbContext DC
        {
            get
            {
                if (_DC == null)
                {
                    _DC = RetrieveDataContext.Invoke();

                    //if (_DC != null) _DC.Database.SetCommandTimeout(60);
                }
                _entities = _DC.Set<T>();
                return _DC;
            }
        }

        private static Func<DbContext> RetrieveDataContext;
        public static void ConfigDataContext<D>(string connectionString)
            where D : DbContext, new()
        {
            RetrieveDataContext = () =>
            {
                D d = new D();
                d.Database.GetDbConnection().ConnectionString = connectionString;
                return d;
            };
        }      
        public void SetChildren<TChild>(string MasterField, string ChildField, bool AllowDeleteChild)
        where TChild : class, new()
        {
            ChildTableManager<TChild> ch = new ChildTableManager<TChild>(DC, MasterField, ChildField, AllowDeleteChild);
            _childrenMA.Add(ch);
        }
        public void SetParent<TParent>(string ChildrentField, string ParentField)
             where TParent : class, new()
        {            
            ParentTableManager<TParent> parentTable = new ParentTableManager<TParent>(DC, ChildrentField, ParentField);
            _parentMA.Add(parentTable);
        }

        public void AddAutoTrimColumns(string[] columns) {
            if (!_trimColumns.Exists(t => t == columns))
                _trimColumns.Add(columns);
        }

        private void SetTrimColumns(T entity) {
            foreach(var cols in _trimColumns) {
                entity = ObjectUtility.TrimByProperties(entity, cols);
            }
        }

        public void SetUnique(string[] keyfields)
        {
            if (!_keyFields.Exists(t => t == keyfields))
                _keyFields.Add(keyfields);
        }
        public void SetAllowEditColumn(string[] columns, bool isAllowEdit, string strError)
        {
            if (isAllowEdit && !_allowColumnEdit.Exists(t => t == columns))
                _allowColumnEdit.Add(columns);
            else if (!isAllowEdit && !_notAllowColumnEdit.Exists(t => t == columns))
                _notAllowColumnEdit.Add(columns);
            this._errorMessage = strError;
        }        
        public void AddValidation(DataColumnError validation)
        {
            _lValidation.Add(validation);
        }
        //private HandleState ValidateBeforeSave(T dr, DataModeType mode)
        //{
        //    foreach (var validation in _lValidation)
        //    {
        //        if (!validation.Validate(dr))
        //            return new HandleState(validation._error);
        //    }
        //    return new HandleState();
        //}


        private HandleState ValidateBeforeSave(T dr, DataModeType mode)
        {
            HandleValidation hv = new HandleValidation(dr);
            foreach (var validation in _lValidation)
            {
                if (!validation.Validate(dr))
                {
                    hv.AddError(validation._dataField, validation._error);
                }
            }
            if (!hv.IsValid)
                return new HandleState(hv.Errors);
            return new HandleState();
        }

        public bool CheckExist(T entitty, DataModeType mode)
        {
            foreach (var key in _keyFields)
            {
                if (mode == DataModeType.Edit)
                    return CheckExist(entitty, key, PrimaryKey);
                else
                {
                    return CheckExist(entitty, key);
                }
            }
            return false;
        }
        private bool CheckExist(T entitty, string[] keyfields)
        {
            return dExec.Exist(entitty, keyfields);
        }
        //For Edit
        private bool CheckExist(T entitty, string[] keyfields, string[] PrimaryKey)
        {
            return dExec.Exist(entitty, keyfields, PrimaryKey);
        }
        public bool HaveParent(T entitty)
        {
            if (entitty != null)
            {
                foreach (IParentTableManager ch in _parentMA)
                    if (!ch.HaveParent(entitty))
                        return false;
            }
            return true;
        }
        private void DeleteChildren(object dr)
        {
            if (dr != null)
            {
                foreach (IChildTableManager ch in _childrenMA)
                    ch.Delete(dr);
            }
        }

        private bool CheckChildrenDeleteOK(object entitty)
        {
            if (entitty != null)
            {
                foreach (IChildTableManager ch in _childrenMA)
                    if (!ch.MasterDeleteOK(entitty))
                        return false;
            }
            return true;
        }

        private HandleState CheckAllowEdit(T oldEntity, T newEntity)
        {
            if (_allowColumnEdit.Count == 0 && _notAllowColumnEdit.Count == 0)
                return new HandleState();

            HandleValidation hv = new HandleValidation(newEntity);

            if(oldEntity == null)
            {
                oldEntity = new T();
            }

            foreach (PropertyInfo prop in newEntity.GetType().GetProperties())
            {
                object oldVal = oldEntity.GetType().GetProperty(prop.Name).GetValue(oldEntity);
                object newVal = prop.GetValue(newEntity);

                if (
                    _allowColumnEdit.Count > 0 && !_allowColumnEdit.Exists(t => t.Any(u => u == prop.Name))
                    || _notAllowColumnEdit.Count > 0 && _notAllowColumnEdit.Exists(t => t.Any(u => u == prop.Name))
                    || (!_allowColumnEdit.Exists(t => t.Any(u => u == prop.Name)) && _notAllowColumnEdit.Exists(t => t.Any(u => u == prop.Name)))
                  )
                {
                    if ((oldVal != null || newVal != null) && oldVal != newVal)
                    {
                        prop.SetValue(newEntity, oldVal);
                        hv.AddError(prop.Name, this._errorMessage);
                    }
                }
            }

            if (!hv.IsValid)
                return new HandleState(hv.Errors);
            return new HandleState();
        }


        public void AddAutoValue(DataFieldAutoValue dataFieldAutoValue)
        {
            _lAutoValue.Add(dataFieldAutoValue);
        }


        private void SetAutoValue(T row)
        {
            if (row == null)
                return;

            foreach (var df in _lAutoValue)
                df.SetAutoValue(row);
        }

        public ContextBase()
        {           
        }
        //public ContextBase(DbContext context) {
        //    _DC = context;
        //    _DC.Set<T>();
        //}
        public IQueryable<T> Get()
        {
            return Entities;
        }
        public async Task<List<T>> GetAsync()
        {
            return await Entities.ToListAsync();
        }
        public IQueryable<T> Where(Expression<Func<T, bool>> exp)
        {
            return Entities.Where(exp);
        }
        public async Task<List<T>> WhereAsync(Expression<Func<T, bool>> exp)
        {
            return await Entities.Where(exp).ToListAsync();
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> query)
        {
            return Get(query, -1, -1);
        }

        public async Task<List<T>> GetAsync(Expression<Func<T, bool>> query)
        {
            return await Entities.AsNoTracking().Where(query).ToListAsync();
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> query, int from, int count)
        {
            IQueryable<T> q = (query == null) ? Get() : Get().Where(query);
            if (from > -1)
                q = q.Skip(from);
            if (count > -1)
                q = q.Take(count);

            return q;
        }
        public async Task<List<T>> GetAsync(Expression<Func<T, bool>> query, int from, int count)
        {
            IQueryable<T> q = (query == null) ? Get() : Get().Where(query);

            if (from > -1)
                q = q.Skip(from);
            if (count > -1)
                q = q.Take(count);

            return await q.ToListAsync();
        }

        public T First(Expression<Func<T, bool>> query)
        {
            return Get().FirstOrDefault(query);
        }
        public async Task<T> FirstAsync(Expression<Func<T, bool>> query)
        {
            return await Get().AsNoTracking().FirstOrDefaultAsync();
        }

        public bool Any(Expression<Func<T, bool>> query)
        {
            return Get().Any(query);
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> query)
        {
            return await Get().AsNoTracking().AnyAsync();
        }

        //public bool Any(T entity, IEnumerable<string> keyFields)
        //{
        //    try
        //    {
        //        var predicate = keyFields
        //            .Aggregate(TUtility.True<T>(), (current, keyField) => current.And(DataContextEx.Predicate<T>(entity, keyField)));
        //        return Get().Any(predicate);
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        //public bool Any(string propertyName, object value)
        //{
        //    return value is T
        //               ? Get().Any(DataContextEx.Predicate<T>((T)value, propertyName))
        //               : Get().Any(DataContextEx.Predicate<T>(propertyName, value));
        //}

        //public bool Any2(string propertyName, object value)
        //{
        //    object realValue = value;
        //    if (value is T)
        //        realValue = ObjectUtility.GetValue(value, propertyName);

        //    Func<T, bool> preBody = t => ObjectUtility.GetValue(t, propertyName) == realValue;
        //    return Get().ToList<T>().Any(preBody);
        //}

        public int Count(Expression<Func<T, bool>> query)
        {
            return Where(query).Count();
        }
        public async Task<int> CountAsync(Expression<Func<T, bool>> query)
        {
            return await Where(query).AsNoTracking().CountAsync();
        }

        public IQueryable<T> Paging(Expression<Func<T, bool>> query, int page, int size, out int rowsCount)
        {
            return Paging(query, page, size, null, false, out rowsCount);
        }
        public async Task<(List<T> result, int rowsCount)> PagingAsync(Expression<Func<T, bool>> query, int page, int size)
        {
            return await PagingAsync(null, page, size, null, false);
        }

        public IQueryable<T> Paging(int page, int size, out int rowsCount)
        {
            return Paging(null, page, size, null, false, out rowsCount);
        }

        public async Task<(List<T> result, int rowsCount)> PagingAsync(int page, int size)
        {
            return await PagingAsync(null, page, size, null, false);
        }

        public IQueryable<T> Paging(int page, int size, Expression<Func<T, object>> orderByProperty, bool isAscendingOrder, out int rowsCount)
        {
            var q = Get();
            return q.Paging(page, size, orderByProperty, isAscendingOrder, out rowsCount);
        }
        public IQueryable<T> Paging(Expression<Func<T, bool>> query, int page, int size, Expression<Func<T, object>> orderByProperty, bool isAscendingOrder, out int rowsCount)
        {
            var q = query == null ? Get() : Get().Where(query);
            return q.Paging(page, size, orderByProperty, isAscendingOrder, out rowsCount);
        }
        public async Task<(List<T> result, int rowsCount)> PagingAsync(Expression<Func<T, bool>> query, int page, int size, Expression<Func<T, object>> orderByProperty, bool isAscendingOrder)
        {
            var q = query == null ? Get() : Get().Where(query);
            return await q.PagingAsync(page, size, orderByProperty, isAscendingOrder);
        }

        public HandleState Add(T entity)
        {
            return Add(entity, true);
        }

        public Task<HandleState> AddAsync(T entity)
        {
            return AddAsync(entity, true);
        }

        public HandleState PrepareBeforeModified(T entityOld, T entityNew, DataModeType dataMode)
        {
            this.SetTrimColumns(entityNew);
            this.SetAutoValue(entityNew);

            if (dataMode == DataModeType.Edit)
            {
                if (entityOld == null)
                    return new HandleState(201, "Entity is not found");

                if (CheckExist(entityNew, dataMode))
                    return new HandleState(203, "Object exists");
                if (!HaveParent(entityNew))
                    return new HandleState(204, "Haven't parent");

                HandleState hs = CheckAllowEdit(entityOld, entityNew);
                if (!hs.Success)
                    return hs;

                hs = ValidateBeforeSave(entityNew, dataMode);
                if (!hs.Success)
                    return hs;

            }
            else if(dataMode == DataModeType.AddNew) {

                if (CheckExist(entityNew, dataMode))
                    return new HandleState(203, "Object exists");
                if (!HaveParent(entityNew))
                    return new HandleState(204, "Haven't parent");

                HandleState hs = CheckAllowEdit(null, entityNew);
                if (!hs.Success)
                    return hs;

                hs = ValidateBeforeSave(entityNew, dataMode);
                if (!hs.Success)
                    return hs;
            }            

            return new HandleState();
        }
        
        public HandleState Add(T entity, bool allowSubmitChange)
        {
            try
            {

                HandleState hs = this.PrepareBeforeModified(null, entity, DataModeType.AddNew);
                if (!hs.Success)
                    return hs;

                Entities.Add(entity);
                if (allowSubmitChange)
                    return SubmitChanges();

                return new HandleState();
            }
            catch (Exception ex) { return new HandleState(ex); }
        }
        public async Task<HandleState> AddAsync(T entity, bool allowSubmitChange)
        {
            try
            {
                //if (CheckExist(entity, DataModeType.AddNew))
                //    return new HandleState(203, "Object exists");
                //if (!HaveParent(entity))
                //    return new HandleState(204, "Haven't parent");

                //HandleState hs = CheckAllowEdit(entity, entity, DataModeType.AddNew);
                //if (!hs.Success)
                //    return hs;

                //hs = ValidateBeforeSave(entity, DataModeType.AddNew);
                //if (!hs.Success)
                //    return hs;

                HandleState hs = this.PrepareBeforeModified(null, entity, DataModeType.AddNew);
                if (!hs.Success)
                    return hs;

                await Entities.AddAsync(entity);
                if (allowSubmitChange)
                    return await SubmitChangesAsync();

                return new HandleState();
            }
            catch (Exception ex) { return new HandleState(ex); }
        }

        public HandleState Add(IEnumerable<T> entities)
        {
            return Add(entities, true);
        }

        public async Task<HandleState> AddAsync(IEnumerable<T> entities)
        {
            return await AddAsync(entities, true);
        }

        public HandleState Add(IEnumerable<T> entities, bool allowSubmitChange)
        {
            try
            {
                foreach(var entity in entities) {
                    HandleState hs = this.PrepareBeforeModified(null, entity, DataModeType.AddNew);
                    if (!hs.Success)
                        return hs;
                }
                Entities.AddRange(entities);
                if (allowSubmitChange)
                    return SubmitChanges();

                return new HandleState();
            }
            catch (Exception ex) { return new HandleState(ex); }
        }
        public async Task<HandleState> AddAsync(IEnumerable<T> entities, bool allowSubmitChange)
        {
            try
            {
                await Entities.AddRangeAsync(entities);
                if (allowSubmitChange)
                    return await SubmitChangesAsync();

                return new HandleState();
            }
            catch (Exception ex) { return new HandleState(ex); }
        }

        public HandleState Update(T entity, Expression<Func<T, bool>> query)
        {
            return Update(entity, query, true);
        }
        public async Task<HandleState> UpdateAsync(T entity, Expression<Func<T, bool>> query)
        {
            return await UpdateAsync(entity, query, true);
        }

        public HandleState Update(T entity, Expression<Func<T, bool>> query, bool allowSubmitChange)
        {
            try
            {
                var entityFromDB = First(query);

                //if (entityFromDB == null)
                //    return new HandleState(201, "Entity is not found");

                //if (CheckExist(entity, DataModeType.Edit))
                //    return new HandleState(203, "Not unique");

                //if (!HaveParent(entity))
                //    return new HandleState(204, "Haven't parent");

                //HandleState hs = CheckAllowEdit(entity, entity, DataModeType.Edit);
                //if (!hs.Success)
                //    return hs;

                //hs = ValidateBeforeSave(entity, DataModeType.Edit);
                //if (!hs.Success)
                //    return hs;

                HandleState hs = this.PrepareBeforeModified(entityFromDB, entity, DataModeType.Edit);
                if (!hs.Success)
                    return hs;

                DC.Entry(entityFromDB).CurrentValues.SetValues(entity);
                if (allowSubmitChange)
                    return SubmitChanges();

                return new HandleState();
            }
            catch (Exception ex) { return new HandleState(ex); }
        }
        public async Task<HandleState> UpdateAsync(T entity, Expression<Func<T, bool>> query, bool allowSubmitChange)
        {
            try
            {
                T entityFromDB = await FirstAsync(query);

                //if (null == entityFromDB)
                //    return new HandleState(201, "Entity is not found");

                //if (CheckExist(entity, DataModeType.Edit))
                //    return new HandleState(203, "Not unique");

                //if (!HaveParent(entity))
                //    return new HandleState(204, "Haven't parent");

                //HandleState hs = CheckAllowEdit(entity, entity, DataModeType.Edit);
                //if (!hs.Success)
                //    return hs;

                //hs = ValidateBeforeSave(entity, DataModeType.Edit);
                //if (!hs.Success)
                //    return hs;

                HandleState hs = this.PrepareBeforeModified(entityFromDB, entity, DataModeType.Edit);
                if (!hs.Success)
                    return hs;

                DC.Entry(entityFromDB).CurrentValues.SetValues(entity);
                if (allowSubmitChange)
                    return await SubmitChangesAsync();

                return new HandleState();
            }
            catch (Exception ex) { return new HandleState(ex); }
        }

        //public HandleState Update(T entity, Expression<Func<T, bool>> query, IEnumerable<string> propertyNames, bool reverse, bool allowSubmitChange)
        //{
        //    try
        //    {
        //        T entityFromDB = First(query);
        //        if (entityFromDB == null)
        //            return new HandleState(new NullReferenceException());

        //        DC.Entry(entityFromDB).CurrentValues.SetValues(entity);
        //        if (allowSubmitChange)
        //            return SubmitChanges();

        //        return new HandleState();
        //    }
        //    catch (Exception ex) { return new HandleState(ex); }
        //}
        //public async Task<HandleState> UpdateAsync(T entity, Expression<Func<T, bool>> query, IEnumerable<string> propertyNames, bool reverse, bool allowSubmitChange)
        //{
        //    try
        //    {
        //        T entityFromDB = await FirstAsync(query);
        //        if (entityFromDB == null)
        //            return new HandleState(new NullReferenceException());

        //        DC.Entry(entityFromDB).CurrentValues.SetValues(entity);
        //        if (allowSubmitChange)
        //            return SubmitChanges();

        //        return new HandleState();
        //    }
        //    catch (Exception ex) { return new HandleState(ex); }
        //}
        public HandleState Delete(Expression<Func<T, bool>> query)
        {
            return Delete(query, true);
        }
        public async Task<HandleState> DeleteAsync(Expression<Func<T, bool>> query)
        {
            return await DeleteAsync(query, true);
        }
        public HandleState Delete(Expression<Func<T, bool>> query, bool allowSubmitChange)
        {
            try
            {
                var lEntities = Entities.Where(query);

                if (!lEntities.Any())
                    return new HandleState(201, "Do not have object to delete");

                bool deleteAll = true;
                foreach (var ety in lEntities)
                {
                    if (!CheckChildrenDeleteOK(ety))
                        deleteAll = false;
                    else
                    {
                        DeleteChildren(ety);
                        Entities.Remove(ety);
                    }
                }               
                HandleState state = new HandleState();

                if (allowSubmitChange)
                    state = SubmitChanges();

                if (!deleteAll)
                    state = new HandleState(202, "Have entity cannot remove due to have child object");

                return state;
            }
            catch (Exception ex) { return new HandleState(ex); }
        }
        public async Task<HandleState> DeleteAsync(Expression<Func<T, bool>> query, bool allowSubmitChange)
        {
            try
            {
                var lEntities = Entities.Where(query);

                if (!lEntities.Any())
                    return new HandleState(201, "Do not have object to delete");

                bool deleteAll = true;
                foreach (var ety in lEntities)
                {
                    if (!CheckChildrenDeleteOK(ety))
                        deleteAll = false;
                    else
                        Entities.Remove(ety);
                }
                HandleState state = new HandleState();

                if (allowSubmitChange)
                    state = await SubmitChangesAsync();

                if (!deleteAll)
                    state = new HandleState(202, "Have entity cannot remove due to have child object");

                return state;
            }
            catch (Exception ex) { return new HandleState(ex); }
        }

        public HandleState SubmitChanges()
        {
            try
            {
                DC.SaveChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex);
            }
        }
        public async Task<HandleState> SubmitChangesAsync()
        {
            try
            {
                await DC.SaveChangesAsync();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex);
            }
        }
        private T initCreated(T entity)
        {
           var list=entity.GetType().GetProperties();
            foreach (var prop in list)
            {
                if(prop.Name== "DatetimeCreated")
                {
                    prop.SetValue(entity, DateTime.Now);
                    break;
                }
                if (prop.Name == "UserCreated")
                {
                    prop.SetValue(entity, DateTime.Now);
                    break;
                }
            }
            return entity;
        }
        #region IDisposable Members

        public void Dispose()
        {
            DC.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
