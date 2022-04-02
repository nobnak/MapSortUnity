using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuncComaparer<T> : IComparer<T> {

    public readonly System.Func<T, T, int> _Compare;

    public FuncComaparer(System.Func<T, T, int> comp) {
        this._Compare = comp;
    }

    public int Compare(T x, T y) => _Compare(x, y);
}