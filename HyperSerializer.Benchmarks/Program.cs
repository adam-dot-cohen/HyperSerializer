using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Microsoft.Diagnostics.Tracing.Analysis;

namespace Hyper.Benchmarks
{
    class Program
    {
        private static IConfig BenchConfig => DefaultConfig.Instance.AddJob(Job.Default.AsDefault()
          // .WithLaunchCount(1).WithInvocationCount(1).WithWarmupCount(1).WithUnrollFactor(1)
          .WithRuntime(CoreRuntime.Core70)
          .WithJit(Jit.RyuJit)
          .WithArguments(new[] { new MsBuildArgument("/p:Optimize=true") }));
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, BenchConfig);
        }
    }
}
