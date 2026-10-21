using System.Net;

namespace Api.Security;

/// <summary>
/// Loopback and private ranges: the host and the Docker network (nginx, the Next.js
/// server, n8n). In production the API port is published on 127.0.0.1 only, so every
/// legitimate caller that is not a visitor comes from one of these.
/// </summary>
public static class InternalNetworks
{
    public static readonly IReadOnlyList<IPNetwork> All =
    [
        IPNetwork.Parse("127.0.0.0/8"),
        IPNetwork.Parse("10.0.0.0/8"),
        IPNetwork.Parse("172.16.0.0/12"),
        IPNetwork.Parse("192.168.0.0/16"),
        IPNetwork.Parse("::1/128"),
        IPNetwork.Parse("fc00::/7"),
    ];

    public static bool Contains(IPAddress address)
    {
        // Kestrel listens dual-stack, so IPv4 peers show up as ::ffff:a.b.c.d.
        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        return All.Any(network => network.Contains(address));
    }
}
