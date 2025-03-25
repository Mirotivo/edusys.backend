using Backend.DTOs.Listing;
using FluentValidation;

namespace Backend.Validators
{
    public class ListingRequestValidator : AbstractValidator<ListingRequestDto>
    {
        public ListingRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

            RuleFor(x => x.HourlyRate)
                .NotNull().WithMessage("Hourly rate is required.")
                .GreaterThanOrEqualTo(0).WithMessage("Hourly rate must be a positive value.");

            RuleFor(x => x.CategoryIds)
                .NotEmpty().WithMessage("At least one lesson category is required.");
        }
    }
}
