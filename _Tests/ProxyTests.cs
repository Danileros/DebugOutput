using System.Diagnostics;
using DebugOutput;
using DebugOutput.Examples;

namespace Tests;

public class ProxyTests
{
    [Test]
    public void _IsInactive()
    {
        var debugManager = new DebugManagerProxy(); 
        Assert.IsFalse(debugManager.IsActive);
    }

    [Test]
    public void IsActive()
    {
        // Force add DebugOutput to AppDomain
        DebugManager.GetDebugEnabled("test");
        var debugManager = new DebugManagerProxy();
        Assert.IsTrue(debugManager.IsActive);
    }

    [Test]
    public void Optimization()
    {
        var stubDelegate = () => "stub";
        // warmup
        DebugManager.GetDebugEnabled("test");
        
        var debugManager = new DebugManagerProxy();
        debugManager.GetDebugEnabled("test");
        
        
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100000; i++)
        {
            DebugManager.GetDebugEnabled("test");
        }

        sw.Stop();
        var elapsedOrig = sw.Elapsed.TotalMilliseconds;

        sw.Restart();
        for (int i = 0; i < 100000; i++)
        {
            debugManager.GetDebugEnabled("test");
        }
        sw.Stop();
        var elapsedProxy = sw.Elapsed.TotalMilliseconds;
        
        // Check if hadn't screwed up with optimization
        // Usually proxy is 5% slower than direct call
        // And this does not matter because all those functions are usually called just once
        var performanseDegradation = elapsedProxy / elapsedOrig;
        Assert.Less(performanseDegradation, 1.25);
        Console.WriteLine($"Proxy time compared to direct call: {performanseDegradation}");
    }
}