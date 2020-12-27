﻿using Cfg.Net.Contracts;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Pipeline.Web.Orchard.Impl {
    public class CfgNetNotifier : ILogger {

        private readonly INotifier _notifier;
        public Localizer T { get; set; }

        public CfgNetNotifier(INotifier notifier) {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public void Warn(string message, params object[] args) {
            _notifier.Warning(T(message, args));
        }

        public void Error(string message, params object[] args) {
            _notifier.Error(T(message, args));
        }
    }
}