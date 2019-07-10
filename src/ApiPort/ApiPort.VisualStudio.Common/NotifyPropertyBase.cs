// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ApiPortVS
{
    public class NotifyPropertyBase : INotifyPropertyChanged
    {
        private static bool _switchThread = IsInVs();

        public event PropertyChangedEventHandler PropertyChanged;

        public NotifyPropertyBase()
        {
        }

        protected void UpdateProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;

                OnPropertyUpdated(propertyName);
            }
        }

        protected async void OnPropertyUpdated([CallerMemberName]string propertyName = "")
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged == null)
            {
                return;
            }

            if (_switchThread)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            }

            propertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static bool IsInVs()
        {
            try
            {
                return ThreadHelper.JoinableTaskFactory != null;
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }
    }
}
