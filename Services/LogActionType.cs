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

        public const string LocationCreated = "LOCATION_CREATED";
        public const string LocationUpdated = "LOCATION_UPDATED";
        public const string LocationDeleted = "LOCATION_DELETED";
    }
}
