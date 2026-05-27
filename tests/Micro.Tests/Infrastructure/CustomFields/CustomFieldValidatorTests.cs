using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Micro.API.Data.Models;
using Micro.API.Infrastructure.CustomFields;

namespace Micro.Tests.Infrastructure.CustomFields;

public class CustomFieldValidatorTests
{
    [Fact]
    public void Validate_RequiredField_MissingValue_ReturnsError()
    {
        // Arrange
        var def = new CustomFieldDefinition
        {
            Label = "Test Field",
            FieldType = CustomFieldType.ShortText,
            IsRequired = true
        };

        // Act
        var errors = CustomFieldValidator.Validate(def, "");

        // Assert
        Assert.Single(errors);
        Assert.Contains("'Test Field' is required", errors.First());
    }

    [Fact]
    public void Validate_ShortText_LengthCheck_ReturnsSuccessAndErrors()
    {
        // Arrange
        var def = new CustomFieldDefinition
        {
            Label = "About",
            FieldType = CustomFieldType.ShortText,
            ValidationJson = "{\"MinLength\":5,\"MaxLength\":10}"
        };

        // Act & Assert (Length OK)
        var errorsOk = CustomFieldValidator.Validate(def, "Hello");
        Assert.Empty(errorsOk);

        // Act & Assert (Too Short)
        var errorsShort = CustomFieldValidator.Validate(def, "Hi");
        Assert.Single(errorsShort);
        Assert.Contains("must be at least 5 characters", errorsShort.First());

        // Act & Assert (Too Long)
        var errorsLong = CustomFieldValidator.Validate(def, "Hello World!");
        Assert.Single(errorsLong);
        Assert.Contains("must be at most 10 characters", errorsLong.First());
    }

    [Fact]
    public void Validate_Number_RangeCheck_ReturnsSuccessAndErrors()
    {
        // Arrange
        var def = new CustomFieldDefinition
        {
            Label = "Age",
            FieldType = CustomFieldType.Number,
            ValidationJson = "{\"Min\":18,\"Max\":65}"
        };

        // Act & Assert (Valid)
        var errorsOk = CustomFieldValidator.Validate(def, "25");
        Assert.Empty(errorsOk);

        // Act & Assert (Invalid Type)
        var errorsType = CustomFieldValidator.Validate(def, "twenty");
        Assert.Single(errorsType);
        Assert.Contains("must be a valid number", errorsType.First());

        // Act & Assert (Too Low)
        var errorsLow = CustomFieldValidator.Validate(def, "17");
        Assert.Single(errorsLow);
        Assert.Contains("must be greater than or equal to 18", errorsLow.First());

        // Act & Assert (Too High)
        var errorsHigh = CustomFieldValidator.Validate(def, "66");
        Assert.Single(errorsHigh);
        Assert.Contains("must be less than or equal to 65", errorsHigh.First());
    }

    [Fact]
    public void Validate_Date_RangeCheck_ReturnsSuccessAndErrors()
    {
        // Arrange
        var def = new CustomFieldDefinition
        {
            Label = "Start Date",
            FieldType = CustomFieldType.Date,
            ValidationJson = "{\"MinDate\":\"2026-01-01\",\"MaxDate\":\"2026-12-31\"}"
        };

        // Act & Assert (Valid)
        var errorsOk = CustomFieldValidator.Validate(def, "2026-06-01");
        Assert.Empty(errorsOk);

        // Act & Assert (Invalid Date format)
        var errorsFormat = CustomFieldValidator.Validate(def, "not-a-date");
        Assert.Single(errorsFormat);
        Assert.Contains("must be a valid date", errorsFormat.First());

        // Act & Assert (Too Early)
        var errorsEarly = CustomFieldValidator.Validate(def, "2025-12-31");
        Assert.Single(errorsEarly);
        Assert.Contains("must be on or after 2026-01-01", errorsEarly.First());

        // Act & Assert (Too Late)
        var errorsLate = CustomFieldValidator.Validate(def, "2027-01-01");
        Assert.Single(errorsLate);
        Assert.Contains("must be on or before 2026-12-31", errorsLate.First());
    }

    [Fact]
    public void Validate_Boolean_ValueCheck_ReturnsSuccessAndErrors()
    {
        // Arrange
        var def = new CustomFieldDefinition
        {
            Label = "Consent",
            FieldType = CustomFieldType.Boolean
        };

        // Act & Assert (Valid)
        var errorsTrue = CustomFieldValidator.Validate(def, "true");
        var errorsFalse = CustomFieldValidator.Validate(def, "false");
        Assert.Empty(errorsTrue);
        Assert.Empty(errorsFalse);

        // Act & Assert (Invalid)
        var errorsInvalid = CustomFieldValidator.Validate(def, "yes");
        Assert.Single(errorsInvalid);
        Assert.Contains("must be true or false", errorsInvalid.First());
    }

    [Fact]
    public void Validate_SingleChoice_ChoiceCheck_ReturnsSuccessAndErrors()
    {
        // Arrange
        var def = new CustomFieldDefinition
        {
            Label = "Option",
            FieldType = CustomFieldType.SingleChoice,
            ValidationJson = "{\"Choices\":[\"Yes\",\"No\"]}"
        };

        // Act & Assert (Valid)
        var errorsOk = CustomFieldValidator.Validate(def, "Yes");
        Assert.Empty(errorsOk);

        // Act & Assert (Invalid Choice)
        var errorsBad = CustomFieldValidator.Validate(def, "Maybe");
        Assert.Single(errorsBad);
        Assert.Contains("contains an invalid value", errorsBad.First());
    }

    [Fact]
    public void Validate_ShortText_PresetCheck_ReturnsSuccessAndErrors()
    {
        // Arrange
        var def = new CustomFieldDefinition
        {
            Label = "Email Field",
            FieldType = CustomFieldType.ShortText,
            ValidationJson = "{\"Presets\":[\"email\"]}"
        };

        // Act & Assert (Valid email format)
        var errorsOk = CustomFieldValidator.Validate(def, "test@example.com");
        Assert.Empty(errorsOk);

        // Act & Assert (Invalid email format)
        var errorsBad = CustomFieldValidator.Validate(def, "invalid-email");
        Assert.Single(errorsBad);
        Assert.Contains("Invalid email address", errorsBad.First());
    }

    [Fact]
    public void Validate_ShortText_FormatMaskCheck_ReturnsSuccessAndErrors()
    {
        // Arrange
        var def = new CustomFieldDefinition
        {
            Label = "Zip Code",
            FieldType = CustomFieldType.ShortText,
            ValidationJson = "{\"FormatMask\":\"#####-###\"}"
        };

        // Act & Assert (Valid zip)
        var errorsOk = CustomFieldValidator.Validate(def, "12345-678");
        Assert.Empty(errorsOk);

        // Act & Assert (Invalid zip format)
        var errorsBad = CustomFieldValidator.Validate(def, "12-345");
        Assert.Single(errorsBad);
        Assert.Contains("does not match the expected format", errorsBad.First());
    }

    [Fact]
    public void Validate_NullValidationJson_ReturnsSuccess()
    {
        // Arrange
        var def = new CustomFieldDefinition
        {
            Label = "Optional Text",
            FieldType = CustomFieldType.ShortText,
            ValidationJson = null
        };

        // Act
        var errors = CustomFieldValidator.Validate(def, "Some random text");

        // Assert
        Assert.Empty(errors);
    }
}
