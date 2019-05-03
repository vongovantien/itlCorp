using System;
using System.Linq;
using System.Linq.Expressions;
using ITL.NetCore.Common;
using Microsoft.EntityFrameworkCore;

namespace ITL.NetCore.Connection.LinQ
{
    public class DataAutomation<T>
        where T : class, new()
    {
        private readonly DbContext _DContext;
        protected virtual DbContext DContext
        {
            get
            {
                return _DContext;
            }
        }

        public DataAutomation(DbContext datacontext)
        {
            _DContext = datacontext;
        }

        public bool Exist(object val, string FieldName)
        {
            bool result;

            if (val is T)
                result = DContext.Set<T>().AnyAsync(TUtility.GetLambda<T>((T)val, FieldName)).Result;
            else
                result = DContext.Set<T>().AnyAsync(TUtility.GetLambda2<T>(val, FieldName)).Result;

            return result;
        }

        public bool Exist(object[] vals, string[] FieldNames)
        {
            try
            {
                var pre = TUtility.True<T>();
                for (int i = 0; i < FieldNames.Length; i++)
                    pre = pre.AndAlso(TUtility.GetLambda2<T>(vals[i], FieldNames[i]));

                return DContext.Set<T>().AnyAsync(pre).Result;
            }
            catch { }
            return false;
        }

        public bool Exist(T dr, string[] KeyFields)
        {
            try
            {
                var pre = TUtility.True<T>();
                foreach (string KeyField in KeyFields)
                    pre = pre.And(TUtility.GetLambda<T>(dr, KeyField));
                return DContext.Set<T>().AnyAsync(pre).Result;
            }
            catch(Exception ex) { }
            return false;
        }
        public bool Exist(T dr, string[] KeyFields, string[] PrimaryKey)
        {
            try
            {
                var pre = TUtility.True<T>();
                var preC= TUtility.False<T>();
                foreach (string KeyField in PrimaryKey)
                    pre = pre.AndAlso(TUtility.NotEquals<T>(dr, KeyField));
                foreach (string KeyField in KeyFields)
                    preC = preC.AndAlso(TUtility.GetLambda<T>(dr, KeyField));
                pre = pre.AndAlso(preC);
                return DContext.Set<T>().AnyAsync(pre).Result;
            }
            catch (Exception ex) { }
            return false;
        }
        public HandleState Add(T dr)
        {
            try
            {
                DContext.Set<T>().Add(dr);
                DContext.SaveChanges();
                return new HandleState();
            }
            catch (Exception e) { return new HandleState(e); }
        }

        public HandleState Update(T dr, string[] KeyFields)
        {
            try
            {
                var pre = TUtility.True<T>();
                foreach (string KeyField in KeyFields)
                    pre = pre.AndAlso(TUtility.GetLambda<T>(dr, KeyField));

                return Update(dr, pre);
            }
            catch (Exception e) { return new HandleState(e); }
        }

        private HandleState Update(T entity, Expression<Func<T, bool>> query)
        {
            try
            {
                //object propertyValue = null;              
                T entityFromDB = DContext.Set<T>().Where(query).FirstOrDefault();
                if (null == entityFromDB)
                {
                    throw new NullReferenceException("Query Supplied to. Get entity from DB is invalid, NULL value returned");
                }
                //PropertyInfo[] properties = entityFromDB.GetType().GetProperties();
                //foreach (PropertyInfo property in properties)
                //{
                //    propertyValue = null;
                //    if (null != property.GetSetMethod())
                //    {
                //        PropertyInfo entityProperty = entity.GetType().GetProperty(property.Name);
                //        if (entityProperty.PropertyType.BaseType ==
                //            Type.GetType("System.ValueType") ||
                //            entityProperty.PropertyType ==
                //            Type.GetType("System.String"))

                //            propertyValue = entity.GetType().GetProperty(property.Name).GetValue(entity, null);
                //        if (null != propertyValue)
                //            property.SetValue(entityFromDB, propertyValue, null);
                //    }
                //}
                DContext.Entry(entityFromDB).CurrentValues.SetValues(entity);                
                DContext.SaveChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {               
                return new HandleState(ex);
            }
        }

        public HandleState Delete(T dr, string[] KeyFields)
        {
            try
            {              
                var pre = TUtility.True<T>();
                foreach (string KeyField in KeyFields)
                    pre = pre.AndAlso(TUtility.GetLambda<T>(dr, KeyField));

                DContext.Set<T>().Remove(DContext.Set<T>().Where(pre).Single());
                DContext.SaveChanges();
                return new HandleState();
            }
            catch (Exception e) { return new HandleState(e); }
        }

        public HandleState Delete(object[] Values, string[] KeyFields)
        {
            try
            {               
                var pre = TUtility.True<T>();
                for (int i = 0; i < KeyFields.Length; i++)
                    pre = pre.AndAlso(TUtility.GetLambda2<T>(Values[i], KeyFields[i]));

                DContext.Set<T>().RemoveRange(DContext.Set<T>().Where(pre));
                DContext.SaveChanges();
                return new HandleState();
            }
            catch (Exception e) { return new HandleState(e); }
        }
    }
}