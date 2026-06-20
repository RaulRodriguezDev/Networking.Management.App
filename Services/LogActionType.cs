namespace Network.Management.App.Services
{
    public static class LogActionType
    {
        public const string UserLogin = "USER_LOGIN";
        public const string UserLoginFailed = "USER_LOGIN_FAILED";
        public const string UserLogout = "USER_LOGOUT";
        public const string UserCreated = "USER_CREATED";
        public const string UserUpdated = "USER_UPDATED";
        public const string UserDeleted = "USER_DELETED";

        public const string DeviceCreated = "DEVICE_CREATED";
        public const string DeviceUpdated = "DEVICE_UPDATED";
        public const string DeviceDeleted = "DEVICE_DELETED";
        public const string DeviceStatusChanged = "DEVICE_STATUS_CHANGED";
        public const string DeviceProbed = "DEVICE_PROBED";
        public const string DeviceIpChanged = "DEVICE_IP_CHANGED";
        public const string DeviceVlanChanged = "DEVICE_VLAN_CHANGED";
        public const string DevicePortConfigured = "DEVICE_PORT_CONFIGURED";
        public const string DeviceRouteChanged = "DEVICE_ROUTE_CHANGED";

        public const string DeviceTypeCreated = "DEVICE_TYPE_CREATED";

        public const string VlanCreated = "VLAN_CREATED";
        public const string VlanUpdated = "VLAN_UPDATED";
        public const string VlanDeleted = "VLAN_DELETED";

        public const string LocationCreated = "LOCATION_CREATED";
        public const string LocationUpdated = "LOCATION_UPDATED";
        public const string LocationDeleted = "LOCATION_DELETED";

        public const string NetworkRangeChanged = "NETWORK_RANGE_CHANGED";
    }
}
