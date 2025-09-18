using System.Diagnostics.CodeAnalysis;

namespace Payroc.LoadBalancer.Core.Exceptions;

[Serializable]
[ExcludeFromCodeCoverage]
public class NoAvailableServersException : LoadBalancerException
{
    public NoAvailableServersException(string message) : base(message)
    {}

    public NoAvailableServersException(string message, Exception innerException) : base(message, innerException)
    {}

    public NoAvailableServersException() : this(string.Empty)
    {}
}