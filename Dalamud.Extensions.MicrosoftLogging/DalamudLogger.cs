using System.Reflection;
using Microsoft.Extensions.Logging;
using System.Text;
using Serilog.Events;

namespace Dalamud.Extensions.MicrosoftLogging
{
    public sealed class DalamudLogger : ILogger
    {
        private readonly string _name;
        private readonly string _assemblyName;
        private readonly IExternalScopeProvider? _scopeProvider;
        private readonly Serilog.ILogger _pluginLogDelegate;

        public DalamudLogger(string name, Assembly assembly, IExternalScopeProvider? scopeProvider)
        {
            _name = name;
            _assemblyName = assembly.GetName().Name ??
                            typeof(DalamudLogger).Assembly.GetName().Name ??
                            "{unknown DalamudLogger}";
            _scopeProvider = scopeProvider;
            _pluginLogDelegate =
                Serilog.Log.ForContext("SourceContext", _assemblyName);
        }

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
            => _scopeProvider?.Push(state) ?? NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None && IsEnabled(ToSerilogLevel(logLevel));

        private bool IsEnabled(LogEventLevel logEventLevel) => _pluginLogDelegate.IsEnabled(logEventLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.None)
                return;

            LogEventLevel logEventLevel = ToSerilogLevel(logLevel);
            if (!IsEnabled(logEventLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            StringBuilder sb = new StringBuilder();
            sb.Append('[').Append(_assemblyName).Append("] ");
            _scopeProvider?.ForEachScope((scope, builder) =>
                {
                    if (scope is IEnumerable<KeyValuePair<string, object>> properties)
                    {
                        foreach (KeyValuePair<string, object> pair in properties)
                        {
                            builder.Append('<').Append(pair.Key).Append('=').Append(pair.Value)
                                .Append("> ");
                        }
                    }
                    else if (scope != null)
                        builder.Append('<').Append(scope).Append("> ");
                },
                sb);
            sb.Append(_name).Append(": ").Append(formatter(state, null));
            _pluginLogDelegate.Write(logEventLevel, exception, sb.ToString());
        }

        private LogEventLevel ToSerilogLevel(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Critical => LogEventLevel.Fatal,
                LogLevel.Error => LogEventLevel.Error,
                LogLevel.Warning => LogEventLevel.Warning,
                LogLevel.Information => LogEventLevel.Information,
                LogLevel.Debug => LogEventLevel.Debug,
                LogLevel.Trace => LogEventLevel.Verbose,
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
            };
        }

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();

            private NullScope()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
