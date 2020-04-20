using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace SubWorker.FionDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<FionSubWorkerBackgroundService>();
                })
                .Build();
            using (host)
            {
                host.Start();
                host.WaitForShutdown();
            }
        }
    }


}
