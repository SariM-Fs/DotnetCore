using BCrypt.Net;
using final_project_Core.Common;
using final_project_Core.Entities;
using final_project_Core.Interface;
using final_project_Core.Repositories;
using Microsoft.Extensions.Logging;

namespace final_project_Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDriverRepository _driverRepository;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IDriverRepository driverRepository, ITokenGenerator tokenGenerator, ILogger<AuthService> logger)
        {
            _driverRepository = driverRepository;
            _tokenGenerator = tokenGenerator;
            _logger = logger;
        }

        public async Task<OperationResult<(Driver Driver, string Token)>> RegisterAsync(
            string name, string email, string password, string licensePlate, CancellationToken ct)
        {
            var existing = await _driverRepository.GetByEmailAsync(email, ct);
            if (existing is not null)
            {
                return OperationResult<(Driver, string)>.Conflict("A driver with this email already exists.");
            }

            var driver = new Driver
            {
                Name = name,
                Email = email,
                LicensePlate = licensePlate,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "User"
            };

            await _driverRepository.AddAsync(driver, ct);
            await _driverRepository.SaveChangesAsync(ct);

            _logger.LogInformation("Driver {Email} registered", driver.Email);

            var token = _tokenGenerator.GenerateToken(driver);
            return OperationResult<(Driver, string)>.Success((driver, token));
        }

        public async Task<OperationResult<(Driver Driver, string Token)>> LoginAsync(string email, string password, CancellationToken ct)
        {
            var driver = await _driverRepository.GetByEmailAsync(email, ct);
            if (driver is null || !BCrypt.Net.BCrypt.Verify(password, driver.PasswordHash))
            {
                _logger.LogWarning("Login failed for {Email}", email);
                return OperationResult<(Driver, string)>.ValidationError("Invalid email or password.");
            }

            _logger.LogInformation("Driver {Email} logged in", driver.Email);

            var token = _tokenGenerator.GenerateToken(driver);
            return OperationResult<(Driver, string)>.Success((driver, token));
        }
    }
}
