// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.Fx.Portability.Analyzer;
using System;
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
        private readonly IDependencyFilter _filter;
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentDictionary<string, HashSet<FrameworkName>> _cache;

        private ConcurrentStringHashSet _unknown;
        private ImmutableArray<FrameworkName> _currentNames;

        public ServiceCatalogCache(IApiPortService service, IAnalyzerSettings settings, IDependencyFilter filter)
        {
            _settings = settings;
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _cache = new ConcurrentDictionary<string, HashSet<FrameworkName>>(StringComparer.Ordinal);
            _semaphore = new SemaphoreSlim(0, 1);
            _unknown = new ConcurrentStringHashSet();
            _cts = new CancellationTokenSource();
            _filter = filter;

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
            else if (string.Equals(e.PropertyName, nameof(IAnalyzerSettings.Platforms), StringComparison.Ordinal))
            {
                ImmutableInterlocked.InterlockedExchange(ref _currentNames, _settings.Platforms.ToImmutableArray());
            }
        }

        public void Dispose()
        {
            _settings.PropertyChanged -= SettingsPropertyChanged;

            _cts.Cancel();
            _semaphore.Dispose();
        }

        private bool Include(ISymbol symbol)
        {
            var assembly = symbol.ContainingAssembly.Identity;

            return _filter.IsFrameworkAssembly(assembly.Name, assembly.PublicKey);
        }

        public ApiStatus GetApiStatus(ISymbol symbol, out ImmutableArray<FrameworkName> unsupported)
        {
            unsupported = ImmutableArray.Create<FrameworkName>();

            if (!_settings.IsAutomaticAnalyze)
            {
                return ApiStatus.Off;
            }

            var api = symbol.GetDocumentationCommentId();

            if (_cache.TryGetValue(api, out var set))
            {
                unsupported = _currentNames;

                foreach (var version in _currentNames)
                {
                    if (set.Contains(version))
                    {
                        unsupported = unsupported.Remove(version);
                    }
                }

                if (unsupported.IsEmpty)
                {
                    return ApiStatus.Available;
                }

                return ApiStatus.Unavailable;
            }

            if (Include(symbol))
            {
                _unknown.Add(api);
                _semaphore.Release();
            }

            return ApiStatus.Unknown;
        }

        private async Task UpdateCatalogAsync()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    await _semaphore.WaitAsync(_cts.Token).ConfigureAwait(false);

                    if (_unknown.Count > 0)
                    {
                        var unknown = Interlocked.Exchange(ref _unknown, new ConcurrentStringHashSet());

                        try
                        {
                            var result = await _service.QueryDocIdsAsync(unknown).ConfigureAwait(false);

                            foreach (var api in result.Response)
                            {
                                _cache.TryAdd(api.Definition.DocId, new HashSet<FrameworkName>(api.Supported));
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
    }
}
