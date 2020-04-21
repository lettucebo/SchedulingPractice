using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchedulingPractice.Core;

namespace SubWorkers.MoneyDemo
{
    public class RetrieveWorker : BackgroundService
    {
        private readonly ILogger<RetrieveWorker> _logger;

        public RetrieveWorker(ILogger<RetrieveWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var repo = new JobsRepo())
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("RetrieveWorker running at: {time}", DateTimeOffset.Now);

                    // get jobs into queue list

                    // set duration with a delta time to compensate the delay time of various items
                    var duration = (JobSettings.MinPrepareTime + JobSettings.MaxDelayTime).Add(new TimeSpan(0, 0, 0, -5, 0));
                    var jobs = repo.GetReadyJobs(duration).ToList();

                    _logger.LogWarning($"Retrieve job count: {jobs.Count}");


                    // remove duplicate items
                    jobs.RemoveAll(x => Program.JobList.Select(i => i.Id).Contains(x.Id));

                    var obj = new object();
                    lock (obj)
                    {
                        Program.JobList.AddRange(jobs);
                    }

                    await Task.Delay(duration, stoppingToken).ConfigureAwait(false);
                }
            }
        }
    }
}
