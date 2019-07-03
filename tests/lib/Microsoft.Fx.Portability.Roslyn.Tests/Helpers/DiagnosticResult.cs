// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using System;

namespace TestHelper
{
    /// <summary>
    /// Struct that stores information about a Diagnostic appearing in a source.
    /// </summary>
    public struct DiagnosticResult
    {
        private DiagnosticResultLocation[] locations;

#pragma warning disable CA1819 // Properties should not return arrays
        public DiagnosticResultLocation[] Locations
#pragma warning restore CA1819 // Properties should not return arrays
        {
            get
            {
                if (locations == null)
                {
                    locations = Array.Empty<DiagnosticResultLocation>();
                }

                return locations;
            }

            set
            {
                locations = value;
            }
        }

        public DiagnosticSeverity Severity { get; set; }

        public string Id { get; set; }

        public string Message { get; set; }

        public string Path => Locations.Length > 0 ? Locations[0].Path : string.Empty;

        public int Line => Locations.Length > 0 ? Locations[0].Line : -1;

        public int Column => Locations.Length > 0 ? Locations[0].Column : -1;
    }
}
