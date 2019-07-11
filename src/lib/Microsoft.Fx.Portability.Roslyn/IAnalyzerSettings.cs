// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability
{
    public interface IAnalyzerSettings : INotifyPropertyChanged
    {
        bool IsAutomaticAnalyze { get; }

        IEnumerable<FrameworkName> Platforms { get; }
    }
}
