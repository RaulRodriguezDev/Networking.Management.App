using System.Net.NetworkInformation;

namespace Network.Management.App.Services
{
    public class ReachabilityService : IReachabilityService
    {
        public async Task<ReachabilityResult> PingAsync(string ipAddress, int timeoutMs = 2000)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ipAddress.Trim(), timeoutMs);

                return reply.Status == IPStatus.Success
                    ? new ReachabilityResult(true, reply.RoundtripTime)
                    : new ReachabilityResult(false, null);
            }
            catch (PingException)
            {
                // Invalid host / DNS failure / network down: treat as unreachable.
                return new ReachabilityResult(false, null);
            }
            catch (InvalidOperationException)
            {
                return new ReachabilityResult(false, null);
            }
        }
    }
}
