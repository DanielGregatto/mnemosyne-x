using FluentValidation;
using Microsoft.Extensions.Localization;
using System;

namespace Services.Features.RagDocument.Commands.DeleteDocument
{
    /// <summary>
    /// Validator for DeleteDocumentCommand
    /// </summary>
    public class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
    {
        public DeleteDocumentCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.DocumentId)
                .NotEqual(Guid.Empty)
                .WithMessage(localizer["InvalidEmpty", nameof(DeleteDocumentCommand.DocumentId)]);
        }
    }
}
