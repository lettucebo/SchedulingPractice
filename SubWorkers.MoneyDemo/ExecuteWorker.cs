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
    public class ExecuteWorker : BackgroundService
    {
        private readonly ILogger<ExecuteWorker> _logger;

        public ExecuteWorker(ILogger<ExecuteWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var repo = new JobsRepo())
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("ExecuteWorker running at: {time}", DateTimeOffset.Now);

                    // get all jobs need to be process from JobList
                    var readyJobs =
                        Program.JobList.Where(x =>
                            (DateTime.Now - x.RunAt) > new TimeSpan(0, 0, 0, 0, JobSettings.LoopDuration)).ToList();

                    if (readyJobs.Count > 0)
                        _logger.LogWarning($"Current execute job count: {readyJobs.Count}");

                    // process all jobs by using Task.Run
                    foreach (JobInfo job in readyJobs)
                    {
                        //Task.Run(() =>
                        //{
                        var isLock = repo.AcquireJobLock(job.Id);
                        if (isLock)
                        {
                            repo.ProcessLockedJob(job.Id);
                        }
                        //});
                    }

                    var obj = new object();
                    lock (obj)
                    {
                        Program.JobList.RemoveAll(x => readyJobs.Select(i => i.Id).Contains(x.Id));
                    }

                    await Task.Delay(JobSettings.LoopDuration, stoppingToken).ConfigureAwait(false);
                }
            }
        }
    }
}