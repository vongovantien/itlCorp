using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Infrastructure.NoSql
{
    public static class ChangeTrackerHelper
    {
        //public static string currentUser;
        public static ICurrentUser currentUser;
        public static List<AuditLog> GetChangModifield(IEnumerable<EntityEntry> entities)
        {
            var modifiedEntities = entities
                .Where(p => p.State == EntityState.Modified).ToList();

            List<AuditLog> listLog = null;
            if (modifiedEntities.Count > 0)
            {
                listLog = new List<AuditLog>();
                foreach (var change in modifiedEntities)
                {
                    var entityName = change.Entity.GetType().Name;
                    var properties = change.OriginalValues.Properties;
                    if (properties.Count < 1) break;
                    else
                    {
                        var primaryKey = properties.FirstOrDefault(x => x.IsKey());
                        List<PropertyChange> changedProperties = new List<PropertyChange>();
                        //log.PropertyChange = new List<PropertyChange>();
                        foreach (var prop in properties)
                        {
                            if (prop.Name == "UserModified" || prop.Name == "DatetimeModified"
                                || prop.Name == "DatetimeCreated" || prop.Name == "UserCreated"
                                || prop.Name == "ModifiedDate") continue;
                            var originalValue = change.OriginalValues[prop] != null ? change.OriginalValues[prop].ToString() : string.Empty;
                            var currentValue = change.CurrentValues[prop] != null ? change.CurrentValues[prop].ToString() : string.Empty;
                            //if (prop.Name == "InactiveOn" && change.OriginalValues["Inactive"].ToString() == change.OriginalValues["Inactive"].ToString()) continue;
                            if (originalValue != currentValue)
                            {
                                var addObject = new PropertyChange()
                                {
                                    PropertyName = prop.Name,
                                    OldValue = originalValue,
                                    NewValue = currentValue
                                };
                                changedProperties.Add(addObject);
                            }
                        }
                        if (changedProperties != null)
                        {
                            var log = new ItemLog { Id = Guid.NewGuid(),
                                PrimaryKeyValue = change.OriginalValues[primaryKey.Name].ToString(),
                                ActionType = EntityState.Modified,
                                ActionName = "Modified",
                                DatetimeModified = DateTime.Now,
                                UserModified = currentUser.UserID,
                                UserNameModified = currentUser.UserName,
                                CompanyId = true? currentUser.CompanyID.ToString(): string.Empty,
                                OfficeId = true? currentUser.OfficeID.ToString(): string.Empty,
                                DepartmentId = currentUser.DepartmentId,
                                GroupId = currentUser.GroupId,
                                // UserModified = change.CurrentValues["UserModified"]?.ToString()
                            };
                            log.ItemObject = change.Entity;
                            log.Function = currentUser.Action;
                            log.ChangedProperties = changedProperties;
                            var objectLog = new AuditLog { EntityName = entityName, ChangeLog = log };
                            listLog.Add(objectLog);
                        }
                    }
                }
            }
            return listLog;
        }

        /// <summary>
        /// Get log change value between old and new object
        /// </summary>
        /// <param name="oldEntities"></param>
        /// <param name="newEntities"></param>
        /// <returns></returns>
        public static List<AuditLog> GetChangeModifield(IEnumerable<object> oldEntities, IEnumerable<object> newEntities)
        {
            List<AuditLog> listLog = null;
            if (newEntities.Count() > 0)
            {
                listLog = new List<AuditLog>();
                foreach (var oldChange in oldEntities)
                {
                    var entityName = oldChange.GetType().Name;
                    var propertyOld = oldChange.GetType().GetProperties();

                    var surchargeLookup = newEntities.ToLookup(x => x.GetType().GetProperty("Id").GetValue(x));
                    var newChange = surchargeLookup[oldChange.GetType().GetProperty("Id").GetValue(oldChange)].FirstOrDefault();

                    var propertyNew = newChange.GetType().GetProperties();
                    var primaryKey = oldChange.GetType().GetProperty("Id").GetValue(oldChange); // get primarykey value
                    List<PropertyChange> changedProperties = new List<PropertyChange>();
                    for (var i = 0; i< propertyOld.Count(); i++)
                    {
                        var originalValue = propertyOld[i].GetValue(oldChange)?.ToString();
                        var currentValue = propertyNew[i].GetValue(newChange)?.ToString();
                        if (originalValue != currentValue)
                        {
                            var addObject = new PropertyChange()
                            {
                                PropertyName = propertyOld[i].Name,
                                OldValue = originalValue?.ToString(),
                                NewValue = currentValue?.ToString()
                            };
                            changedProperties.Add(addObject);
                        }
                    }
                    if (changedProperties != null)
                    {
                        var log = new ItemLog
                        {
                            Id = Guid.NewGuid(),
                            PrimaryKeyValue = primaryKey.ToString(),
                            ActionType = EntityState.Modified,
                            ActionName = "Modified",
                            DatetimeModified = DateTime.Now,
                            UserModified = currentUser.UserID,
                            UserNameModified = currentUser.UserName,
                            CompanyId = true ? currentUser.CompanyID.ToString() : string.Empty,
                            OfficeId = true ? currentUser.OfficeID.ToString() : string.Empty,
                            DepartmentId = currentUser.DepartmentId,
                            GroupId = currentUser.GroupId,
                        };
                        log.ItemObject = newChange;
                        log.Function = currentUser.Action;
                        log.ChangedProperties = changedProperties;
                        var objectLog = new AuditLog { EntityName = entityName, ChangeLog = log };
                        listLog.Add(objectLog);
                    }
                }
            }
            return listLog;
        }

        public static List<AuditLog> GetAdded(IEnumerable<EntityEntry> entities)
        {
            var addedEntities = entities
                .Where(p => p.State == EntityState.Added).ToList();

            List<AuditLog> listLog = null;
            if (addedEntities.Count > 0)
            {
                listLog = new List<AuditLog>();
                foreach (var add in addedEntities)
                {
                    var entityName = add.Entity.GetType().Name;
                    var properties = add.OriginalValues.Properties;
                    var primaryKey = properties.FirstOrDefault(x => x.IsKey());
                    var log = new ItemLog { Id = Guid.NewGuid(),
                        PrimaryKeyValue = add.OriginalValues[primaryKey.Name].ToString(),
                        ActionType = EntityState.Added,
                        ActionName = "Added",
                        DatetimeModified = DateTime.Now,
                        UserModified = currentUser.UserID,
                        UserNameModified = currentUser.UserName,
                        CompanyId = true ? currentUser.CompanyID.ToString() : string.Empty,
                        OfficeId = true ? currentUser.OfficeID.ToString() : string.Empty,
                        DepartmentId = currentUser.DepartmentId,
                        GroupId = currentUser.GroupId,
                    };
                    log.ItemObject = add.Entity;
                    log.Function = currentUser.Action;
                    var objectLog = new AuditLog { EntityName = entityName, ChangeLog = log };
                    listLog.Add(objectLog);
                }
            }
            return listLog;
        }

        /// <summary>
        /// Get log added object
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static List<AuditLog> GetAdded(IEnumerable<object> entities)
        {
            List<AuditLog> listLog = null;
            if (entities.Count() > 0)
            {
                listLog = new List<AuditLog>();
                foreach (var addEntity in entities)
                {
                    var entityName = addEntity.GetType().Name;
                    var primaryKey = addEntity.GetType().GetProperty("Id").GetValue(addEntity);
                    var log = new ItemLog
                    {
                        Id = Guid.NewGuid(),
                        PrimaryKeyValue = primaryKey.ToString(),
                        ActionType = EntityState.Added,
                        ActionName = "Added",
                        DatetimeModified = DateTime.Now,
                        UserModified = currentUser.UserID,
                        UserNameModified = currentUser.UserName,
                        CompanyId = true ? currentUser.CompanyID.ToString() : string.Empty,
                        OfficeId = true ? currentUser.OfficeID.ToString() : string.Empty,
                        DepartmentId = currentUser.DepartmentId,
                        GroupId = currentUser.GroupId,
                    };
                    log.ItemObject = addEntity;
                    log.Function = currentUser.Action;
                    var objectLog = new AuditLog { EntityName = entityName, ChangeLog = log };
                    listLog.Add(objectLog);
                }
            }
            return listLog;
        }

        public static List<AuditLog> GetDeleted(IEnumerable<EntityEntry> entities)
        {
            var addedEntities = entities
                .Where(p => p.State == EntityState.Deleted).ToList();

            List<AuditLog> listLog = null;
            if (addedEntities.Count > 0)
            {
                listLog = new List<AuditLog>();
                foreach (var delete in addedEntities)
                {
                    var entityName = delete.Entity.GetType().Name;
                    var properties = delete.OriginalValues.Properties;
                    var primaryKey = properties.FirstOrDefault(x => x.IsKey());
                    var log = new ItemLog
                    {
                        Id = Guid.NewGuid(),
                        PrimaryKeyValue = delete.OriginalValues[primaryKey.Name].ToString(),
                        ActionType = EntityState.Deleted,
                        ActionName = "Deleted",
                        DatetimeModified = DateTime.Now,
                        UserModified = currentUser.UserID,
                        UserNameModified = currentUser.UserName,
                        CompanyId = true ? currentUser.CompanyID.ToString() : string.Empty,
                        OfficeId = true ? currentUser.OfficeID.ToString() : string.Empty,
                        DepartmentId = currentUser.DepartmentId,
                        GroupId = currentUser.GroupId,
                    };
                    log.ItemObject = delete.Entity;
                    log.Function = currentUser.Action;
                    var objectLog = new AuditLog { EntityName = entityName, ChangeLog = log };
                    listLog.Add(objectLog);
                }
            }
            return listLog;
        }

        /// <summary>
        /// Get log deleted object
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static List<AuditLog> GetDeleted(IEnumerable<object> entities)
        {
            List<AuditLog> listLog = null;
            if (entities.Count() > 0)
            {
                listLog = new List<AuditLog>();
                foreach (var deleteEntity in entities)
                {
                    var entityName = deleteEntity.GetType().Name;
                    var primaryKey = deleteEntity.GetType().GetProperty("Id").GetValue(deleteEntity);
                    var log = new ItemLog
                    {
                        Id = Guid.NewGuid(),
                        PrimaryKeyValue = primaryKey.ToString(),
                        ActionType = EntityState.Deleted,
                        ActionName = "Deleted",
                        DatetimeModified = DateTime.Now,
                        UserModified = currentUser.UserID,
                        UserNameModified = currentUser.UserName,
                        CompanyId = true ? currentUser.CompanyID.ToString() : string.Empty,
                        OfficeId = true ? currentUser.OfficeID.ToString() : string.Empty,
                        DepartmentId = currentUser.DepartmentId,
                        GroupId = currentUser.GroupId,
                    };
                    log.ItemObject = deleteEntity;
                    log.Function = currentUser.Action;
                    var objectLog = new AuditLog { EntityName = entityName, ChangeLog = log };
                    listLog.Add(objectLog);
                }
            }
            return listLog;
        }

        public static void InsertToMongoDb(List<AuditLog> list)
        {
            if (list == null) return;
            var s = list.GroupBy(x => x.EntityName);
            foreach (var log in s)
            {
                var k = log.Select(x => x.ChangeLog).ToList<object>();
                try
                {
                    MongoDbHelper.InsertMany(log.Key, k);
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }
    }
}
