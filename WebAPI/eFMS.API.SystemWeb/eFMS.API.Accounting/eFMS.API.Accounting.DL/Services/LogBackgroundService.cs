using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.Services
{
    public class eFMSQueueBackgroundService : BackgroundService
    {
        public IBackgroundTaskQueue TaskQueue { get; }
        public eFMSQueueBackgroundService(IBackgroundTaskQueue _taskQueue)
        {
            TaskQueue = _taskQueue;
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("eFMS Queued Hosted Service is starting.");

            while (!cancellationToken.IsCancellationRequested)
            {
                var workItem = await TaskQueue.DequeueAsync(cancellationToken);
                await workItem(cancellationToken);
            }

            Console.WriteLine("eFMS Queued Hosted Service is stopped.");
        }
    }
}
