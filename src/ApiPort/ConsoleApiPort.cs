﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.Resources;
using Microsoft.Fx.Portability;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPort
{
    internal class ConsoleApiPort
    {
        private readonly ApiPortClient _apiPortClient;
        private readonly ITargetMapper _targetMapper;
        private readonly IApiPortOptions _options;
        private readonly DocIdSearchRepl _repl;

        public ConsoleApiPort(ApiPortClient apiPortClient, ITargetMapper targetMapper, IApiPortOptions options, DocIdSearchRepl repl)
        {
            _apiPortClient = apiPortClient;
            _targetMapper = targetMapper;
            _options = options;
            _repl = repl;
        }

        public async Task ListOutputFormatsAsync()
        {
            var outputFormats = await _apiPortClient.GetResultFormatsAsync();

            if (outputFormats.Any())
            {
                Console.WriteLine();
                Console.WriteLine(LocalizedStrings.AvailableOutputFormats);

                foreach (var outputFormat in outputFormats)
                {
                    Console.WriteLine(string.Format(LocalizedStrings.TargetsListNoVersion, outputFormat));
                }
            }
        }

        public async Task ListTargetsAsync()
        {
            const string SelectedMarker = "*";

            var targets = await _apiPortClient.GetTargetsAsync();

            if (targets.Any())
            {
                Console.WriteLine();
                Console.WriteLine(LocalizedStrings.AvailableTargets);

                var expandableTargets = targets.Where(target => target.ExpandedTargets.Any());
                var groupedTargets = targets.Where(target => !target.ExpandedTargets.Any()).GroupBy(target => target.Name);

                foreach (var item in groupedTargets)
                {
                    Console.WriteLine(LocalizedStrings.TargetsList, item.Key, String.Join(LocalizedStrings.VersionListJoin, item.Select(v => v.Version.ToString() + (v.IsSet ? SelectedMarker : String.Empty))));
                }

                if (expandableTargets.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine(Microsoft.Fx.Portability.Resources.LocalizedStrings.AvailableGroupedTargets);

                    foreach (var item in expandableTargets)
                    {
                        Console.WriteLine(LocalizedStrings.TargetsListGrouped, item.Name, String.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ", item.ExpandedTargets));
                    }
                }
            }

            if (_targetMapper.Aliases.Any())
            {
                Console.WriteLine();
                Console.WriteLine(LocalizedStrings.AvailableAliases);

                foreach (var alias in _targetMapper.Aliases)
                {
                    Console.WriteLine(LocalizedStrings.TargetsListNoVersion, alias);
                }
            }

            Console.WriteLine();
            Console.WriteLine(LocalizedStrings.NotesOnUsage);
            Console.WriteLine(LocalizedStrings.TargetsListNoVersion, Microsoft.Fx.Portability.Resources.LocalizedStrings.HowToSpecifyVersion);
            Console.WriteLine();
            Console.WriteLine(LocalizedStrings.TargetsListNoVersion, LocalizedStrings.WhatAsteriskMeans);
        }

        public async Task AnalyzeAssembliesAsync()
        {
            var outputPaths = await _apiPortClient.WriteAnalysisReportsAsync(_options);

            Console.WriteLine();
            Console.WriteLine(LocalizedStrings.OutputWrittenTo);

            foreach (var outputPath in outputPaths)
            {
                Console.WriteLine(outputPath);
            }
        }

        public Task RunDocIdSearchAsync() => _repl.DocIdSearchAsync();
    }
}
