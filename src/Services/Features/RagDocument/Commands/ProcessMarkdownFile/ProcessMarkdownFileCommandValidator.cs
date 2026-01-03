using FluentValidation;
using Microsoft.Extensions.Localization;
using System.IO;
using System.Linq;

namespace Services.Features.RagDocument.Commands.ProcessMarkdownFile
{
    /// <summary>
    /// Validator for ProcessMarkdownFileCommand
    /// </summary>
    public class ProcessMarkdownFileCommandValidator : AbstractValidator<ProcessMarkdownFileCommand>
    {
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
        private static readonly string[] AllowedExtensions = { ".md", ".markdown" };

        public ProcessMarkdownFileCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.File)
                .NotNull()
                .WithMessage(localizer["RequiredField", nameof(ProcessMarkdownFileCommand.File)]);

            RuleFor(x => x.File.Length)
                .GreaterThan(0)
                .WithMessage(localizer["InvalidEmpty", nameof(ProcessMarkdownFileCommand.File)])
                .LessThanOrEqualTo(MaxFileSizeBytes)
                .WithMessage(localizer["File_TooLarge", (MaxFileSizeBytes / 1024 / 1024)])
                .When(x => x.File != null);

            RuleFor(x => x.File.FileName)
                .Must(HaveValidExtension)
                .WithMessage(localizer["File_InvalidFormat", string.Join(", ", AllowedExtensions)])
                .When(x => x.File != null);

            RuleFor(x => x.Category)
                .NotEmpty()
                .WithMessage(localizer["RequiredField", nameof(ProcessMarkdownFileCommand.Category)])
                .MaximumLength(100)
                .WithMessage(localizer["MaxLength", nameof(ProcessMarkdownFileCommand.Category), 100]);

            RuleFor(x => x.Weight)
                .InclusiveBetween(1, 10)
                .WithMessage(localizer["InvalidRange", nameof(ProcessMarkdownFileCommand.Weight), 1, 10]);

            RuleFor(x => x.AccessLevel)
                .InclusiveBetween(0, 2)
                .WithMessage(localizer["RagMardownFile_ErrorAccessLevel"]);

            RuleFor(x => x.ChunkSize)
                .InclusiveBetween(100, 4000)
                .WithMessage(localizer["InvalidRange", nameof(ProcessMarkdownFileCommand.ChunkSize), 100, 4000]);

            RuleFor(x => x.ChunkOverlap)
                .GreaterThanOrEqualTo(0)
                .WithMessage(localizer["MustBeGreaterThan", nameof(ProcessMarkdownFileCommand.ChunkOverlap), 0])
                .LessThan(x => x.ChunkSize)
                .WithMessage(localizer["RagMarkdownFile_InvalidChunkSize"]);

            RuleFor(x => x.Keywords)
                .MaximumLength(500)
                .WithMessage(localizer["MaxLength", nameof(ProcessMarkdownFileCommand.Keywords), 500])
                .When(x => !string.IsNullOrEmpty(x.Keywords));

            RuleFor(x => x.Source)
                .MaximumLength(200)
                .WithMessage(localizer["MaxLength", nameof(ProcessMarkdownFileCommand.Source), 200])
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
