// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Fx.Portability.Reports
{
    public class Divider : Content
    {
        private Divider()
        {
        }

        internal static Divider Instance { get; } = new Divider();
    }
}
