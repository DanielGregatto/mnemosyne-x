using FluentValidation;
using System.IO;
using System.Linq;

namespace Services.Features.RagDocument.Commands.UpdateMarkdownFile
{
    /// <summary>
    /// Validator for UpdateMarkdownFileCommand
    /// </summary>
    public class UpdateMarkdownFileCommandValidator : AbstractValidator<UpdateMarkdownFileCommand>
    {
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
        private static readonly string[] AllowedExtensions = { ".md", ".markdown" };

        public UpdateMarkdownFileCommandValidator()
        {
            RuleFor(x => x.ExistingFileName)
                .NotEmpty()
                .WithMessage("ExistingFileName is required")
                .MaximumLength(255)
                .WithMessage("ExistingFileName cannot exceed 255 characters");

            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("File is required");

            RuleFor(x => x.File.Length)
                .GreaterThan(0)
                .WithMessage("File cannot be empty")
                .LessThanOrEqualTo(MaxFileSizeBytes)
                .WithMessage($"File size cannot exceed {MaxFileSizeBytes / 1024 / 1024} MB")
                .When(x => x.File != null);

            RuleFor(x => x.File.FileName)
                .Must(HaveValidExtension)
                .WithMessage($"Only markdown files are allowed ({string.Join(", ", AllowedExtensions)})")
                .When(x => x.File != null);

            RuleFor(x => x.Category)
                .NotEmpty()
                .WithMessage("Category is required")
                .MaximumLength(100)
                .WithMessage("Category cannot exceed 100 characters");

            RuleFor(x => x.Weight)
                .InclusiveBetween(1, 10)
                .WithMessage("Weight must be between 1 and 10");

            RuleFor(x => x.AccessLevel)
                .InclusiveBetween(0, 2)
                .WithMessage("AccessLevel must be 0 (public), 1 (authenticated), or 2 (admin)");

            RuleFor(x => x.ChunkSize)
                .InclusiveBetween(100, 4000)
                .WithMessage("ChunkSize must be between 100 and 4000 characters");

            RuleFor(x => x.ChunkOverlap)
                .GreaterThanOrEqualTo(0)
                .WithMessage("ChunkOverlap cannot be negative")
                .LessThan(x => x.ChunkSize)
                .WithMessage("ChunkOverlap must be less than ChunkSize");

            RuleFor(x => x.Keywords)
                .MaximumLength(500)
                .WithMessage("Keywords cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Keywords));

            RuleFor(x => x.Source)
                .MaximumLength(200)
                .WithMessage("Source cannot exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.Source));
        }

        private bool HaveValidExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return AllowedExtensions.Contains(extension);
        }
    }
}
