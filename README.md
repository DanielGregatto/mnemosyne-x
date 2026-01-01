# MNEMOSYNE-X

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Qdrant](https://img.shields.io/badge/Vector%20DB-Qdrant-red)](https://qdrant.tech/)
[![OpenAI](https://img.shields.io/badge/Embeddings-OpenAI-green)](https://platform.openai.com/)

**Memory with the X Factor**

A production-ready **RAG (Retrieval-Augmented Generation)** API built with **ASP.NET Core 8**, featuring the innovative **Factor X algorithm** for contextual semantic search. MNEMOSYNE-X solves the critical problem of **context fragmentation** in traditional vector search by intelligently retrieving neighboring document chunks based on dynamic similarity thresholds.

## What Makes This Different?

### The Factor X Algorithm

Traditional RAG systems suffer from **context fragmentation** - they return isolated chunks that may contain incomplete information. The **Factor X algorithm** solves this by:

1. **Dynamic Threshold Adaptation**: Instead of returning fixed top-K results, it adapts expansion based on semantic similarity gradients
2. **Bidirectional Context Expansion**: Automatically retrieves neighboring chunks that maintain semantic coherence with the query
3. **Smart Boundary Detection**: Stops expansion when similarity drops below a configurable threshold (FactorX)

### Real-World Example

**Use Case: Medical Formulation Database**

Imagine a medicine formulation spread across multiple chunks:

```
Chunk #4: "Medicine X formula requires compound A (500mg)..."
Chunk #5: "...mixed with compound B (300mg) at room temperature..."
Chunk #6: "...administered twice daily with meals..."
```

**Query:** "What is the dosage for Medicine X?"

**Standard Vector Search:**
- Returns only Chunk #5 (best match for "dosage")
- ❌ Result: Incomplete information (missing compound A and administration frequency)

**Factor X Search:**
1. Finds best match: Chunk #5 (score: 0.87)
2. Sets threshold: 0.87 - 0.05 = 0.82
3. Expands bidirectionally:
   - Chunk #4 (score: 0.84) ✓ Include
   - Chunk #6 (score: 0.83) ✓ Include
4. ✅ Result: Complete formulation with full context

## Table of Contents

- [Key Features](#key-features)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
- [RAG Configuration](#rag-configuration)
- [Document Processing](#document-processing)
- [Search API](#search-api)
- [Factor X Algorithm Details](#factor-x-algorithm-details)
- [Use Cases](#use-cases)
- [API Endpoints](#api-endpoints)
- [Testing](#testing)
- [Project Structure](#project-structure)
- [Contributing](#contributing)

## Key Features

### RAG & Vector Search
- **OpenAI Embeddings** - Text-embedding-ada-002 (1536 dimensions)
- **Qdrant Vector Database** - High-performance similarity search
- **Factor X Algorithm** - Intelligent neighbor chunk retrieval
- **Smart Chunking** - Configurable sliding window with overlap
- **Access Control** - Multi-level document permissions (public, authenticated, admin)
- **Deduplication** - Content and embedding hash tracking
- **Metadata Search** - Filter by category, source, keywords
- **Graceful Degradation** - Fallback embeddings when OpenAI unavailable

### Architecture & Patterns
- **CQRS Pattern** - Separate commands and queries with MediatR
- **Clean Architecture** - Clear separation of concerns across layers
- **Result Pattern** - Consistent error handling across the application
- **Domain Events** - Automatic entity lifecycle events
- **Unit of Work** - Transaction management with event publishing

### Authentication & Security
- **JWT Token Authentication** with refresh token support
- **Social Login Integration** (Google, Facebook, Microsoft)
- **Email Confirmation** workflow
- **Password Reset** functionality
- **Role-Based Access Control** via ASP.NET Core Identity
- **Rate Limiting** - Prevent API abuse with configurable limits
- **CORS Configuration** - Environment-specific origin whitelisting

### Development Features
- **FluentValidation** - Declarative validation for all commands
- **Swagger/OpenAPI** documentation with JWT support
- **Localization** - Multi-language support (en-US, pt-BR)
- **Structured Logging** with Application Insights integration
- **Docker Support** - Ready for containerized deployment
- **Unit Tests** - xUnit, FluentAssertions, Moq

## Architecture

### RAG Processing Pipeline

```
┌─────────────────────────────────────────────────────────────────┐
│                    DOCUMENT INGESTION                           │
├─────────────────────────────────────────────────────────────────┤
│  Upload File → Chunk Content → Generate Embeddings             │
│              → Link Chunks → Store in Qdrant                    │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                    VECTOR STORAGE (Qdrant)                      │
├─────────────────────────────────────────────────────────────────┤
│  • 1536-dimensional vectors (OpenAI ada-002)                    │
│  • Cosine similarity metric                                     │
│  • Metadata: category, source, keywords, access level           │
│  • Chunk navigation: previous_id, next_id                       │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                    RETRIEVAL (Factor X)                         │
├─────────────────────────────────────────────────────────────────┤
│  1. User Query → Generate Embedding                             │
│  2. Semantic Search → Top-K Results                             │
│  3. For each result:                                            │
│     • Calculate threshold: score - FactorX                      │
│     • Expand NEXT chunks while score ≥ threshold                │
│     • Expand PREVIOUS chunks while score ≥ threshold            │
│  4. Deduplicate & Sort → Return Contextual Results              │
└─────────────────────────────────────────────────────────────────┘
```

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                   Presentation Layer                        │
│                      (UI.API)                               │
│         Controllers, Middleware, Configurations             │
└─────────────────────────┬───────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────┐
│                  Application Layer                          │
│                     (Services)                              │
│        Commands, Queries, Handlers, Validators              │
│                                                             │
│  Features:                                                  │
│  • RagDocument (Process, Search, Update)                    │
│  • Auth (Login, Register, OAuth)                            │
│  • Account (Profile, Password)                              │
└─────────────────────────┬───────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────┐
│                    Domain Layer                             │
│               (Domain + Domain.Core)                        │
│     Entities, DTOs, Interfaces, Events, Enums               │
│                                                             │
│  Key Entities:                                              │
│  • RagDocument (chunks with embeddings)                     │
│  • ApplicationUser (Identity)                               │
└─────────────────────────┬───────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────┐
│                Infrastructure Layer                         │
│           (Data, Identity, IoC, Util)                       │
│                                                             │
│  • QdrantRagDocumentRepository (Vector DB)                  │
│  • OpenAIService (Embeddings)                               │
│  • AppDbContext (SQL Server)                                │
│  • JWT Authentication                                       │
└─────────────────────────────────────────────────────────────┘
```

## Technology Stack

| Component | Technology |
|-----------|-----------|
| **Framework** | ASP.NET Core 8.0 |
| **Vector Database** | Qdrant (v1.7+) |
| **Embeddings** | OpenAI text-embedding-ada-002 |
| **RDBMS** | SQL Server (metadata, users) |
| **CQRS/Mediator** | MediatR 12.0 |
| **ORM** | Entity Framework Core 8.0 |
| **Validation** | FluentValidation 12.1 |
| **Authentication** | ASP.NET Core Identity, JWT Bearer |
| **Testing** | xUnit, FluentAssertions, Moq |
| **Documentation** | Swashbuckle (Swagger) |
| **Containerization** | Docker |

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, or Full)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for Qdrant)
- [OpenAI API Key](https://platform.openai.com/api-keys)

### Installation

#### 1. Clone the Repository

```bash
git clone https://github.com/DanielGregatto/mnemosyne-x.git
cd mnemosyne-x
```

#### 2. Start Qdrant Vector Database

Using Docker (recommended):

```bash
docker run -p 6333:6333 -p 6334:6334 \
  -v $(pwd)/qdrant_storage:/qdrant/storage:z \
  qdrant/qdrant
```

Verify Qdrant is running:
- Web UI: http://localhost:6333/dashboard
- API: http://localhost:6333/collections

#### 3. Configure Application Settings

Copy the example configuration:

```bash
cd src/UI.API
copy appsettings.Development.json.example appsettings.Development.json
```

Edit `appsettings.Development.json` with your credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RagApiDb;Trusted_Connection=True;"
  },
  "OpenAIConfig": {
    "Token": "sk-your-openai-api-key-here",
    "EmbeddingModel": "text-embedding-ada-002"
  },
  "QdrantConfig": {
    "Host": "localhost",
    "Port": 6334,
    "ApiKey": null,
    "UseHttps": false,
    "CollectionName": "rag_documents",
    "VectorSize": 1536,
    "Distance": "cosine"
  },
  "Jwt": {
    "Secret": "your-super-secret-key-minimum-32-characters-long",
    "MinutesValid": "60",
    "Issuer": "RagApi",
    "Audience": "https://localhost:7182"
  }
}
```

**Important Configuration Notes:**

- **OpenAI Token**: Required for generating embeddings. Get from https://platform.openai.com/api-keys
- **Qdrant Port**: Use 6334 for gRPC (faster) or 6333 for HTTP
- **JWT Secret**: Must be at least 32 characters for security
- **VectorSize**: Must match OpenAI model (ada-002 = 1536 dimensions)

#### 4. Run Database Migrations

```bash
cd src/UI.API
dotnet ef database update --project ../Data
```

This creates:
- Identity tables (users, roles, tokens)
- Application tables (status, log errors)
- Note: RagDocuments are stored in Qdrant, not SQL Server

#### 5. Run the Application

```bash
dotnet run
```

The API will start on:
- HTTPS: https://localhost:7182
- HTTP: http://localhost:5116

#### 6. Access Swagger Documentation

Navigate to: https://localhost:7182/swagger

## RAG Configuration

### OpenAI Embeddings

```json
{
  "OpenAIConfig": {
    "Token": "sk-your-api-key",
    "EmbeddingModel": "text-embedding-ada-002",
    "AssistantID": ""
  }
}
```

**Supported Models:**
- `text-embedding-ada-002` (1536 dimensions) - Default, cost-effective
- `text-embedding-3-small` (512 or 1536 dimensions) - Newer, cheaper
- `text-embedding-3-large` (256, 1024, or 3072 dimensions) - Best quality

**Note:** If you change the model, update `QdrantConfig.VectorSize` accordingly.

### Qdrant Configuration

```json
{
  "QdrantConfig": {
    "Host": "localhost",
    "Port": 6334,
    "ApiKey": null,
    "UseHttps": false,
    "CollectionName": "rag_documents",
    "VectorSize": 1536,
    "Distance": "cosine",
    "Timeout": 30000,
    "MaxRetries": 3
  }
}
```

**Distance Metrics:**
- `cosine` (default) - Best for semantic text similarity
- `dot` - Dot product similarity
- `euclid` - Euclidean distance

**Production Setup:**
- Set `UseHttps: true` for cloud deployment
- Add `ApiKey` for authentication
- Use managed Qdrant Cloud: https://cloud.qdrant.io/

### Chunking Configuration

Configure in `ProcessMarkdownFileCommand`:

```json
{
  "ChunkSize": 1000,
  "ChunkOverlap": 200,
  "Category": "Documentation",
  "Weight": 5,
  "AccessLevel": 0
}
```

**Parameters:**
- **ChunkSize**: Characters per chunk (default: 1000)
- **ChunkOverlap**: Overlap between chunks (default: 200)
  - 20% overlap ensures context continuity
  - Critical for Factor X algorithm effectiveness
- **Weight**: Search ranking multiplier (1-10)
- **AccessLevel**:
  - 0 = Public
  - 1 = Authenticated users only
  - 2 = Admin only

### Factor X Search Parameters

```json
{
  "Limit": 10,
  "FactorX": 0.05,
  "MaxExpansionDepth": 5
}
```

**Parameters:**
- **Limit**: Initial search results (top-K)
- **FactorX**: Similarity threshold drop tolerance
  - 0.05 = Allow 5% similarity drop for neighbors
  - Lower values = stricter expansion (more precise)
  - Higher values = looser expansion (more context)
- **MaxExpansionDepth**: Max chunks per direction
  - Limits context window size
  - Prevents excessive expansion

## Document Processing

### Upload and Process Document

**Endpoint:** `POST /v1/rag/process-file`

Upload a markdown file for processing:

```bash
curl -X POST "https://localhost:7182/v1/rag/process-file" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@medicine-guide.md" \
  -F "category=Medical" \
  -F "weight=8" \
  -F "accessLevel=1" \
  -F "chunkSize=1000" \
  -F "chunkOverlap=200"
```

**Response:**
```json
{
  "success": true,
  "fileName": "medicine-guide.md",
  "totalChunks": 25,
  "processedChunks": 25,
  "message": "Successfully processed 25 chunks"
}
```

### Processing Pipeline

1. **File Upload**: Receives markdown file
2. **Content Extraction**: Reads file as text
3. **Chunking**: Splits into overlapping segments
   ```
   Chunk 1: chars 0-1000
   Chunk 2: chars 800-1800   (200 char overlap)
   Chunk 3: chars 1600-2600  (200 char overlap)
   ...
   ```
4. **Embedding Generation**: Calls OpenAI for each chunk
5. **Metadata Enrichment**: Adds category, weight, access level
6. **Chunk Linking**: Creates bidirectional links
   ```
   Chunk 1 ←→ Chunk 2 ←→ Chunk 3 ←→ ... ←→ Chunk N
   ```
7. **Vector Storage**: Upserts to Qdrant with metadata
8. **Hash Computation**: Stores SHA256 for deduplication

### Batch Processing

**Endpoint:** `POST /v1/rag/batch-process`

Process multiple files:

```bash
curl -X POST "https://localhost:7182/v1/rag/batch-process" \
  -H "Content-Type: multipart/form-data" \
  -F "files=@doc1.md" \
  -F "files=@doc2.md" \
  -F "files=@doc3.md" \
  -F "category=Documentation" \
  -F "weight=5"
```

### Update Document

**Endpoint:** `PUT /v1/rag/update-file/{fileName}`

Update an existing document (replaces all chunks):

```bash
curl -X PUT "https://localhost:7182/v1/rag/update-file/medicine-guide.md" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@medicine-guide-v2.md"
```

## Search API

### Standard Semantic Search

**Endpoint:** `POST /v1/rag/search`

Basic vector similarity search:

```bash
curl -X POST "https://localhost:7182/v1/rag/search" \
  -H "Content-Type: application/json" \
  -d '{
    "userQuery": "How to administer Medicine X?",
    "limit": 10
  }'
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "fileName": "medicine-guide.md",
      "content": "...administered twice daily with meals...",
      "chunkIndex": 5,
      "totalChunks": 25,
      "category": "Medical",
      "score": 0.87
    }
  ]
}
```

### Factor X Contextual Search

**Endpoint:** `POST /v1/rag/search-factorx`

Enhanced search with neighbor expansion:

```bash
curl -X POST "https://localhost:7182/v1/rag/search-factorx" \
  -H "Content-Type: application/json" \
  -d '{
    "userQuery": "What is the dosage for Medicine X?",
    "limit": 5,
    "factorX": 0.05,
    "maxExpansionDepth": 5
  }'
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "...",
      "fileName": "medicine-guide.md",
      "content": "Medicine X formula requires compound A (500mg)...",
      "chunkIndex": 4,
      "score": 0.84
    },
    {
      "id": "...",
      "fileName": "medicine-guide.md",
      "content": "...mixed with compound B (300mg) at room temperature...",
      "chunkIndex": 5,
      "score": 0.87
    },
    {
      "id": "...",
      "fileName": "medicine-guide.md",
      "content": "...administered twice daily with meals...",
      "chunkIndex": 6,
      "score": 0.83
    }
  ],
  "message": "Retrieved 3 contextual chunks (expanded from 1 initial match)"
}
```

**Note:** Results include chunks 4, 5, and 6 as a contiguous sequence, providing complete context.

### Get All Chunks of a File

**Endpoint:** `GET /v1/rag/file/{fileName}`

Retrieve all chunks of a specific file in order:

```bash
curl -X GET "https://localhost:7182/v1/rag/file/medicine-guide.md"
```

## Factor X Algorithm Details

### Mathematical Model

**Similarity Function:**
```
similarity(query, chunk) ∈ [0, 1]  (cosine similarity)
```

**Inclusion Criteria:**

For initial match chunk C₀ with score S₀:

```
Include adjacent chunk Cᵢ if:
  similarity(query, Cᵢ) ≥ S₀ - FactorX
  AND |i| ≤ MaxExpansionDepth
```

### Algorithm Steps

```
1. Generate query embedding q
2. Search Qdrant for top-K chunks: {C₁, C₂, ..., Cₖ}
3. For each initial match Cᵢ with score Sᵢ:
   a. Set threshold Tᵢ = Sᵢ - FactorX
   b. Expand forward (NEXT):
      - Get Cᵢ.NextDocumentId → Cⱼ
      - Calculate similarity(q, Cⱼ)
      - If similarity(q, Cⱼ) ≥ Tᵢ AND depth < MaxDepth:
        * Include Cⱼ
        * Continue with Cⱼ.NextDocumentId
      - Else: Stop expansion
   c. Expand backward (PREVIOUS):
      - Same logic using PreviousDocumentId
4. Deduplicate chunks by ID
5. Sort by (fileName, chunkIndex)
6. Return sorted results
```

### Expansion Example

**Initial Query:** "JWT refresh token implementation"

```
Initial Search Results (top-3):
┌────────┬────────┬──────────────────────────────┐
│ Chunk  │ Score  │ Content Preview              │
├────────┼────────┼──────────────────────────────┤
│ #12    │ 0.89   │ "JWT refresh token rotation" │
│ #27    │ 0.82   │ "Token refresh endpoint..."  │
│ #45    │ 0.78   │ "Refresh token validation"   │
└────────┴────────┴──────────────────────────────┘

Factor X Parameters:
- FactorX: 0.05
- MaxExpansionDepth: 5

Expansion Process for Chunk #12 (Score: 0.89):

Threshold: 0.89 - 0.05 = 0.84

Forward Expansion (NEXT):
┌────────┬────────┬────────────┬──────────┐
│ Chunk  │ Score  │ Meets?     │ Action   │
├────────┼────────┼────────────┼──────────┤
│ #13 →  │ 0.86   │ ≥ 0.84 ✓   │ Include  │
│ #14 →  │ 0.85   │ ≥ 0.84 ✓   │ Include  │
│ #15 →  │ 0.82   │ < 0.84 ✗   │ Stop     │
└────────┴────────┴────────────┴──────────┘

Backward Expansion (PREVIOUS):
┌────────┬────────┬────────────┬──────────┐
│ Chunk  │ Score  │ Meets?     │ Action   │
├────────┼────────┼────────────┼──────────┤
│ #11 ←  │ 0.87   │ ≥ 0.84 ✓   │ Include  │
│ #10 ←  │ 0.84   │ ≥ 0.84 ✓   │ Include  │
│ #9  ←  │ 0.81   │ < 0.84 ✗   │ Stop     │
└────────┴────────┴────────────┴──────────┘

Final Result for Chunk #12:
Chunks #10, #11, #12, #13, #14 (5 chunks total)
```

### Tuning Guidelines

**FactorX Value:**
- **0.02-0.03**: Very strict, minimal expansion (high precision)
- **0.05**: Balanced (recommended default)
- **0.10-0.15**: Aggressive expansion (high recall)

**MaxExpansionDepth:**
- **2-3**: Short answers, focused context
- **5**: Balanced (recommended default)
- **10+**: Long-form content, comprehensive context

**ChunkSize & Overlap:**
- **Small chunks (500 chars)**: Better precision, more granular
- **Large chunks (2000 chars)**: Better context per chunk, fewer chunks
- **Overlap 20-25%**: Recommended for smooth transitions

## Use Cases

### Ideal Scenarios for Factor X

1. **Technical Documentation**
   - API references where method signatures, parameters, and examples span multiple sections
   - Installation guides with prerequisites → steps → verification flow
   - **Example Query:** "How to configure OAuth authentication?"
   - **Benefit:** Returns complete setup flow, not just isolated steps

2. **Medical/Pharmaceutical**
   - Drug formulations with ingredients, preparation, and administration
   - Treatment protocols with diagnostic criteria, procedure steps, and follow-up
   - **Example Query:** "What are the contraindications for Drug Y?"
   - **Benefit:** Returns contraindications plus relevant context (indications, dosing)

3. **Legal/Compliance Documents**
   - Contract clauses that reference previous sections
   - Regulatory requirements with prerequisites and procedures
   - **Example Query:** "What are the data retention requirements?"
   - **Benefit:** Returns requirements plus definitions and exceptions

4. **Scientific Papers**
   - Methods sections where reagents → procedure → analysis are sequential
   - Results that reference methodology
   - **Example Query:** "How was the protein purified?"
   - **Benefit:** Returns complete protocol, not just isolated steps

5. **Knowledge Base Articles**
   - Troubleshooting guides: symptom → diagnosis → solution
   - Product specifications: overview → details → warnings
   - **Example Query:** "How to fix error code 500?"
   - **Benefit:** Returns error context, causes, and solutions together

6. **Educational Content**
   - Tutorials with sequential steps
   - Explanations where concepts build on each other
   - **Example Query:** "Explain how async/await works"
   - **Benefit:** Returns fundamentals, implementation, and examples

## API Endpoints

### RAG Endpoints (`/v1/rag`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/v1/rag/process-file` | Upload and process single markdown file |
| POST | `/v1/rag/batch-process` | Process multiple files |
| PUT | `/v1/rag/update-file/{fileName}` | Update existing document |
| DELETE | `/v1/rag/delete-file/{fileName}` | Delete all chunks of a file |
| POST | `/v1/rag/search` | Standard semantic search |
| POST | `/v1/rag/search-factorx` | Enhanced search with Factor X |
| GET | `/v1/rag/file/{fileName}` | Get all chunks of a file |
| GET | `/v1/rag/{id}` | Get specific chunk by ID |

### Authentication (`/v1/auth`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/v1/auth/login` | Login with email/password |
| POST | `/v1/auth/register` | Register new user |
| POST | `/v1/auth/forgot-password` | Request password reset |
| POST | `/v1/auth/reset-password` | Reset password with token |
| POST | `/v1/auth/refresh` | Refresh access token |
| GET | `/v1/auth/email-confirmed` | Confirm email address |
| GET | `/v1/auth/google-login` | Initiate Google OAuth |
| GET | `/v1/auth/facebook-login` | Initiate Facebook OAuth |

### Account (`/v1/account`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/v1/account/profile` | Get user profile |
| POST | `/v1/account/update-personal-info` | Update personal information |
| POST | `/v1/account/update-password` | Change password |

For complete API documentation with examples, run the project and visit `/swagger`.

## Testing

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Project

```bash
dotnet test src/Tests/Unitary/Domain.Unit.Tests/Unit.Tests.csproj
```

### Run with Code Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Integration Testing with Qdrant

For testing RAG features, ensure Qdrant is running:

```bash
# Start test Qdrant instance
docker run -d --name qdrant-test -p 6333:6333 -p 6334:6334 qdrant/qdrant

# Run integration tests
dotnet test --filter Category=Integration

# Cleanup
docker stop qdrant-test && docker rm qdrant-test
```

## Project Structure

```
src/
├── UI.API/                              # Presentation Layer
│   ├── Controllers/
│   │   ├── RagDocumentController.cs     # RAG endpoints
│   │   ├── AuthController.cs            # Authentication
│   │   └── AccountController.cs         # User account
│   ├── Middleware/
│   └── Program.cs
│
├── Services/                            # Application Layer (CQRS)
│   ├── Features/
│   │   ├── RagDocument/                 # RAG Features
│   │   │   ├── Commands/
│   │   │   │   ├── ProcessMarkdownFile/ # Document ingestion
│   │   │   │   ├── BatchProcessFiles/   # Batch processing
│   │   │   │   └── UpdateMarkdownFile/  # Update documents
│   │   │   └── Queries/
│   │   │       ├── SearchBySemantic/    # Standard search
│   │   │       ├── SearchBySemanticFactorX/  # Factor X search ⭐
│   │   │       ├── GetFileChunks/       # Get all chunks
│   │   │       └── GetRagDocumentById/  # Get single chunk
│   │   ├── Auth/                        # Authentication features
│   │   └── Account/                     # Account management
│   ├── Infrastructure/
│   │   ├── OpenAIService.cs             # OpenAI embeddings ⭐
│   │   └── MediatorHandler.cs
│   └── Core/
│       ├── BaseCommandHandler.cs
│       └── BaseQueryHandler.cs
│
├── Domain/                              # Domain Layer
│   ├── Entities/
│   │   ├── RagDocument.cs               # Chunk entity ⭐
│   │   ├── ApplicationUser.cs
│   │   └── Status.cs
│   ├── DTO/
│   │   ├── RagDocumentDto.cs            # RAG DTOs ⭐
│   │   └── Responses/
│   ├── Interfaces/
│   │   ├── IRagDocumentRepository.cs    # Repository interface ⭐
│   │   └── IOpenAIService.cs            # AI service interface ⭐
│   └── Resources/                       # Localization
│
├── Data/                                # Infrastructure - Data
│   ├── Repository/
│   │   └── QdrantRagDocumentRepository.cs  # Qdrant operations ⭐
│   ├── Context/
│   │   └── AppDbContext.cs              # EF Core context
│   ├── Mappings/                        # EF configurations
│   └── UnitOfWork/                      # UoW pattern
│
├── Identity/                            # Infrastructure - Auth
│   ├── Services/
│   │   ├── JwtService.cs
│   │   └── EmailService.cs
│   └── Model/
│       └── ApplicationUser.cs
│
├── IoC/                                 # Dependency Injection
│   └── DIBootstrapper.cs
│
└── Tests/
    └── Unitary/
        └── Domain.Unit.Tests/

⭐ = RAG-specific components
```

## Docker Deployment

### Development (Docker Compose)

```bash
cd platform/docker
docker-compose up --build
```

This starts:
- ASP.NET Core API (port 5000/5001)
- SQL Server (port 1433)
- Qdrant (port 6333/6334)

### Production Deployment

1. **Build Production Image**
   ```bash
   docker build -t rag-api:latest -f Dockerfile .
   ```

2. **Run with External Services**
   ```bash
   docker run -d \
     -p 5000:5000 \
     -e ConnectionStrings__DefaultConnection="Server=sql-server;Database=RagDb;User=sa;Password=YourPassword;" \
     -e QdrantConfig__Host="qdrant-server" \
     -e OpenAIConfig__Token="sk-your-key" \
     rag-api:latest
   ```

3. **Use Managed Services (Recommended)**
   - **Database**: Azure SQL, AWS RDS
   - **Vector DB**: Qdrant Cloud (https://cloud.qdrant.io/)
   - **API**: Azure App Service, AWS ECS, Kubernetes

## Performance Optimization

### Qdrant Optimization

**Indexing:**
```bash
# Create HNSW index for faster search
curl -X PUT "http://localhost:6333/collections/rag_documents" \
  -H "Content-Type: application/json" \
  -d '{
    "hnsw_config": {
      "m": 16,
      "ef_construct": 100
    }
  }'
```

**Quantization** (reduce memory usage):
```bash
# Enable scalar quantization
curl -X PUT "http://localhost:6333/collections/rag_documents" \
  -H "Content-Type: application/json" \
  -d '{
    "quantization_config": {
      "scalar": {
        "type": "int8",
        "quantile": 0.99
      }
    }
  }'
```

### Caching

**Add response caching** for frequent queries:

```csharp
[HttpPost("search-factorx")]
[ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "userQuery", "limit" })]
public async Task<IActionResult> SearchBySemanticFactorX([FromBody] SearchBySemanticFactorXQuery query)
{
    // ...
}
```

### Batch Processing

Process large document sets efficiently:

```bash
# Process 100 files in parallel
for file in docs/*.md; do
  curl -X POST "http://localhost:5116/v1/rag/process-file" \
    -F "file=@$file" &
done
wait
```

## Monitoring & Observability

### Application Insights (Azure)

Configure in `appsettings.json`:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-key-here"
  }
}
```

**Key Metrics to Track:**
- Query latency (p50, p95, p99)
- Chunks retrieved per query
- Expansion ratio (final chunks / initial matches)
- OpenAI API latency and errors
- Qdrant search latency

### Health Checks

```csharp
// Already configured in Program.cs
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
```

**Check Dependencies:**
```bash
curl http://localhost:5116/health
# Returns: Healthy if all dependencies are reachable
```

## Troubleshooting

### Qdrant Connection Issues

**Error:** "Failed to connect to Qdrant"

**Solution:**
```bash
# Verify Qdrant is running
curl http://localhost:6333/collections

# Check logs
docker logs <qdrant-container-id>

# Restart Qdrant
docker restart <qdrant-container-id>
```

### OpenAI Rate Limits

**Error:** "Rate limit exceeded"

**Solution:**
- Implement exponential backoff (already included in OpenAIService)
- Use OpenAI's batch API for bulk processing
- Consider caching embeddings for duplicate content

### Search Returns Empty Results

**Checklist:**
1. Verify documents are processed: `GET /v1/rag/file/{fileName}`
2. Check access level: Unauthenticated users can't see level 1+ docs
3. Test with direct Qdrant query:
   ```bash
   curl -X POST "http://localhost:6333/collections/rag_documents/points/search" \
     -H "Content-Type: application/json" \
     -d '{"vector": [...], "limit": 10}'
   ```

## Roadmap

### Planned Features

- [ ] **Generation Layer**: Add LLM-based answer synthesis
- [ ] **Hybrid Search**: Combine Factor X (semantic) with BM25 (keyword)
- [ ] **Semantic Chunking**: Parse markdown structure (headings, sections)
- [ ] **Citation Tracking**: Return chunk IDs with answers for verification
- [ ] **Query Rewriting**: HyDE, query expansion, step-back prompting
- [ ] **Reranking**: Cross-encoder reranking for expanded results
- [ ] **Multi-modal Support**: PDF, DOCX, HTML parsing
- [ ] **Evaluation Framework**: Precision@K, Recall@K, F1 metrics
- [ ] **Conversational Interface**: Chat history, context retention

### Research Ideas

- Adaptive FactorX based on query type
- Cluster-aware expansion (stop at topic boundaries)
- User feedback for retrieval quality (relevance labels)
- A/B testing framework for algorithm tuning

## Contributing

Contributions are welcome! Please follow these guidelines:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Follow** existing patterns:
   - CQRS structure for new features
   - Result pattern for error handling
   - FluentValidation for commands
4. **Write** unit tests for new functionality
5. **Update** documentation (README, CLAUDE.md)
6. **Commit** with clear messages (`git commit -m 'Add semantic chunking'`)
7. **Push** to your branch (`git push origin feature/amazing-feature`)
8. **Open** a Pull Request

### Code Style

- Follow C# coding conventions
- Use async/await throughout
- Add XML documentation for public APIs
- Keep methods focused and testable

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- **Factor X Algorithm**: Original contribution by this project
- **Vector Database**: [Qdrant](https://qdrant.tech/) for high-performance similarity search
- **Embeddings**: [OpenAI](https://platform.openai.com/) for state-of-the-art text embeddings
- **CQRS Pattern**: [MediatR](https://github.com/jbogard/MediatR) by Jimmy Bogard
- **Validation**: [FluentValidation](https://fluentvalidation.net/) by Jeremy Skinner
- **Framework**: [ASP.NET Core](https://docs.microsoft.com/aspnet/core) by Microsoft

## Citation

If you use this work in research or production, please cite:

```bibtex
@software{mnemosyne_x,
  title={MNEMOSYNE-X: Memory with the X Factor},
  author={Daniel Gregatto},
  year={2025},
  url={https://github.com/DanielGregatto/mnemosyne-x}
}
```

---

**Built with ❤️ for the AI/ML and .NET communities.**

**Questions?** Open an issue or reach out on GitHub Discussions.

**Found this helpful?** Give it a ⭐ and share with your network!
