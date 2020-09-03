// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Microsoft.Fx.Portability.Reports.Generators;

namespace Microsoft.Fx.Portability.Reports
{
    public class ReportGeneratorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ReportGenerator>()
                .As<IReportGenerator2>()
                .SingleInstance();

            builder.RegisterType<SummaryPageGenerator>()
                .As<IPageGenerator>()
                .SingleInstance();

            builder.RegisterType<DetailsPageGenerator>()
                .As<IPageGenerator>()
                .SingleInstance();
        }
    }
}
