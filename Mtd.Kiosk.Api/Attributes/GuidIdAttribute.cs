using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Mtd.Kiosk.Api.Attributes;

/// <summary>
/// Custom validation attribute to validate Kiosk ID patterns.
/// Ensures the Kiosk ID is exactly 32 characters long and contains only lowercase letters and digits.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
public class GuidIdAttribute(bool required) : ValidationAttribute
{
	private static readonly Regex _kioskIdRegex = new Regex("^[0-9a-f]{8}-?[0-9a-f]{4}-?[0-9a-f]{4}-?[0-9a-f]{4}-?[0-9a-f]{12}$", RegexOptions.Compiled);

	/// <inheritdoc/>

	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		if (value == null)
		{
			if (required)
			{
				return new ValidationResult("A GUID is required.");
			}

			return ValidationResult.Success;
		}

		if (value is not string stringValue || !_kioskIdRegex.IsMatch(stringValue))
		{
			// Return a ValidationResult with the error message
			return new ValidationResult("The GUID format is invalid. It must be exactly 32 characters long, containing only lowercase letters and digits.");
		}

		// If valid, return Success (using null-forgiving operator)
		return ValidationResult.Success;
	}
}
