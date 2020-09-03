// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Fx.Portability.Reports
{
    public abstract class PageVisitor
    {
        public abstract void Visit(Table table);

        public abstract void Visit(Divider divider);

        public abstract void Visit(Text text);

        public void Visit(Page page)
        {
            var first = true;

            foreach (var content in page.Content)
            {
                if (!first)
                {
                    Visit(Divider.Instance);
                }
                else
                {
                    first = false;
                }

                if (content is Table table)
                {
                    Visit(table);
                }
                else if (content is Text text)
                {
                    Visit(text);
                }
            }
        }
    }
}
