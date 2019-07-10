// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Models;
using ApiPortVS.Reporting;
using ApiPortVS.Resources;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ApiPortVS
{
    internal class AutoAnalyzeNotifier : IVsInfoBarUIEvents
    {
        private readonly object _context = new object();
        private readonly OptionsModel _options;
        private readonly IVsInfoBarUIFactory _factory;
        private readonly IVsInfoBarHost _host;
        private readonly IResultToolbar _result;

        private uint _cookie;

        public AutoAnalyzeNotifier(OptionsModel options, IVsInfoBarUIFactory factory, IVsShell shell, IResultToolbar result)
        {
            _options = options;
            _factory = factory;
            shell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out var obj);
            _host = (IVsInfoBarHost)obj;
            _result = result;
        }

        public void Notify()
        {
            if (!_options.ShowAutomaticAnalyzeNotification)
            {
                return;
            }

            if (_host is null)
            {
                return;
            }

            var text = new InfoBarTextSpan(LocalizedStrings.AutomaticAnalysisNotification);
            var link = new InfoBarHyperlink(LocalizedStrings.OpenOptions, _context);

            var spans = new InfoBarTextSpan[] { text };
            var actions = new InfoBarActionItem[] { link };
            var model = new InfoBarModel(spans, actions);

            var element = _factory.CreateInfoBar(model);

            element.Advise(this, out _cookie);

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                _host.AddInfoBar(element);
            });
        }

        public void OnClosed(IVsInfoBarUIElement infoBarUIElement) => Update(infoBarUIElement);

        public void OnActionItemClicked(IVsInfoBarUIElement infoBarUIElement, IVsInfoBarActionItem actionItem)
        {
            if (actionItem.ActionContext == _context)
            {
                _result.ShowOptionsPage();
            }

            Update(infoBarUIElement);
        }

        private void Update(IVsInfoBarUIElement element)
        {
            element.Unadvise(_cookie);
            _host.RemoveInfoBar(element);
            _options.ShowAutomaticAnalyzeNotification = false;
            _options.Save();
        }
    }
}
