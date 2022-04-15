using NUnit.Framework;
using Unity.Mathematics;
using MapSort;
using UnityEngine;
using System.Linq;
using Unity.PerformanceTesting;

public class TestSort {

    [Test]
    public void TestSortSequence() {
        var p = System.Environment.ProcessorCount;
        var rand = new Unity.Mathematics.Random(31);
        System.Func<int, int, int> comp0 = (i, j) => (i < j) ? -1 : 1;
        var comp = new FuncComaparer<int>(comp0);

        var src = Enumerable.Range(0, p).Select(v => rand.NextInt()).ToArray();
        var dst = new int[src.Length];
        src.Sort(dst, comp, p, 1);

        //Debug.Log($"src: {string.Join(",", src)}\ndst: {string.Join(",", dst)}");
        System.Array.Sort(src);
        Assert.IsTrue(src.SequenceEqual(dst));

        var n = 1000000;
        src = Enumerable.Range(0, n).Select(v => rand.NextInt()).ToArray();
        dst = new int[src.Length];
        src.Sort(dst, comp, p, 1);
        System.Array.Sort(src, comp);
        Assert.IsTrue(src.SequenceEqual(dst));
    }

    [Test]
    [Performance]
    public void TestSize10000() {
        var n = 10000;
        Benchmark(n);
    }
    [Test]
    [Performance]
    public void TestSize100000() {
        var n = 100000;
        Benchmark(n);
    }
    [Test]
    [Performance]
    public void TestSize1000000() {
        var n = 1000000;
        Benchmark(n);
    }

    private static void Benchmark(int n) {
        var p = System.Environment.ProcessorCount;
        var rand = new Unity.Mathematics.Random(31);
        System.Func<int, int, int> comp0 = (i, j) => (i < j) ? -1 : 1;
        var comp = new FuncComaparer<int>(comp0);

        var src = Enumerable.Range(0, n).Select(v => rand.NextInt()).ToArray();
        var dst = new int[src.Length];

        var markers = MapSortExtension.SAMPLES.Select(v => new SampleGroup(v, SampleUnit.Millisecond)).ToArray();

        Measure.Method(() => {
            src.Sort(dst, comp, 1);
        }).SampleGroup("Sequential Sort")
        .Run();
        Measure.Method(() => {
            src.Sort(dst, comp, p);
        }).SampleGroup($"Parallel Sort: p={p}")
        .ProfilerMarkers(markers)
        .Run();

        Measure.Method((System.Action)(() => {
            System.Array.Sort(src, (System.Collections.Generic.IComparer<int>)comp);
        })).SampleGroup(".net Sort")
        .Run();
    }
}

