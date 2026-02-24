using Microsoft.EntityFrameworkCore;
using Trackii.Api.Data;
using Trackii.Api.Interfaces;
using Trackii.Api.Models;

namespace Trackii.Api.Repositories;

public sealed class AuthRepository : IAuthRepository
{
    private readonly TrackiiDbContext _dbContext;

    public AuthRepository(TrackiiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> TokenExistsAsync(string tokenCode, CancellationToken cancellationToken) =>
        _dbContext.Tokens.AnyAsync(token => token.Code == tokenCode, cancellationToken);

    public Task<bool> LocationExistsAsync(uint locationId, CancellationToken cancellationToken) =>
        _dbContext.Locations.AnyAsync(location => location.Id == locationId && location.Active, cancellationToken);

    public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken) =>
        _dbContext.Users.AnyAsync(user => user.Username == username, cancellationToken);

    public Task<User?> GetActiveUserByUsernameAsync(string username, CancellationToken cancellationToken) =>
        _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username && u.Active, cancellationToken);

    public Task<Device?> GetDeviceByUidAsync(string deviceUid, CancellationToken cancellationToken) =>
        _dbContext.Devices.FirstOrDefaultAsync(d => d.DeviceUid == deviceUid, cancellationToken);

    public Task<Device?> GetActiveDeviceByUidForUserAsync(string deviceUid, uint userId, CancellationToken cancellationToken) =>
        _dbContext.Devices
            .Include(d => d.Location)
            .FirstOrDefaultAsync(d => d.DeviceUid == deviceUid && d.UserId == userId && d.Active, cancellationToken);

    public void AddUser(User user) => _dbContext.Users.Add(user);

    public void AddDevice(Device device) => _dbContext.Devices.Add(device);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => _dbContext.SaveChangesAsync(cancellationToken);
}
