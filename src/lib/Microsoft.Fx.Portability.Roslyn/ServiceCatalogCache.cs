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
    public sealed class ServiceCatalogCache : ICatalogCache, IDisposable
    {
        private readonly IAnalyzerSettings _settings;
        private readonly IApiPortService _service;
        private readonly CancellationTokenSource _cts;
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentDictionary<string, bool> _cache;

        private ConcurrentStringHashSet _unknown;

        public ServiceCatalogCache(IApiPortService service, IAnalyzerSettings settings)
        {
            _settings = settings;
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _cache = new ConcurrentDictionary<string, bool>(StringComparer.Ordinal);
            _semaphore = new SemaphoreSlim(0, 1);
            _unknown = new ConcurrentStringHashSet();
            _cts = new CancellationTokenSource();

            _settings.PropertyChanged += SettingsPropertyChanged;

            Task.Run(async () => await UpdateCatalogAsync());
        }

        private void SettingsPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, nameof(IAnalyzerSettings.IsAutomaticAnalyze), StringComparison.Ordinal) && !_settings.IsAutomaticAnalyze)
            {
                _cache.Clear();
                _unknown.Clear();
            }
        }

        public FrameworkName Framework { get; } = new FrameworkName(".NET Core, Version=3.0");

        public void Dispose()
        {
            _cts.Cancel();
            _semaphore.Dispose();
        }

        public ApiStatus GetApiStatus(string api)
        {
            if (!_settings.IsAutomaticAnalyze)
            {
                return ApiStatus.Off;
            }

            if (_cache.TryGetValue(api, out var isAvailable))
            {
                return isAvailable ? ApiStatus.Available : ApiStatus.Unavailable;
            }

            _unknown.Add(api);
            _semaphore.Release();

            return ApiStatus.Unknown;
        }

        public async Task UpdateCatalogAsync()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    await _semaphore.WaitAsync(_cts.Token).ConfigureAwait(false);

                    // Give a bit of time in case the buffer is being filled by the compiler
                    await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

                    if (_unknown.Count > 0)
                    {
                        var unknown = Interlocked.Exchange(ref _unknown, new ConcurrentStringHashSet());

                        try
                        {
                            var result = await _service.QueryDocIdsAsync(unknown).ConfigureAwait(false);

                            foreach (var api in result.Response)
                            {
                                _cache.TryAdd(api.Definition.DocId, api.Supported.Contains(Framework));
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private class ConcurrentStringHashSet : IEnumerable<string>
        {
            private readonly ConcurrentDictionary<string, byte> _dict = new ConcurrentDictionary<string, byte>(StringComparer.Ordinal);

            public bool Add(string str) => _dict.TryAdd(str, 0);

            public int Count => _dict.Count;

            public void Clear() => _dict.Clear();

            public IEnumerator<string> GetEnumerator() => _dict.Keys.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
