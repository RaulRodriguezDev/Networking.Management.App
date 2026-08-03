using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Network.Management.App.Services
{
    /// <summary>
    /// Resolves MAC addresses via ARP. On Windows it calls the native SendARP (iphlpapi),
    /// which actively resolves the entry; on other platforms it falls back to reading the
    /// system ARP cache (populated by a prior ping) with the `arp` command.
    /// MAC resolution only works for hosts on the same local network segment as the server.
    /// </summary>
    public partial class MacAddressResolver : IMacAddressResolver
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int destIp, int srcIp, byte[] macAddr, ref int macAddrLen);

        public Task<string?> ResolveAsync(string ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress?.Trim(), out var ip)
                || ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                return Task.FromResult<string?>(null);

            var mac = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? ResolveWithSendArp(ip)
                : ResolveFromArpCommand(ip.ToString());

            return Task.FromResult(mac);
        }

        private static string? ResolveWithSendArp(IPAddress ip)
        {
            try
            {
                var dest = BitConverter.ToInt32(ip.GetAddressBytes(), 0);
                var buffer = new byte[6];
                var length = buffer.Length;

                if (SendARP(dest, 0, buffer, ref length) != 0 || length == 0)
                    return null;

                return FormatMac(buffer, length);
            }
            catch (DllNotFoundException) { return null; }
            catch (EntryPointNotFoundException) { return null; }
        }

        private static string? ResolveFromArpCommand(string ip)
        {
            try
            {
                var psi = new ProcessStartInfo("arp", "-a")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process is null) return null;

                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(2000);

                foreach (var line in output.Split('\n'))
                {
                    if (!line.Contains(ip)) continue;

                    var match = MacRegex().Match(line);
                    if (match.Success)
                        return NormalizeMac(match.Value);
                }

                return null;
            }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
            {
                return null;
            }
        }

        private static string FormatMac(byte[] buffer, int length) =>
            string.Join(":", buffer.Take(length).Select(b => b.ToString("X2")));

        private static string NormalizeMac(string raw) =>
            raw.Replace('-', ':').ToUpperInvariant();

        [GeneratedRegex("([0-9A-Fa-f]{2}[:-]){5}[0-9A-Fa-f]{2}")]
        private static partial Regex MacRegex();
    }
}
