using eFMS.API.Catalogue.Service.ViewModels;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using eFMS.IdentityServer.DL.UserManager;

namespace eFMS.API.Catalogue.Service.Helpers
{
    public class ChangeTrackerHelper
    {
        public static string currentUser;
        public static List<EntityChangeLog> GetChangModifield(IEnumerable<EntityEntry> entities)
        {
            var modifiedEntities = entities
                .Where(p => p.State == EntityState.Modified).ToList();
            
            List<EntityChangeLog> listLog = null;
            if (modifiedEntities.Count > 0)
            {
                listLog = new List<EntityChangeLog>();
                foreach (var change in modifiedEntities)
                {
                    var entityName = change.Entity.GetType().Name;
                    var properties = change.OriginalValues.Properties;
                    if (properties.Count < 1) break;
                    else
                    {
                        var primaryKey = properties.FirstOrDefault(x => x.IsKey());
                        var log = new ChangeLog { Id = Guid.NewGuid() };
                        log.PropertyCommon = new PropertyCommon {
                            PrimaryKeyValue = change.OriginalValues[primaryKey.Name].ToString(),
                            ActionType = EntityState.Modified,
                            DatetimeModified = DateTime.Now,
                            UserModified = change.CurrentValues["UserModified"]?.ToString()
                        };
                        log.NewObject = change.Entity;
                        //log.PropertyChange = new List<PropertyChange>();
                        foreach (var prop in properties)
                        {
                            if (prop.Name == "UserModified" || prop.Name == "DatetimeModified"
                                || prop.Name == "DatetimeCreated" || prop.Name == "UserCreated") continue;
                            var originalValue = change.OriginalValues[prop] !=null? change.OriginalValues[prop].ToString(): string.Empty;
                            var currentValue = change.CurrentValues[prop] != null? change.CurrentValues[prop].ToString(): string.Empty;
                            //if (prop.Name == "InactiveOn" && change.OriginalValues["Inactive"].ToString() == change.OriginalValues["Inactive"].ToString()) continue;
                            if (originalValue != currentValue)
                            {
                                var addObject = new PropertyChange()
                                {
                                    PropertyName = prop.Name,
                                    OldValue = originalValue,
                                    NewValue = currentValue
                                };
                                log.PropertyCommon.PropertyChange = addObject;
                            }
                        }
                        var objectLog = new EntityChangeLog { EntityName = entityName, ChangeLog = log };
                        listLog.Add(objectLog);
                    }

                }
            }
            return listLog;
        }

        public static List<EntityChangeLog> GetAdded(IEnumerable<EntityEntry> entities)
        {
            var addedEntities = entities
                .Where(p => p.State == EntityState.Added).ToList();

            List<EntityChangeLog> listLog = null;
            if (addedEntities.Count > 0)
            {
                listLog = new List<EntityChangeLog>();
                foreach (var add in addedEntities)
                {
                    var entityName = add.Entity.GetType().Name;
                    var properties = add.OriginalValues.Properties;
                    var primaryKey = properties.FirstOrDefault(x => x.IsKey());
                    var log = new ChangeLog { Id = Guid.NewGuid() };
                    log.PropertyCommon = new PropertyCommon
                    {
                        PrimaryKeyValue = add.OriginalValues[primaryKey.Name].ToString(),
                        ActionType = EntityState.Added,
                        DatetimeModified = DateTime.Now,
                        UserModified = add.CurrentValues["UserCreated"]?.ToString()
                    };
                    log.NewObject = add.Entity;
                    var objectLog = new EntityChangeLog { EntityName = entityName, ChangeLog = log };
                    listLog.Add(objectLog);
                }
            }
            return listLog;
        }
        public static List<EntityChangeLog> GetDeleted(IEnumerable<EntityEntry> entities)
        {
            var addedEntities = entities
                .Where(p => p.State == EntityState.Deleted).ToList();

            List<EntityChangeLog> listLog = null;
            if (addedEntities.Count > 0)
            {
                listLog = new List<EntityChangeLog>();
                foreach (var delete in addedEntities)
                {
                    var entityName = delete.Entity.GetType().Name;
                    var properties = delete.OriginalValues.Properties;
                    var primaryKey = properties.FirstOrDefault(x => x.IsKey());
                    var log = new ChangeLog
                    {
                        Id = Guid.NewGuid(),
                    };
                    log.PropertyCommon = new PropertyCommon
                    {
                        PrimaryKeyValue = delete.OriginalValues[primaryKey.Name].ToString(),
                        ActionType = EntityState.Deleted,
                        DatetimeModified = DateTime.Now,
                        UserModified = currentUser ?? string.Empty
                    };
                    log.NewObject = delete.Entity;
                    var objectLog = new EntityChangeLog { EntityName = entityName, ChangeLog = log };
                    listLog.Add(objectLog);
                }
            }
            return listLog;
        }
        public static void InsertToMongoDb(List<EntityChangeLog> list, EntityState state)
        {
            if (list == null) return;
            var s = list.GroupBy(x => x.EntityName);
            foreach(var log in s)
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
