﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dive.Utility.CsharpExtensions
{
    internal class TrivialGrouping<TKey, TSource> : IGrouping<TKey, TSource>
    {
        public TrivialGrouping(TKey key, IList<TSource> items)
        {
            Key = key;
            Items = items;
        }

        public IList<TSource> Items { get; }
        public TKey Key { get; }

        public IEnumerator<TSource> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}