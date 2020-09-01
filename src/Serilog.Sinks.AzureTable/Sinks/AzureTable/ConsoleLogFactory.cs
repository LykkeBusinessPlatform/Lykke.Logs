using Common.Log;
using Lykke.Common.Log;
using Microsoft.Extensions.Logging;

namespace Serilog.Sinks.AzureTable.Sinks.AzureTable
{
    internal class ConsoleLogFactory : ILogFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILog CreateLog<TComponent>(TComponent component, string componentNameSuffix)
        {
            return new ConsoleLog();
        }

        public ILog CreateLog<TComponent>(TComponent component)
        {
            return new ConsoleLog();
        }

        public void Dispose()
        {
        }
    }
}
