namespace Network.Management.App.Services
{
    /// <summary>
    /// Resolves the hardware (MAC) address for an IPv4 host using the operating system's
    /// ARP resolution. Returns null when the address cannot be determined (host unreachable,
    /// not on the local segment, or ARP unavailable on the platform).
    /// </summary>
    public interface IMacAddressResolver
    {
        Task<string?> ResolveAsync(string ipAddress);
    }
}
