namespace Network.Management.App.Services
{
    /// <summary>
    /// Helper for IPv4 parsing, formatting and validation. Subnet masks / CIDR suffixes
    /// are intentionally rejected — the network range is inferred from the configured range.
    /// </summary>
    public static class IpAddressHelper
    {
        public static bool IsValidIpv4(string? ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return false;

            // Reject CIDR / subnet mask suffixes (e.g. "192.168.1.10/24").
            if (ip.Contains('/')) return false;

            var parts = ip.Trim().Split('.');
            if (parts.Length != 4) return false;

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part) || !byte.TryParse(part, out _))
                    return false;
            }

            return true;
        }

        public static bool TryParse(string? ip, out uint value)
        {
            value = 0;
            if (!IsValidIpv4(ip)) return false;

            var parts = ip!.Trim().Split('.');
            value = ((uint)byte.Parse(parts[0]) << 24)
                  | ((uint)byte.Parse(parts[1]) << 16)
                  | ((uint)byte.Parse(parts[2]) << 8)
                  | byte.Parse(parts[3]);

            return true;
        }

        public static string ToIp(uint value) =>
            $"{(value >> 24) & 0xFF}.{(value >> 16) & 0xFF}.{(value >> 8) & 0xFF}.{value & 0xFF}";
    }
}
