using System.Diagnostics.CodeAnalysis;

namespace Payroc.LoadBalancer.Core.Exceptions;

/// <summary>
/// A generic exception identifying all load balancer exceptions from Exception
/// </summary>
[Serializable]
[ExcludeFromCodeCoverage]
public abstract class LoadBalancerException : Exception
{
    protected LoadBalancerException(string message) : base(message)
    {}

    protected LoadBalancerException(string message, Exception innerException) : base(message, innerException)
    {}

    protected LoadBalancerException() : this(string.Empty)
    {}
}