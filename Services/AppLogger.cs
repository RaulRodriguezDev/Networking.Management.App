using Network.Management.App.Repository;
using Network.Management.App.Repository.Models;

namespace Network.Management.App.Services
{
    public class AppLogger : IAppLogger
    {
        private readonly ILogger<AppLogger> _logger;
        private readonly IDbRepository _repository;

        public AppLogger(ILogger<AppLogger> logger, IDbRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public void Info(string actionType, string details)
        {
            _logger.LogInformation("[{ActionType}] {Details}", actionType, details);
        }

        public void Warning(string actionType, string details)
        {
            _logger.LogWarning("[{ActionType}] {Details}", actionType, details);
        }

        public void Error(string actionType, string details, Exception? ex = null)
        {
            _logger.LogError(ex, "[{ActionType}] {Details}", actionType, details);
        }

        public async Task PersistAsync(string actionType, string details, int userId)
        {
            _logger.LogInformation("[{ActionType}] (persisted) {Details}", actionType, details);

            await _repository.AddAsync(new Log
            {
                ActionType = actionType,
                Details = details,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            });
        }

        public async Task PersistDeviceAsync(string actionType, string details, int userId, int deviceId)
        {
            _logger.LogInformation("[{ActionType}] (device:{DeviceId}) {Details}", actionType, deviceId, details);

            await _repository.AddAsync(new Log
            {
                ActionType = actionType,
                Details = details,
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                DeviceId = deviceId
            });
        }
    }
}
