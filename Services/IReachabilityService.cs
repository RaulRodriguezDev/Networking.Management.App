namespace Network.Management.App.Services
{
    public record ReachabilityResult(bool Reachable, long? LatencyMs);

    public interface IReachabilityService
    {
        /// <summary>Performs a real ICMP ping against the given IP address.</summary>
        Task<ReachabilityResult> PingAsync(string ipAddress, int timeoutMs = 2000);
    }
}
