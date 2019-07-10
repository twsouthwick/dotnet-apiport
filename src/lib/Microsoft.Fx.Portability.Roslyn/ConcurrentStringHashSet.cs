// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.Roslyn
{
    internal class ConcurrentStringHashSet : IEnumerable<string>
    {
        private readonly ConcurrentDictionary<string, byte> _dict = new ConcurrentDictionary<string, byte>(StringComparer.Ordinal);

        public bool Add(string str) => _dict.TryAdd(str, 0);

        public int Count => _dict.Count;

        public void Clear() => _dict.Clear();

        public IEnumerator<string> GetEnumerator() => _dict.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
