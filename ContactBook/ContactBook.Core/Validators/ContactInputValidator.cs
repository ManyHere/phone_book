using System.Text.RegularExpressions;

namespace ContactBook.Core.Validators
{
    public static partial class ContactInputValidator
    {
        [GeneratedRegex(@"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{2,6}$", RegexOptions.Compiled)]
        private static partial Regex PhoneRegex();

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
        private static partial Regex EmailRegex();

        public static (bool IsValid, string? ErrorMessage) Validate(string firstName, string phoneNumber, string? email)
        {
            if (string.IsNullOrWhiteSpace(firstName))
            {
                return (false, "Имя контакта обязательно для заполнения.");
            }

            if (firstName.Trim().Length > 100)
            {
                return (false, "Имя не может превышать 100 символов.");
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return (false, "Номер телефона обязателен для заполнения.");
            }

            var cleanPhone = phoneNumber.Trim();
            if (cleanPhone.Length < 5 || cleanPhone.Length > 30)
            {
                return (false, "Длина номера телефона должна быть от 5 до 30 символов.");
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var cleanEmail = email.Trim();
                if (!EmailRegex().IsMatch(cleanEmail))
                {
                    return (false, "Указан некорректный адрес электронной почты.");
                }
            }

            return (true, null);
        }
    }
}
