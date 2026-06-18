namespace Network.Management.App.Services
{
    public interface IAppLogger
    {
        /// <summary>Logs an informational message to the console only.</summary>
        void Info(string actionType, string details);

        /// <summary>Logs a warning message to the console only.</summary>
        void Warning(string actionType, string details);

        /// <summary>Logs an error message to the console only.</summary>
        void Error(string actionType, string details, Exception? ex = null);

        /// <summary>Logs to the console and persists the entry to the database.</summary>
        Task PersistAsync(string actionType, string details, int userId);

        /// <summary>Logs to the console and persists a device-related entry to the database.</summary>
        Task PersistDeviceAsync(string actionType, string details, int userId, int deviceId);
    }
}
