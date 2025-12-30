using FluentValidation;
using System;

namespace Services.Features.RagDocument.Commands.DeleteDocument
{
    /// <summary>
    /// Validator for DeleteDocumentCommand
    /// </summary>
    public class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
    {
        public DeleteDocumentCommandValidator()
        {
            RuleFor(x => x.DocumentId)
                .NotEqual(Guid.Empty)
                .WithMessage("DocumentId cannot be empty");
        }
    }
}
