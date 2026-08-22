using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;
using DevOS.Application.Users.DTOs;
using DevOS.Application.Validation;
using DevOS.Domain.Entities;

namespace DevOS.Application.Users.Commands
{
    public class RegisterUserHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly RegisterUserValidator _validator;

        public RegisterUserHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            RegisterUserValidator validator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _validator = validator;
        }

        public async Task<AuthResponseDto> HandleAsync(
            RegisterUserCommand command,
            CancellationToken cancellationToken = default)
        {
            var errors = _validator.Validate(command);
            if (errors.Count > 0)
                throw new ValidationException(errors);

            var existingUser = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
            if (existingUser != null)
                throw new ArgumentException("User with this email already exists.");

            if (!Enum.TryParse<UserRole>(command.Role, true, out var userRole))
                userRole = UserRole.Developer;

            var passwordHash = _passwordHasher.Hash(command.Password);
            var user = new User(command.Email, passwordHash, userRole);

            await _userRepository.AddAsync(user, cancellationToken);

            var token = _jwtTokenGenerator.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role.ToString()
            };
        }
    }
}