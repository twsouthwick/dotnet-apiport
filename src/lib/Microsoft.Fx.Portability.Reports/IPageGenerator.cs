// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;

namespace Microsoft.Fx.Portability.Reports
{
    public interface IPageGenerator
    {
        Page GeneratePage(AnalyzeResponse response);
    }
}
