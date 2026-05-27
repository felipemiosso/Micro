using Xunit;
using Micro.API.Infrastructure.CustomFields.Presets;

namespace Micro.Tests.Infrastructure.CustomFields;

public class PresetValidatorsTests
{
    [Fact]
    public void CpfValidator_ValidCpf_ReturnsSuccess()
    {
        // Arrange
        var cpf1 = "191.191.191-00";
        var cpf2 = "19119119100";

        // Act
        var result1 = CpfValidator.Validate(cpf1);
        var result2 = CpfValidator.Validate(cpf2);

        // Assert
        Assert.True(result1.IsValid);
        Assert.Null(result1.ErrorMessage);
        Assert.True(result2.IsValid);
        Assert.Null(result2.ErrorMessage);
    }

    [Fact]
    public void CpfValidator_InvalidCpf_ReturnsError()
    {
        // Arrange
        var emptyCpf = "";
        var shortCpf = "1234";
        var repeatedCpf = "111.111.111-11";
        var badChecksum = "19119119101";

        // Act
        var res1 = CpfValidator.Validate(emptyCpf);
        var res2 = CpfValidator.Validate(shortCpf);
        var res3 = CpfValidator.Validate(repeatedCpf);
        var res4 = CpfValidator.Validate(badChecksum);

        // Assert
        Assert.False(res1.IsValid);
        Assert.False(res2.IsValid);
        Assert.False(res3.IsValid);
        Assert.False(res4.IsValid);
    }

    [Fact]
    public void CnpjValidator_ValidCnpj_ReturnsSuccess()
    {
        // Arrange
        var cnpj1 = "00.000.000/0001-91";
        var cnpj2 = "00000000000191";

        // Act
        var result1 = CnpjValidator.Validate(cnpj1);
        var result2 = CnpjValidator.Validate(cnpj2);

        // Assert
        Assert.True(result1.IsValid);
        Assert.Null(result1.ErrorMessage);
        Assert.True(result2.IsValid);
        Assert.Null(result2.ErrorMessage);
    }

    [Fact]
    public void CnpjValidator_InvalidCnpj_ReturnsError()
    {
        // Arrange
        var emptyCnpj = "";
        var shortCnpj = "12345";
        var repeatedCnpj = "11111111111111";
        var badChecksum = "00000000000192";

        // Act
        var res1 = CnpjValidator.Validate(emptyCnpj);
        var res2 = CnpjValidator.Validate(shortCnpj);
        var res3 = CnpjValidator.Validate(repeatedCnpj);
        var res4 = CnpjValidator.Validate(badChecksum);

        // Assert
        Assert.False(res1.IsValid);
        Assert.False(res2.IsValid);
        Assert.False(res3.IsValid);
        Assert.False(res4.IsValid);
    }

    [Fact]
    public void PisValidator_ValidPis_ReturnsSuccess()
    {
        // Arrange
        var pis1 = "120.12012.01-0";
        var pis2 = "12012012010";

        // Act
        var result1 = PisValidator.Validate(pis1);
        var result2 = PisValidator.Validate(pis2);

        // Assert
        Assert.True(result1.IsValid);
        Assert.Null(result1.ErrorMessage);
        Assert.True(result2.IsValid);
        Assert.Null(result2.ErrorMessage);
    }

    [Fact]
    public void PisValidator_InvalidPis_ReturnsError()
    {
        // Arrange
        var emptyPis = "";
        var shortPis = "123";
        var repeatedPis = "11111111111";
        var badChecksum = "12012012011";

        // Act
        var res1 = PisValidator.Validate(emptyPis);
        var res2 = PisValidator.Validate(shortPis);
        var res3 = PisValidator.Validate(repeatedPis);
        var res4 = PisValidator.Validate(badChecksum);

        // Assert
        Assert.False(res1.IsValid);
        Assert.False(res2.IsValid);
        Assert.False(res3.IsValid);
        Assert.False(res4.IsValid);
    }

    [Fact]
    public void CepValidator_ValidCep_ReturnsSuccess()
    {
        // Arrange
        var cep1 = "01001-000";
        var cep2 = "01001000";

        // Act
        var res1 = CepValidator.Validate(cep1);
        var res2 = CepValidator.Validate(cep2);

        // Assert
        Assert.True(res1.IsValid);
        Assert.True(res2.IsValid);
    }

    [Fact]
    public void CepValidator_InvalidCep_ReturnsError()
    {
        // Arrange
        var emptyCep = "";
        var badFormat = "123-45";

        // Act
        var res1 = CepValidator.Validate(emptyCep);
        var res2 = CepValidator.Validate(badFormat);

        // Assert
        Assert.False(res1.IsValid);
        Assert.False(res2.IsValid);
    }

    [Fact]
    public void CnhValidator_ValidCnh_ReturnsSuccess()
    {
        // Arrange
        var cnh = "12345678901";

        // Act
        var res = CnhValidator.Validate(cnh);

        // Assert
        Assert.True(res.IsValid);
    }

    [Fact]
    public void CnhValidator_InvalidCnh_ReturnsError()
    {
        // Arrange
        var emptyCnh = "";
        var shortCnh = "123456789";
        var repeatedCnh = "11111111111";

        // Act
        var res1 = CnhValidator.Validate(emptyCnh);
        var res2 = CnhValidator.Validate(shortCnh);
        var res3 = CnhValidator.Validate(repeatedCnh);

        // Assert
        Assert.False(res1.IsValid);
        Assert.False(res2.IsValid);
        Assert.False(res3.IsValid);
    }

    [Fact]
    public void EmailValidator_ValidEmail_ReturnsSuccess()
    {
        // Arrange
        var email = "john.doe@example.com";

        // Act
        var res = EmailValidator.Validate(email);

        // Assert
        Assert.True(res.IsValid);
    }

    [Fact]
    public void EmailValidator_InvalidEmail_ReturnsError()
    {
        // Arrange
        var empty = "";
        var bad = "john.doe";

        // Act
        var res1 = EmailValidator.Validate(empty);
        var res2 = EmailValidator.Validate(bad);

        // Assert
        Assert.False(res1.IsValid);
        Assert.False(res2.IsValid);
    }

    [Fact]
    public void PhoneMobileValidator_ValidPhone_ReturnsSuccess()
    {
        // Arrange
        var phone = "(11) 98765-4321";

        // Act
        var res = PhoneMobileValidator.Validate(phone);

        // Assert
        Assert.True(res.IsValid);
    }

    [Fact]
    public void PhoneMobileValidator_InvalidPhone_ReturnsError()
    {
        // Arrange
        var empty = "";
        var bad = "12345";

        // Act
        var res1 = PhoneMobileValidator.Validate(empty);
        var res2 = PhoneMobileValidator.Validate(bad);

        // Assert
        Assert.False(res1.IsValid);
        Assert.False(res2.IsValid);
    }

    [Fact]
    public void PhoneLandlineValidator_ValidPhone_ReturnsSuccess()
    {
        // Arrange
        var phone = "(11) 4567-8901";

        // Act
        var res = PhoneLandlineValidator.Validate(phone);

        // Assert
        Assert.True(res.IsValid);
    }

    [Fact]
    public void PhoneLandlineValidator_InvalidPhone_ReturnsError()
    {
        // Arrange
        var empty = "";
        var bad = "12345";

        // Act
        var res1 = PhoneLandlineValidator.Validate(empty);
        var res2 = PhoneLandlineValidator.Validate(bad);

        // Assert
        Assert.False(res1.IsValid);
        Assert.False(res2.IsValid);
    }

    [Fact]
    public void UrlValidator_ValidUrl_ReturnsSuccess()
    {
        // Arrange
        var url1 = "http://google.com";
        var url2 = "https://example.org/path?q=1";

        // Act
        var res1 = UrlValidator.Validate(url1);
        var res2 = UrlValidator.Validate(url2);

        // Assert
        Assert.True(res1.IsValid);
        Assert.True(res2.IsValid);
    }

    [Fact]
    public void UrlValidator_InvalidUrl_ReturnsError()
    {
        // Arrange
        var empty = "";
        var bad = "not_a_url";

        // Act
        var res1 = UrlValidator.Validate(empty);
        var res2 = UrlValidator.Validate(bad);

        // Assert
        Assert.False(res1.IsValid);
        Assert.False(res2.IsValid);
    }
}
