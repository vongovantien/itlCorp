using Cronos;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Common.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.Services
{
    public class PrepaidOverDueBackgroundService : BackgroundService
    {
        public IServiceScopeFactory _services { get; }
        public PrepaidOverDueBackgroundService(IServiceScopeFactory service)
        { 
            _services = service;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await WaitForNextSchedule("0 1 * * *"); // At 1:00am every day.
                    new LogHelper("PrepaidOverDueBackgroundService", "RUNNING at " + DateTime.Now);
                    using (var scope = _services.CreateScope())
                    {
                        var scopedService = scope.ServiceProvider.GetRequiredService<IAccAccountReceivableHostedService>();
                        scopedService.CalculatetorReceivableOverDuePrepaidAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                new LogHelper("PrepaidOverDueBackgroundService", " ERROR at " + DateTime.Now + " " + ex.ToString() + " ");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            try
            {
                new LogHelper("PrepaidOverDueBackgroundService", "STOPPED at " + DateTime.Now);
                await base.StopAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                new LogHelper("PrepaidOverDueBackgroundService", " ERROR at " + DateTime.Now + " " + ex.ToString() + " ");
                throw;
            }

        }

        private async Task WaitForNextSchedule(string cronExpression)
        {
            var parsedExp = CronExpression.Parse(cronExpression);
            var currentTime = DateTimeOffset.Now.DateTime;
            var occurenceTime = parsedExp.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local);

            var delay = occurenceTime.GetValueOrDefault() - currentTime;
            new LogHelper(string.Format("PrepaidOverDueBackgroundService"), "Delay " + delay + "\n");

            await Task.Delay(delay);
        }
    }
}
