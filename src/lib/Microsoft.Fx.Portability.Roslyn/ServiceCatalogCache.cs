// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Roslyn
{
    public class ServiceCatalogCache : ICatalogCache
    {
        private readonly IApiPortService _service;

        private ImmutableDictionary<string, bool> _cache;
        private ConcurrentStringHashSet _unknown;

        public ServiceCatalogCache(IApiPortService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _cache = ImmutableDictionary.Create<string, bool>(StringComparer.Ordinal);
            _unknown = new ConcurrentStringHashSet();

            Framework = new FrameworkName(".NET Core, Version=3.0.0");
        }

        public FrameworkName Framework { get; }

        public ApiStatus GetApiStatus(string api)
        {
            if (_cache.TryGetValue(api, out var isAvailable))
            {
                return isAvailable ? ApiStatus.Available : ApiStatus.Unavailable;
            }

            _unknown.Add(api);

            return ApiStatus.Unknown;
        }

        public void UpdateCatalog()
        {
            Task.Run(async () =>
            {
                try
                {
                    var unknown = Interlocked.Exchange(ref _unknown, new ConcurrentStringHashSet());
                    var result = await _service.QueryDocIdsAsync(unknown).ConfigureAwait(false);
                    var builder = _cache.ToBuilder();

                    foreach (var api in result.Response)
                    {
                        builder[api.Definition.DocId] = api.Supported.Contains(Framework);
                    }

                    Interlocked.Exchange(ref _cache, builder.ToImmutable());
                }
                catch (Exception)
                {
                }
            });
        }

        private class ConcurrentStringHashSet : IEnumerable<string>
        {
            private readonly ConcurrentDictionary<string, byte> _dict = new ConcurrentDictionary<string, byte>(StringComparer.Ordinal);

            public bool Add(string str) => _dict.TryAdd(str, 0);

            public IEnumerator<string> GetEnumerator() => _dict.Keys.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
