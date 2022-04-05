# An implementation of (Parallel) Map Sort for Unity

[Unity project](https://github.com/nobnak/TestMapSortUnity) of this module.

```csharp
using Unity.Mathematics;
using MapSort;
using UnityEngine;
using System.Linq;

var p = System.Environment.ProcessorCount;
var rand = new Unity.Mathematics.Random(31);
System.Func<int, int, int> comp0 = (i, j) => (i < j) ? -1 : 1;
var comp = new FuncComaparer<int>(comp0);

var src = Enumerable.Range(0, p).Select(v => rand.NextInt()).ToArray();
var dst = new int[src.Length];

src.Sort(dst, comp, p);
```

## References
1.Edahiro, M. Parallelizing Fundamental Algorithms such as Sorting on Multi-core Processors for EDA Acceleration. 2009 Asia South Pac Des Automation Conf 1, 230–233 (2009).
