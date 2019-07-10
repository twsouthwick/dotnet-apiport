// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;

namespace ApiPortVS
{
    public static class ApiPortThreadHelper
    {
        public static JoinableTaskFactory JoinableTaskFactory => _factory.Value;

        private static Lazy<JoinableTaskFactory> _factory = new Lazy<JoinableTaskFactory>(() =>
        {
            try
            {
                return ThreadHelper.JoinableTaskFactory;
            }

            // If run in a non-Visual Studio instance, this results in a null reference. If this is the case,
            // we'll just create a new context as described at https://github.com/Microsoft/vs-threading/blob/master/doc/testing_vs.md
            catch (NullReferenceException)
            {
                var jtc = new JoinableTaskContext();
                return jtc.Factory;
            }
        });
    }
}
