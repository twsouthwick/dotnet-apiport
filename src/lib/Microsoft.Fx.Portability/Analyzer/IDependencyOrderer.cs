﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability
{
    public interface IDependencyOrderer
    {
        IEnumerable<string> GetOrder(string entryPoint, IEnumerable<AssemblyInfo> assemblies);
    }
}
