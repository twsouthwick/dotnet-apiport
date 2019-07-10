// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Microsoft.Fx.Portability
{
    public interface IAnalyzerSettings : INotifyPropertyChanged
    {
        bool IsAutomaticAnalyze { get; }
    }
}
