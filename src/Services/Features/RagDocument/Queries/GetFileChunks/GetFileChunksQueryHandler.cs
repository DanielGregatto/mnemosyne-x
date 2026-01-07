using Data.Context;
using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Services.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.RagDocument.Queries.GetFileChunks
{
    public class GetFileChunksQueryHandler : BaseQueryHandler,
        IRequestHandler<GetFileChunksQuery, Result<List<RagDocumentDto>>>
    {
        private readonly IQdrantRagDocumentRepository _qdrantRepository;
        private readonly ILogger<GetFileChunksQueryHandler> _logger;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public GetFileChunksQueryHandler(
            AppDbContext context,
            IUser user,
            IQdrantRagDocumentRepository qdrantRepository,
            ILogger<GetFileChunksQueryHandler> logger,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _qdrantRepository = qdrantRepository;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<Result<List<RagDocumentDto>>> Handle(
            GetFileChunksQuery request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.FileName))
                return Result<List<RagDocumentDto>>.Failure(_localizer["InvalidEmpty", nameof(GetFileChunksQuery.FileName)]);

            _logger.LogInformation("Retrieving all chunks for file: {FileName}", request.FileName);

            var documents = await _qdrantRepository.GetByFileNameAsync(request.FileName);
            var docList = documents.Cast<Domain.RagDocument>().ToList();

            if (!docList.Any())
            {
                _logger.LogInformation("No chunks found for file {FileName}", request.FileName);
                return Result<List<RagDocumentDto>>.Success(new List<RagDocumentDto>());
            }

            var userAccessLevel = DetermineUserAccessLevel();

            var filteredDocs = docList.Where(d => d.AccessLevel <= userAccessLevel)
                                      .OrderBy(d => d.ChunkIndex)
                                      .ToList();

            _logger.LogInformation("Found {TotalCount} chunks for file {FileName}, {FilteredCount} accessible to user", docList.Count, request.FileName, filteredDocs.Count);

            var dtos = filteredDocs.Select(x => x.MapToDto())
                                   .ToList();

            return Result<List<RagDocumentDto>>.Success(dtos);
        }
    }
}