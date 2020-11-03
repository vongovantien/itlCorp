using eFMS.API.System.Infrastructure.Extensions;
using eFMS.API.System.Service.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;


namespace eFMS.API.System.Infrastructure.Hubs
{
    public class NotificationHubSubscription : IDatabaseSubscription
    {
        private bool disposedValue = false;
        private readonly IHubContext<NotificationHub> _hubContext;
        private SqlTableDependency<SysNotifications> _tableDependency;

        public NotificationHubSubscription(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public void Configure(string connectionString)
        {
            try
            {
                _tableDependency = new SqlTableDependency<SysNotifications>(connectionString, null, null, null, null, null, DmlTriggerType.All);
                _tableDependency.OnChanged += Changed;
                _tableDependency.OnError += TableDependency_OnError;
                _tableDependency.Start();

                Console.WriteLine("Waiting for receiving notifications...");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void Changed(object sender, RecordChangedEventArgs<SysNotifications> e)
        {
            if (e.ChangeType != ChangeType.None && e.ChangeType == ChangeType.Insert)
            {
                // TODO: manage the changed entity
                var changedEntity = e.Entity;

                _hubContext.Clients.All.SendAsync("NotificationWhenChange", changedEntity);
            }
        }

        private void TableDependency_OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine($"SqlTableDependency error: {e.Error.Message}");
        }

        #region IDisposable

        public void InventoryDatabaseSubscription()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _tableDependency.Stop();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
