using Dalamud.Plugin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Dalamud.Extensions.MicrosoftLogging
{
    public static class DalamudLoggerExtensions
    {
        public static ILoggingBuilder AddDalamudLogger(
            this ILoggingBuilder builder, IDalamudPlugin dalamudPlugin)
        {
            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider>(
                    new DalamudLoggerProvider(dalamudPlugin.GetType().Assembly)));
            return builder;
        }
    }
}
