#define USE_SPINWAIT
using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SubWorker.FionDemo
{
    public class FionSubWorkerBackgroundService : BackgroundService
    {
        private CancellationToken _stop;
        private List<JobInfo> readyJobs = new List<JobInfo>();
        private readonly int threadsCnts = 5;

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1);
            this._stop = stoppingToken;

            using (JobsRepo repo = new JobsRepo())
            {
                while(true)
                {
                    readyJobs = repo.GetReadyJobs(JobSettings.MinPrepareTime).ToList();
                    foreach (var job in readyJobs)
                    {
                        if (job.State == 0 && repo.AcquireJobLock(job.Id))
                        {
                            if (repo.ProcessLockedJob(job.Id))
                                Console.WriteLine($"[job ID: {job.Id}] update state....");
                        }
                    }

                    //Thread[] threads = new Thread[threadsCnts];
                    //for (int i = 0; i < threadsCnts; i++)
                    //{
                    //    threads[i] = new Thread(RunThread);
                    //    threads[i].Start();
                    //}

                    try
                    {
                        await Task.Delay(JobSettings.MinPrepareTime, stoppingToken);
                        Console.Write("_");
                    }
                    catch (TaskCanceledException) { break; }
                }

            }

        }
        private void RunThread()
        {
            using (JobsRepo repo = new JobsRepo())
            {
                foreach (var job in readyJobs)
                {
                    if (job.State == 0 && repo.AcquireJobLock(job.Id))
                    {
                        if (repo.ProcessLockedJob(job.Id))
                            Console.WriteLine($"[job ID: {job.Id}] update state....");
                    }
                }

            }
        }

    }
}
