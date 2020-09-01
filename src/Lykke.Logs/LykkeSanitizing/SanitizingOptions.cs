using System.Collections.Generic;

namespace Lykke.Logs.LykkeSanitizing
{
    internal sealed class SanitizingOptions
    {
        public ICollection<SanitizingFilter> Filters { get; } = new List<SanitizingFilter>();
    }
}