using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace TinyIoc.Benchmarks
{

#if NET48 //These work on either platform but no point running them twice
	[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net48)]
	[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net461)]
#endif
#if NETCOREAPP3_1_OR_GREATER // These don't seem to work in a FX app
	[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.NetCoreApp31)]
	[RyuJitX86Job]
#endif
#if NET48 //These don't seem to work in a .net core app
	[RyuJitX64Job]
	[MonoJob]
#endif
	[MemoryDiagnoser]
	public class AutoRegisterBenchmarks
	{

		[BenchmarkCategory("AutoResolve"), Benchmark(Baseline = true)]
		public TinyIoC.Original.TinyIoCContainer Original_AutoRegister()
		{
			var retVal = new TinyIoC.Original.TinyIoCContainer();
			retVal.AutoRegister();
			return retVal;
		}

		[BenchmarkCategory("AutoResolve"), Benchmark]
		public TinyIoC.TinyIoCContainer New_AutoRegister()
		{
			var retVal = new TinyIoC.TinyIoCContainer();
			retVal.AutoRegister();
			return retVal;
		}

	}
}
