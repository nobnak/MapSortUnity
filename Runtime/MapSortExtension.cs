using Gist2.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace MapSort {

    public static class MapSortExtension {
        public static readonly ProfilerMarker P_PICK_KEY = new ProfilerMarker("MapSort.PickKeys");
        public static readonly ProfilerMarker P_COUNT_DATA = new ProfilerMarker("MapSort.CountData");
        public static readonly ProfilerMarker P_COPY_DATA = new ProfilerMarker("MapSort.CopyData");
        public static readonly ProfilerMarker P_SORT_INTERVAL = new ProfilerMarker("MapSort.SortIntervals");
        public static readonly string[] SAMPLES = new string[] {
            "MapSort.PickKeys", "MapSort.CountData", "MapSort.CopyData", "MapSort.SortIntervals" };

        public static uint incrementalSeed;

        public static void Sort<T>(this T[] src, T[] dst, 
            IComparer<T> compare,
            int procCount, int minBlockWidth = 10000) {

            var m = math.min(procCount, (int)math.ceil((float)src.Length / minBlockWidth));
            var mwidth = (int)math.ceil((float)src.Length / m);

            // Pick keys
            P_PICK_KEY.Begin();
            var keys = new T[m - 1];
            if (keys.Length > 0) {
                var rand = Unity.Mathematics.Random.CreateFromIndex(incrementalSeed++);
                var keyCandits = new T[math.min(m * 100, src.Length / 1000)];
                for (var i = 0; i < keyCandits.Length - 1; i++)
                    keyCandits[i] = src[rand.NextInt(0, src.Length)];
                System.Array.Sort(keyCandits, compare);
                var di = (float)(keyCandits.Length - 1) / keys.Length;
                for (var i = 0; i < keys.Length; i++)
                    keys[i] = keyCandits[(int)math.round(di * (i + 0.5f))];
                System.Array.Sort(keys, compare);
            }
            P_PICK_KEY.End();

            // Count data
            P_COUNT_DATA.Begin();
            var map = new int[m * m];
            Parallel.For(0, m, i => {
                var start = mwidth * i;
                var end = math.min(mwidth * (i + 1), src.Length);
                for (var j = start; j < end; j++) {
                    var v = src[j];
                    var k = 0;
                    for (; k < keys.Length; k++)
                        if (compare.Compare(v, keys[k]) < 0) break;
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
            P_COUNT_DATA.End();

            // Copy data
            P_COPY_DATA.Begin();
            Parallel.For(0, m, i => {
                var start = mwidth * i;
                var end = math.min(mwidth * (i + 1), src.Length);
                for (var j = start; j < end; j++) {
                    var v = src[j];
                    var k = 0;
                    for (; k < keys.Length; k++)
                        if (compare.Compare(v, keys[k]) < 0) break;
                    var im = k * m + i;
                    var vm = map[im]++;
                    dst[vm] = v;
                }
            });
            P_COPY_DATA.End();

            // Sort intervals
            P_SORT_INTERVAL.Begin();
            Parallel.For(0, m, i => {
                var start = map0[i * m];
                var end = map[(i + 1) * m - 1];
                System.Array.Sort(dst, start, end - start, compare);
            });
            P_SORT_INTERVAL.End();
        }
    }
}
