using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace MapSort {

    public static class MapSortExtension {

        public static void Sort<T>(this T[] src, T[] dst, 
            System.Func<T, T, int> compare,
            int procCount, int minBlockWidth = 1000) {

            var m = math.min(procCount, (int)math.ceil((float)src.Length / minBlockWidth));
            var mwidth = (int)math.ceil((float)src.Length / m);
            //Debug.Log($"MapSort: splits={m}, width={mwidth}");

            // Pick keys
            Profiler.BeginSample("MapSort.PickKeys");
            var rand = Unity.Mathematics.Random.CreateFromIndex((uint)src.Length);
            var keys = new T[m - 1];
            for (var i = 0; i < keys.Length - 1; i++)
                keys[i] = src[math.clamp(rand.NextInt(i * mwidth, (i+1) * mwidth), 0, src.Length)];
            System.Array.Sort(keys);
            Profiler.EndSample();

            // Count data
            Profiler.BeginSample("MapSort.CountData");
            var map = new int[m * m];
            Parallel.For(0, m, i => {
                var start = mwidth * i;
                var end = math.min(mwidth * (i + 1), src.Length);
                for (var j = start; j < end; j++) {
                    var v = src[j];
                    var k = 0;
                    for (; k < keys.Length; k++)
                        if (compare(v, keys[k]) < 0) break;
                    map[i + k * m]++;
                }
            });
            var sum = 0;
            for(var i=0; i < map.Length; i++) {
                var v = map[i];
                map[i] = sum;
                sum += v;
            }
            var map0 = new int[map.Length];
            System.Array.Copy(map, map0, map.Length);

            // Copy data
            Profiler.BeginSample("MapSort.CopyData");
            Parallel.For(0, m, i => {
                var start = mwidth * i;
                var end = math.min(mwidth * (i + 1), src.Length);
                for (var j = start; j < end; j++) {
                    var v = src[j];
                    var k = 0;
                    for (; k < keys.Length; k++)
                        if (compare(v, keys[k]) < 0) break;
                    var im = k * m + i;
                    var vm = map[im]++;
                    dst[vm] = v;
                }
            });

            // Sort intervals
            Profiler.BeginSample("MapSort.SortIntervals");
            Parallel.For(0, m, i => {
                var start = map0[i * m];
                var end = map[(i + 1) * m - 1];
                System.Array.Sort(dst, start, end - start);
            });
        }
    }
}
