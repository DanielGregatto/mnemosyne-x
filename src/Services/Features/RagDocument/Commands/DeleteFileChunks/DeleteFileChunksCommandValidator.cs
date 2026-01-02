using FluentValidation;
using Microsoft.Extensions.Localization;
using Services.Features.RagDocument.Commands.DeleteFileChunks;

namespace Services.Features.RagDocument.Commands.DeleteDocument
{
    /// <summary>
    /// Validator for DeleteFileChunksCommand
    /// </summary>
    public class DeleteFileChunksCommandValidator : AbstractValidator<DeleteFileChunksCommand>
    {
        public DeleteFileChunksCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.FileName)
                .NotEmpty()
                .WithMessage(localizer["InvalidEmpty", nameof(DeleteFileChunksCommand.FileName)]);
        }
    }
}
