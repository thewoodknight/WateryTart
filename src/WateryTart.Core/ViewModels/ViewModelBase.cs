using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using WateryTart.Core.Services;
using WateryTart.Core.Settings;
using WateryTart.MusicAssistant;

namespace WateryTart.Core.ViewModels
{
    public abstract class ViewModelBase<T> : ReactiveObject, IViewModelBase
    {
        internal ILogger<T> _logger;
        internal PlayersService? _playersService;
        internal ISettings? _settings;
        internal MusicAssistantClient _client;
        public IScreen HostScreen { get; set; }
        public virtual bool IsLoading { get; set; }
        public bool ShowMiniPlayer => true;
        public bool ShowNavigation => true;
        public virtual string Title { get; }
        public string UrlPathSegment => string.Empty;

#pragma warning disable CS8618
        public ViewModelBase(
            ILoggerFactory? loggerFactory = null,
            MusicAssistantClient? client = null)
#pragma warning restore CS8618
        {
            if (loggerFactory != null)
                _logger = CreateLogger(loggerFactory);
            if (client != null)
                _client = client;
        }

        internal ILogger<T> CreateLogger(ILoggerFactory loggerFactory)
        {
            return loggerFactory.CreateLogger<T>();
        }
    }
}
