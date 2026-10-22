using ContactBook.Core.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ContactBook.Tests
{
    [TestClass]
    public class ContactInputValidatorTests
    {
        [TestMethod]
        public void Validate_ValidContact_ReturnsTrue()
        {
            var (isValid, errorMessage) = ContactInputValidator.Validate("Иван", "+79991234567", "ivan@test.com");

            Assert.IsTrue(isValid);
            Assert.IsNull(errorMessage);
        }

        [TestMethod]
        public void Validate_EmptyFirstName_ReturnsFalse()
        {
            var (isValid, errorMessage) = ContactInputValidator.Validate("   ", "+79991234567", null);

            Assert.IsFalse(isValid);
            Assert.AreEqual("Имя контакта обязательно для заполнения.", errorMessage);
        }

        [TestMethod]
        public void Validate_EmptyPhone_ReturnsFalse()
        {
            var (isValid, errorMessage) = ContactInputValidator.Validate("Анна", "", null);

            Assert.IsFalse(isValid);
            Assert.AreEqual("Номер телефона обязателен для заполнения.", errorMessage);
        }

        [TestMethod]
        public void Validate_InvalidEmailFormat_ReturnsFalse()
        {
            var (isValid, errorMessage) = ContactInputValidator.Validate("Ольга", "123456", "invalid-email-address");

            Assert.IsFalse(isValid);
            Assert.AreEqual("Указан некорректный адрес электронной почты.", errorMessage);
        }

        [TestMethod]
        public void Validate_ShortPhoneNumber_ReturnsFalse()
        {
            var (isValid, errorMessage) = ContactInputValidator.Validate("Сергей", "123", null);

            Assert.IsFalse(isValid);
            Assert.AreEqual("Длина номера телефона должна быть от 5 до 30 символов.", errorMessage);
        }
    }
}
