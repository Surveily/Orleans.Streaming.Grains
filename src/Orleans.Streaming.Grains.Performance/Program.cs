// <copyright file="Program.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Orleans.Streaming.Grains.Performance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = DefaultConfig.Instance;

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                             .Run(args, config);
        }

        /*
        public static void Main(string[] args)
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

            var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                                             .Run(args, config);

            foreach (var summary in summaries)
            {
                MarkdownExporter.Console.ExportToLog(summary, ConsoleLogger.Default);
                ConclusionHelper.Print(ConsoleLogger.Default, config.GetAnalysers()
                                                                    .First()
                                                                    .Analyse(summary)
                                                                    .ToList());
            }
        }
        */
    }
}