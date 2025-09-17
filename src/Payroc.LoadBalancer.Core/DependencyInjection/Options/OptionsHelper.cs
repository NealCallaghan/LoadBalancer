using System.Net;

namespace Payroc.LoadBalancer.Core.DependencyInjection.Options;

public static class OptionsHelper
{
    public static (IPAddress, int) ValidateOptionsOrThrow(ServerLocation options)
    {
        var addressIsValid = IPAddress.TryParse(options.Address, out var address);
        if (!addressIsValid || address is null)
        {
            throw new ArgumentException($"{nameof(ServerLocation.Address)} must be a valid address");
        }
        
        return (address!, options.Port);
    }
}