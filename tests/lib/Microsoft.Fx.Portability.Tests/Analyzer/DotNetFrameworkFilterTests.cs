﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer;
using Xunit;

namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    public class DotNetFrameworkFilterTests
    {
        private readonly IDependencyFilter _assemblyFilter = new DotNetFrameworkFilter();

        [Fact]
        public void NullIsTrue()
        {
            Assert.True(_assemblyFilter.IsFrameworkAssembly(null, default));
        }

        // Microsoft public key token
        [InlineData("b77a5c561934e089", true)]
        [InlineData("b03f5f7f11d50a3a", true)]
        [InlineData("7cec85d7bea7798e", true)]
        [InlineData("31bf3856ad364e35", true)]
        [InlineData("24eec0d8c86cda1e", true)]
        [InlineData("0738eb9f132ed756", true)]

        // Microsoft public key token (different case)
        [InlineData("0738eb9F132ed756", true)]

        // Non-Microsoft public key token
        [InlineData("1111111111111111", false)]
        [Theory]
        public void DotNetFrameworkFilterCheckPublicKeyToken(string publicKeyToken, bool succeed)
        {
            Assert.Equal(succeed, _assemblyFilter.IsFrameworkAssembly(string.Empty, PublicKeyToken.Parse(publicKeyToken)));
        }

        // Invalid characters
        [InlineData("something")]

        // Invalid length
        [InlineData("111")]
        [Theory]
        public void InvalidPublicKeyToken(string publicKeyToken)
        {
            Assert.Throws<PortabilityAnalyzerException>(() => _assemblyFilter.IsFrameworkAssembly(string.Empty, PublicKeyToken.Parse(publicKeyToken)));
        }

        [InlineData("System.something", true)]
        [InlineData("system.something", true)]
        [InlineData("Microsoft.something", false)]
        [InlineData("microsoft.something", false)]
        [InlineData("Microsoft.AspNetCore.Http", true)]
        [InlineData("microsoft.aspnetcore.http", true)]
        [InlineData("Microsoft.Win32.VisualBasic", true)]
        [InlineData("mono.something", false)]
        [InlineData("Mono.something", false)]
        [InlineData("mscorlib", true)]
        [InlineData("msCorlib", true)]
        [InlineData("mscorlib.something", false)]
        [InlineData("something.else", false)]
        [Theory]
        public void AssemblyNameStartsWithSpecifiedString(string name, bool succeed)
        {
            Assert.Equal(succeed, _assemblyFilter.IsFrameworkAssembly(name, default));
        }
    }
}
