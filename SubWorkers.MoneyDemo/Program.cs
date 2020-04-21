using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;

namespace SubWorkers.MoneyDemo
{
    public class Program
    {
        public static List<JobInfo> JobList = new List<JobInfo>();

        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (host)
            {
                host.Start();
                host.WaitForShutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<RetrieveWorker>();
                    services.AddHostedService<ExecuteWorker>();
                });
    }
}
