﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.Resources;
using Microsoft.Fx.Portability;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ApiPort
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var productInformation = new ProductInformation("ApiPort_Console", typeof(Program));

            Console.WriteLine(LocalizedStrings.Header, LocalizedStrings.ApplicationName, productInformation.Version);

            var options = CommandLineOptions.ParseCommandLineOptions(args);

            if (options.Command == AppCommands.Exit)
            {
                return -1;
            }

            Console.WriteLine();

            using (var container = DependencyBuilder.Build(options, productInformation))
            {
                var progressReport = container.Resolve<IProgressReporter>();

                try
                {
                    var client = container.Resolve<ConsoleApiPort>();

                    switch (options.Command)
                    {
                        case AppCommands.ListTargets:
                            client.ListTargetsAsync().Wait();
                            break;
                        case AppCommands.AnalyzeAssemblies:
                            client.AnalyzeAssembliesAsync().Wait();
                            break;
                        case AppCommands.DocIdSearch:
                            client.RunDocIdSearchAsync().Wait();
                            break;
                        case AppCommands.ListOutputFormats:
                            client.ListOutputFormatsAsync().Wait();
                            break;
                    }

                    return 0;
                }
                catch (PortabilityAnalyzerException ex)
                {
                    Trace.TraceError(ex.ToString());

                    // Display the message as it has already been localized
                    WriteError(ex.Message);
                }
                catch (AggregateException ex)
                {
                    Trace.TraceError(ex.ToString());

                    // If the exception is known, display the message as it has already been localized
                    if (GetRecursiveInnerExceptions(ex).Any(x => x is PortabilityAnalyzerException))
                    {
                        foreach (PortabilityAnalyzerException portEx in GetRecursiveInnerExceptions(ex).Where(x => x is PortabilityAnalyzerException))
                        {
                            WriteError(portEx.Message);
                        }
                    }
                    else if (!IsWebSecurityFailureOnMono(ex))
                    {
                        WriteError(LocalizedStrings.UnknownException);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());

                    WriteError(LocalizedStrings.UnknownException);
                }
                finally
                {
                    if (progressReport != null)
                    {
                        Console.WriteLine();

                        foreach (var issue in progressReport.Issues)
                        {
                            WriteWarning("* " + issue);
                        }
                    }
                }

                return -1;
            }
        }

        private static IEnumerable<Exception> GetRecursiveInnerExceptions(Exception ex)
        {
            if (ex is AggregateException) // AggregateExceptions can have multiple inner exceptions
            {
                foreach (var innerEx in (ex as AggregateException).InnerExceptions)
                {
                    yield return innerEx;
                    foreach (var innerInnerEx in GetRecursiveInnerExceptions(innerEx))
                    {
                        yield return innerInnerEx;
                    }

                }
            }
            else // Other exceptions can have only one inner exception
            {
                if (ex.InnerException != null)
                {
                    yield return ex.InnerException;
                    foreach (var innerInnerEx in GetRecursiveInnerExceptions(ex.InnerException))
                    {
                        yield return innerInnerEx;
                    }
                }
            }
        }

        public static void WriteColorLine(string message, ConsoleColor color)
        {
            var previousColor = Console.ForegroundColor;

            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = previousColor;
            }
        }

        private static void WriteError(string message)
        {
            Console.WriteLine();
            WriteColorLine(message, ConsoleColor.Red);
        }

        private static void WriteWarning(string message)
        {
            WriteColorLine(message, ConsoleColor.Yellow);
        }

        /// <summary>
        /// Mono does not come installed with root certificates.  If a user runs this without configuring them,
        /// they will receive a Mono.Security.Protocol.Tls.TlsException.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private static bool IsWebSecurityFailureOnMono(Exception ex)
        {
            if (ex.InnerException is System.Net.WebException && ex.InnerException.InnerException is System.IO.IOException && ex.InnerException.InnerException.InnerException != null)
            {
                var errorType = ex.InnerException.InnerException.InnerException.GetType();

                if (String.Equals(errorType.FullName, "Mono.Security.Protocol.Tls.TlsException", StringComparison.Ordinal))
                {
                    Console.WriteLine(LocalizedStrings.MonoWebRequestsFailure);

                    return true;
                }
            }

            return false;
        }
    }
}
