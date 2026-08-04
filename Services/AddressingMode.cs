namespace Network.Management.App.Services
{
    /// <summary>
    /// How a device obtains its IP address. Static addresses are configured on the device
    /// itself; DHCP addresses are handed out by the router and pinned via a reservation
    /// keyed on the device's MAC address.
    /// </summary>
    public static class AddressingMode
    {
        public const string Static = "Static";
        public const string Dhcp = "Dhcp";

        public static bool IsValid(string? mode) =>
            mode is Static or Dhcp;

        public static string Normalize(string? mode) =>
            IsValid(mode) ? mode! : Static;
    }
}
