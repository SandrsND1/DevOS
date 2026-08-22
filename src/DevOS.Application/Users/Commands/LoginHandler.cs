using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;
using DevOS.Application.Users.DTOs;
using DevOS.Application.Validation;

namespace DevOS.Application.Users.Commands
{
    public class LoginHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly LoginValidator _validator;

        public LoginHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            LoginValidator validator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _validator = validator;
        }

        public async Task<AuthResponseDto> HandleAsync(
            LoginCommand command,
            CancellationToken cancellationToken = default)
        {
            var errors = _validator.Validate(command);
            if (errors.Count > 0)
                throw new ValidationException(errors);

            var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
            if (user == null || !_passwordHasher.Verify(command.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

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