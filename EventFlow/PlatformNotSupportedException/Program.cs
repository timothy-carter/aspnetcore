using Microsoft.AspNetCore.Hosting;
using Microsoft.Diagnostics.EventFlow;
using Microsoft.Diagnostics.EventFlow.HealthReporters;
using Microsoft.Diagnostics.EventFlow.Inputs;
using Microsoft.Diagnostics.EventFlow.Outputs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace PlatformNotSupportedException
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging((hostBuilderContext, loggingBuilder) =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.Services.TryAdd(ServiceDescriptor.Singleton(CreatePipeline()));
                    loggingBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, EventFlowLoggerProvider>(factory =>
                    {
                        var diagnosticPipeline = factory.GetRequiredService<DiagnosticPipeline>();

                        return new EventFlowLoggerProvider(
                            (LoggerInput)diagnosticPipeline.Inputs.FirstOrDefault(o => o is LoggerInput),
                            diagnosticPipeline.HealthReporter
                            );
                    }));
                });

        public static DiagnosticPipeline CreatePipeline()
        {
            var healthReporter = new CsvHealthReporter(new CsvHealthReporterConfiguration
            {
                AssumeSharedLogFolder = false,
                EnsureOutputCanBeSaved = false,
                LogFileFolder = ".",
                LogFilePrefix = "HealthReport",
                LogRetentionInDays = 1,
                MinReportLevel = "Warning",
                SingleLogFileMaximumSizeInMBytes = 100,
                ThrottlingPeriodMsec = 1000,
            });

            var inputs = new IObservable<EventData>[]
            {
                new LoggerInput(healthReporter),
            };

            var sinks = new EventSink[]
            {
                new EventSink(new StdOutput(healthReporter), null)
            };

            return new DiagnosticPipeline(
                healthReporter,
                inputs,
                null,
                sinks,
                null,
                true,
                null
                );
        }
    }
}
