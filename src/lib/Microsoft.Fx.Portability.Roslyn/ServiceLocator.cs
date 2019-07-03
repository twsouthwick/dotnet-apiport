// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Fx.Portability.Roslyn
{
    public static class ServiceLocator
    {
        internal static ICatalogCache Cache { get; private set; }

        public static void SetCache(ICatalogCache value) => Cache = value ?? throw new ArgumentNullException(nameof(value));
    }
}
