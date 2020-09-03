// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Fx.Portability.Reports
{
    public class Text : Content
    {
        public Text(string content)
        {
            Content = content;
        }

        public string Content { get; }
    }
}
