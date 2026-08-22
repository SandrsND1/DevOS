using DevOS.Application.Users.Commands;

namespace DevOS.Application.Users
{
    public class RegisterUserValidator
    {
        public List<string> Validate(RegisterUserCommand command)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(command.Email))
                errors.Add("Email is required.");
            else if (!command.Email.Contains('@'))
                errors.Add("Invalid email format.");

            if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 6)
                errors.Add("Password must be at least 6 characters long.");

            return errors;
        }
    }
}