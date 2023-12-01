// <copyright file="Program.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

namespace Orleans.Streaming.Grains.Performance
{
    public class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
            RunVerbose(args);
#else
            // Run(args);
            RunSummaries(args);
#endif
        }

        private static void Run(string[] args)
        {
            var config = DefaultConfig.Instance;

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                             .Run(args, config);
        }

        private static void RunVerbose(string[] args)
        {
            var config = new DebugInProcessConfig();

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                             .Run(args, config);
        }

        private static void RunSummaries(string[] args)
        {
            var config = new ManualConfig
            {
                UnionRule = ConfigUnionRule.AlwaysUseGlobal
            };

            config.AddJob(DefaultConfig.Instance.GetJobs().ToArray());
            config.AddExporter(DefaultConfig.Instance.GetExporters().ToArray());
            config.AddAnalyser(DefaultConfig.Instance.GetAnalysers().ToArray());
            config.AddDiagnoser(DefaultConfig.Instance.GetDiagnosers().ToArray());
            config.AddValidator(DefaultConfig.Instance.GetValidators().ToArray());
            config.AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().ToArray());

            var logger = ConsoleLogger.Default;
            var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                                             .Run(args, config);

            foreach (var summary in summaries)
            {
                MarkdownExporter.Console.ExportToLog(summary, logger);
                ConclusionHelper.Print(logger, summary.BenchmarksCases.First().Config.GetCompositeAnalyser().Analyse(summary).ToList());
            }
        }
    }
}