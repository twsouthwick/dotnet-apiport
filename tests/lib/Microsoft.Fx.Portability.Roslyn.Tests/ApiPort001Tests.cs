// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute;
using System.Collections.Immutable;
using System.Runtime.Versioning;
using TestHelper;
using Xunit;

namespace Microsoft.Fx.Portability.Roslyn.Test
{
    public class ApiPort001Tests : DiagnosticVerifier
    {
        private readonly FrameworkName _framework = new FrameworkName(".NET Core, Version=3.1");
        private readonly ICatalogCache _cache;

        public ApiPort001Tests()
        {
            _cache = Substitute.For<ICatalogCache>();
        }

        [Fact]
        public void TestMethod1()
        {
            var test = string.Empty;

            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            void T()
            {
                Console.WriteLine(string.Empty);
            }
        }
    }";
            var api = "M:System.Console.WriteLine(System.String)";

            var expected = new DiagnosticResult
            {
                Id = "ApiPort001",
                Message = $"The API '{api}' is not supported on {_framework}.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 15, 17)
                }
            };

            _cache.GetApiStatus(api, out _).Returns(x =>
            {
                x[1] = ImmutableArray.Create(_framework);
                return ApiStatus.Unavailable;
            });

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ApiPort001(_cache);
        }
    }
}
