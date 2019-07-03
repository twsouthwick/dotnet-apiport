// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.Roslyn
{
    public interface ICatalogCache
    {
        FrameworkName Framework { get; }

        ApiStatus GetApiStatus(string api);

        void UpdateCatalog();
    }
}
