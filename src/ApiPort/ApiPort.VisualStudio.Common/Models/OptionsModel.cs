// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Resources;
using Microsoft.Fx.Portability;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;

namespace ApiPortVS.Models
{
    /// <summary>
    /// The options for ApiPort VS that are persisted.
    /// </summary>
    public class OptionsModel : NotifyPropertyBase, IAnalyzerSettings
    {
        private static readonly string OptionsFilePath;
        private static readonly string DefaultOutputDirectory;
        private static readonly string DefaultOutputFileName;

        private IList<SelectedResultFormat> _formats;
        private IList<TargetPlatform> _platforms;
        private string _outputDirectory;
        private string _outputName;
        private bool _isAutomaticAnalyze;

        static OptionsModel()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var directory = Path.GetDirectoryName(assembly.Location);

            DefaultOutputFileName = "ApiPortAnalysis";
            DefaultOutputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Portability Analysis");
            OptionsFilePath = Path.Combine(directory, "options.dat");
        }

        public OptionsModel()
        {
            _platforms = Array.Empty<TargetPlatform>();
            _formats = Array.Empty<SelectedResultFormat>();
            _outputName = DefaultOutputFileName;
            _outputDirectory = DefaultOutputDirectory;

            LastUpdate = DateTimeOffset.MinValue;
        }

        public DateTimeOffset LastUpdate { get; set; }

        public IList<SelectedResultFormat> Formats
        {
            get { return _formats; }
            set { UpdateProperty(ref _formats, value); }
        }

        public IList<TargetPlatform> Platforms
        {
            get { return _platforms; }
            set { UpdateProperty(ref _platforms, value.ToList()); }
        }

        public string OutputDirectory
        {
            get { return _outputDirectory; }
            set { UpdateProperty(ref _outputDirectory, string.IsNullOrWhiteSpace(value) ? _outputDirectory : value); }
        }

        public string DefaultOutputName
        {
            get { return _outputName; }
            set { UpdateProperty(ref _outputName, string.IsNullOrWhiteSpace(value) ? DefaultOutputFileName : value); }
        }

        public bool IsAutomaticAnalyze
        {
            get { return _isAutomaticAnalyze; }
            set { UpdateProperty(ref _isAutomaticAnalyze, value); }
        }

        public bool ShowAutomaticAnalyzeNotification { get; set; } = true;

        public void PlatformsUpdated() => OnPropertyUpdated(nameof(Platforms));

        public static OptionsModel Load()
        {
            try
            {
                if (File.Exists(OptionsFilePath))
                {
                    var bytes = File.ReadAllBytes(OptionsFilePath);

                    return bytes.Deserialize<OptionsModel>();
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }

            return new OptionsModel();
        }

        public bool Save()
        {
            try
            {
                File.WriteAllBytes(OptionsFilePath, this.Serialize());

                return true;
            }
            catch (IOException)
            {
                Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.UnableToSaveFileFormat, OptionsFilePath));

                return false;
            }
        }

        IEnumerable<FrameworkName> IAnalyzerSettings.Platforms
        {
            get
            {
                foreach (var target in Platforms)
                {
                    foreach (var version in target.Versions)
                    {
                        if (version.IsSelected)
                        {
                            yield return new FrameworkName(version.PlatformName, version.Version);
                        }
                    }
                }
            }
        }
    }
}
