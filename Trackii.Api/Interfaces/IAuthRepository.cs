using Trackii.Api.Models;

namespace Trackii.Api.Interfaces;

public interface IAuthRepository
{
    Task<bool> TokenExistsAsync(string tokenCode, CancellationToken cancellationToken);
    Task<bool> LocationExistsAsync(uint locationId, CancellationToken cancellationToken);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken);
    Task<User?> GetActiveUserByUsernameAsync(string username, CancellationToken cancellationToken);
    Task<Device?> GetDeviceByUidAsync(string deviceUid, CancellationToken cancellationToken);
    Task<Device?> GetActiveDeviceByUidForUserAsync(string deviceUid, uint userId, CancellationToken cancellationToken);
    void AddUser(User user);
    void AddDevice(Device device);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
