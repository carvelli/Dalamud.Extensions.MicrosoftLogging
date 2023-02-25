using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Dalamud.Extensions.MicrosoftLogging
{
    public sealed class DalamudLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly Assembly _assembly;
        private IExternalScopeProvider? _scopeProvider;

        public DalamudLoggerProvider(Assembly assembly)
        {
            _assembly = assembly;
        }

        ILogger ILoggerProvider.CreateLogger(string categoryName)
            => CreateLoggerImpl(categoryName);

        /// <summary>
        /// Manual logger creation, doesn't handle scopes.
        /// </summary>
        public ILogger CreateLogger(Type type)
            => CreateLoggerImpl(type.FullName ?? type.ToString());

        /// <summary>
        /// Manual logger creation, doesn't handle scopes.
        /// </summary>
        public ILogger CreateLogger<T>()
            => CreateLoggerImpl(typeof(T).FullName ?? typeof(T).ToString());

        private ILogger CreateLoggerImpl(string categoryName)
            => new DalamudLogger(categoryName, _assembly, _scopeProvider);

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public void Dispose()
        {
        }
    }
}
