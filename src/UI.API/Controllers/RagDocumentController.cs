using Domain.DTO.Infrastructure.API;
using Domain.DTO.Responses;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Features.RagDocument.Commands.BatchProcessFiles;
using Services.Features.RagDocument.Commands.DeleteDocument;
using Services.Features.RagDocument.Commands.DeleteFileChunks;
using Services.Features.RagDocument.Commands.ProcessMarkdownFile;
using Services.Features.RagDocument.Commands.UpdateMarkdownFile;
using Services.Features.RagDocument.Queries.GetDocumentById;
using Services.Features.RagDocument.Queries.GetFileChunks;
using Services.Features.RagDocument.Queries.SearchBySemantic;
using Services.Features.RagDocument.Queries.SearchBySemanticFactorX;
using UI.API.Controllers.Base;

namespace UI.API.Controllers
{
    /// <summary>
    /// RAG (Retrieval-Augmented Generation) document management controller
    /// Provides endpoints for processing markdown files, semantic search, and Factor X contextual retrieval
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    public class RagDocumentController : CoreController
    {
        private readonly IMediatorHandler _mediator;

        public RagDocumentController(IMediatorHandler mediatorHandler)
        {
            _mediator = mediatorHandler;
        }

        /// <summary>
        /// Search documents by semantic similarity
        /// </summary>
        /// <param name="query">Search query with user query and limit</param>
        /// <returns>List of matching documents ordered by similarity</returns>
        [HttpPost("v1/rag/search")]
        [ProducesResponseType(typeof(SuccessResponse<List<RagDocumentDto>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 400)]
        public async Task<IActionResult> SearchBySemantic([FromBody] SearchBySemanticQuery query)
        {
            var result = await _mediator.SendCommand(query);
            return Response(result);
        }

        /// <summary>
        /// Search documents with Factor X contextual expansion
        /// Expands results by including adjacent chunks based on similarity thresholds
        /// </summary>
        /// <param name="query">Search query with Factor X parameters</param>
        /// <returns>List of matching documents with contextual expansion</returns>
        [HttpPost("v1/rag/search-factorx")]
        [ProducesResponseType(typeof(SuccessResponse<List<RagDocumentDto>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 400)]
        public async Task<IActionResult> SearchBySemanticFactorX([FromBody] SearchBySemanticFactorXQuery query)
        {
            var result = await _mediator.SendCommand(query);
            return Response(result);
        }

        /// <summary>
        /// Get a specific document by ID
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <returns>Document details if found and accessible</returns>
        [HttpGet("v1/rag/{id}")]
        [ProducesResponseType(typeof(SuccessResponse<RagDocumentDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        public async Task<IActionResult> GetDocumentById(Guid id)
        {
            var query = new GetDocumentByIdQuery { DocumentId = id };
            var result = await _mediator.SendCommand(query);
            return Response(result);
        }

        /// <summary>
        /// Get all chunks of a file by filename
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <returns>List of all chunks for the file, sorted by chunk index</returns>
        [HttpGet("v1/rag/file/{fileName}")]
        [ProducesResponseType(typeof(SuccessResponse<List<RagDocumentDto>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 400)]
        public async Task<IActionResult> GetFileChunks(string fileName)
        {
            var query = new GetFileChunksQuery { FileName = fileName };
            var result = await _mediator.SendCommand(query);
            return Response(result);
        }

        /// <summary>
        /// Process a markdown file and store it in the RAG system
        /// </summary>
        /// <param name="command">Command containing the file and processing parameters</param>
        /// <returns>Processing result with chunk statistics</returns>
        [HttpPost("v1/rag/process-file")]
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<ProcessFileResultDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 400)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ProcessMarkdownFile([FromForm] ProcessMarkdownFileCommand command)
        {
            var result = await _mediator.SendCommand(command);
            return Response(result);
        }

        /// <summary>
        /// Update an existing markdown file by deleting old chunks and reprocessing
        /// </summary>
        /// <param name="command">Command containing the existing filename, new file, and processing parameters</param>
        /// <returns>Processing result with updated version and chunk statistics</returns>
        [HttpPut("v1/rag/update-file")]
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<ProcessFileResultDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 400)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateMarkdownFile([FromForm] UpdateMarkdownFileCommand command)
        {
            var result = await _mediator.SendCommand(command);
            return Response(result);
        }

        /// <summary>
        /// Delete a specific document by ID
        /// </summary>
        /// <param name="id">Document ID to delete</param>
        /// <returns>Success if deleted</returns>
        [HttpDelete("v1/rag/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<bool>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 400)]
        public async Task<IActionResult> DeleteDocument(Guid id)
        {
            var command = new DeleteDocumentCommand { DocumentId = id };
            var result = await _mediator.SendCommand(command);
            return Response(result);
        }

        /// <summary>
        /// Delete all chunks of a file by filename
        /// </summary>
        /// <param name="fileName">Name of the file to delete</param>
        /// <returns>Number of deleted chunks</returns>
        [HttpDelete("v1/rag/file/{fileName}")]
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<int>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 400)]
        public async Task<IActionResult> DeleteFileChunks(string fileName)
        {
            var command = new DeleteFileChunksCommand { FileName = fileName };
            var result = await _mediator.SendCommand(command);
            return Response(result);
        }

        /// <summary>
        /// Process multiple markdown files in batch
        /// </summary>
        /// <param name="command">Command containing the files and processing parameters</param>
        /// <returns>List of processing results for each file</returns>
        [HttpPost("v1/rag/batch-process")]
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<List<ProcessFileResultDto>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 400)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> BatchProcessFiles([FromForm] BatchProcessFilesCommand command)
        {
            var result = await _mediator.SendCommand(command);
            return Response(result);
        }
    }
}
