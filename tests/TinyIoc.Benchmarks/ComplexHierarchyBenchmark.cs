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
    public class ComplexHierarchyBenchmark
    {
	    [BenchmarkCategory("ComplexHierarchy"), Benchmark]
	    public DepA Original_DeepHierarchy()
	    {
		    var retVal = new TinyIoC.Original.TinyIoCContainer();
		    return retVal.Resolve<DepA>();
	    }
	    
	    [BenchmarkCategory("ComplexHierarchy"), Benchmark]
	    public DepA New_DeepHierarchy()
	    {
		    var retVal = new TinyIoC.TinyIoCContainer();
		    return retVal.Resolve<DepA>();
	    }
    }

    public class DepA
    {
	    public DepA(DepB depB, DepC depC, DepD depD, DepE depE, DepF depF, DepG depG, DepH depH, DepI depI, DepJ depJ, DepK depK, DepL depL, DepM depM, DepN depN, DepO depO, DepP depP, DepQ depQ, DepR depR, DepS depS, DepT depT) { }
    }
    
    public class DepB
    {
	    public DepB(DepC depC, DepD depD, DepE depE, DepF depF, DepG depG, DepH depH, DepI depI, DepJ depJ, DepK depK, DepL depL, DepM depM, DepN depN, DepO depO, DepP depP, DepQ depQ, DepR depR, DepS depS, DepT depT) { }
    }

    public class DepC
    {
	    public DepC(DepD depD, DepE depE, DepF depF, DepG depG, DepH depH, DepI depI, DepJ depJ, DepK depK, DepL depL, DepM depM, DepN depN, DepO depO, DepP depP, DepQ depQ, DepR depR, DepS depS, DepT depT) { }
    }

    
    public class DepD
    {
	    public DepD(DepE depE, DepF depF, DepG depG, DepH depH, DepI depI, DepJ depJ, DepK depK, DepL depL, DepM depM, DepN depN, DepO depO, DepP depP, DepQ depQ, DepR depR, DepS depS, DepT depT) { }
    }
    
    public class DepE
    {
	    public DepE(DepF depF, DepG depG, DepH depH, DepI depI, DepJ depJ, DepK depK, DepL depL, DepM depM, DepN depN, DepO depO, DepP depP, DepQ depQ, DepR depR, DepS depS, DepT depT) { }
    }
    
    public class DepF
    {
	    public DepF(DepG depG, DepH depH, DepI depI, DepJ depJ, DepK depK, DepL depL, DepM depM, DepN depN, DepO depO, DepP depP, DepQ depQ, DepR depR, DepS depS, DepT depT) { }
    }
    
    public class DepG
    {
	    public DepG(DepH depH, DepI depI, DepJ depJ, DepK depK, DepL depL, DepM depM, DepN depN, DepO depO, DepP depP, DepQ depQ, DepR depR, DepS depS, DepT depT) { }
    }
    
    public class DepH
    {
	    public DepH(DepI depI, DepJ depJ, DepK depK, DepL depL, DepM depM, DepN depN, DepO depO, DepP depP, DepQ depQ, DepR depR, DepS depS, DepT depT) { }
    }
    
    public class DepI
    {
	    public DepI(DepJ depJ, DepK depK, DepL depL, DepM depM, DepN depN, DepO depO, DepP depP, DepQ depQ, DepR depR, DepS depS, DepT depT) { }
    }
    
    public class DepJ
    {
	    public DepJ(DepK depK, DepL depL, DepM depM, DepN depN, DepO depO, DepP depP, DepQ depQ, DepR depR, DepS depS, DepT depT) { }
    }
    
    public class DepK
    {
	    public DepK(DepL depL, DepM depM, DepN depN, DepO depO, DepP depP, DepQ depQ, DepR depR, DepS depS, DepT depT) { }
    }
    
    public class DepL
    {
	    public DepL(DepM depM, DepN depN, DepO depO, DepP depP, DepQ depQ, DepR depR, DepS depS, DepT depT) { }
    }
    
    public class DepM
    {
	    public DepM(DepN depN, DepO depO, DepP depP, DepQ depQ, DepR depR, DepS depS, DepT depT) { }
    }
    
    public class DepN
    {
	    public DepN(DepO depO, DepP depP, DepQ depQ, DepR depR, DepS depS, DepT depT) { }
    }
    
    public class DepO
    {
	    public DepO(DepP depP, DepQ depQ, DepR depR, DepS depS, DepT depT) { }
    }
    
    public class DepP
    {
	    public DepP(DepQ depQ, DepR depR, DepS depS, DepT depT) { }
    }
    
    public class DepQ
    {
	    public DepQ(DepR depR, DepS depS, DepT depT) { }
    }
    
    public class DepR
    {
	    public DepR(DepS depS, DepT depT) { }
    }
    
    public class DepS
    {
	    public DepS(DepT depT) { }
    }
    
    public class DepT
    {
    }
}