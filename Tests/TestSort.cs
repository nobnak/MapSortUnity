using NUnit.Framework;
using Unity.Mathematics;
using MapSort;
using UnityEngine;
using System.Linq;
using Unity.PerformanceTesting;

public class TestSort {
    // A Test behaves as an ordinary method
    [Test]
    [Performance]
    public void TestSortSimplePasses() {
        var p = System.Environment.ProcessorCount;
        var rand = new Unity.Mathematics.Random(31);
        System.Func<int, int, int> comp = (i, j) => (i < j) ? -1 : 1;
        var comp1 = new FuncComaparer<int>(comp);

        var src = Enumerable.Range(0, p).Select(v => rand.NextInt()).ToArray();
        var dst = new int[src.Length];
        src.Sort(dst, comp, p, 1);

        //Debug.Log($"src: {string.Join(",", src)}\ndst: {string.Join(",", dst)}");
        System.Array.Sort(src);
        Assert.IsTrue(src.SequenceEqual(dst));

        var n = 100000;
        src = Enumerable.Range(0, n).Select(v => rand.NextInt()).ToArray();
        dst = new int[src.Length];
        src.Sort(dst, comp, p, 1);
        System.Array.Sort(src, comp1);
        Assert.IsTrue(src.SequenceEqual(dst));

        Measure.Method(() => {
            src.Sort(dst, comp, 1, 1);
        }).SampleGroup("Sequential Sort")
            .MeasurementCount(10)
            .Run();
        Measure.Method(() => {
            src.Sort(dst, comp, p);
        }).SampleGroup($"Parallel Sort: p={p}")
            .MeasurementCount(10).Run();

        Measure.Method(() => {
            System.Array.Sort(src, comp1);
        }).SampleGroup(".net Sort")
        .MeasurementCount(10)
        .Run();
    }

}

