# Contributing to CQRS API Boilerplate

First off, thank you for considering contributing to CQRS API Boilerplate! It's people like you that make this project a great tool for the .NET community.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Setup](#development-setup)
- [Coding Standards](#coding-standards)
- [Commit Guidelines](#commit-guidelines)
- [Pull Request Process](#pull-request-process)
- [Project Architecture](#project-architecture)

## Code of Conduct

This project and everyone participating in it is governed by respect and professionalism. By participating, you are expected to uphold this standard. Please be respectful, constructive, and collaborative.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the existing issues to avoid duplicates. When creating a bug report, include as many details as possible:

- **Use a clear and descriptive title**
- **Describe the exact steps to reproduce the problem**
- **Provide specific examples** (code snippets, configuration files)
- **Describe the behavior you observed** and what you expected
- **Include .NET version, OS, and other environment details**

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion:

- **Use a clear and descriptive title**
- **Provide a detailed description** of the suggested enhancement
- **Explain why this enhancement would be useful** to most users
- **List any alternative solutions** you've considered

### Contributing Code

#### Good First Issues

Look for issues labeled `good first issue` - these are great starting points for new contributors.

#### Types of Contributions We're Looking For

- Bug fixes
- New features (after discussion in an issue)
- Documentation improvements
- Performance improvements
- Test coverage improvements
- Refactoring for better code quality

## Development Setup

### Prerequisites

- .NET 8.0 SDK or later
- SQL Server (LocalDB, Express, or Full)
- Git
- Your favorite IDE (Visual Studio, VS Code, Rider)

### Getting Started

1. **Fork the repository** on GitHub

2. **Clone your fork**
   ```bash
   git clone https://github.com/your-username/cqrs-api-boilerplate.git
   cd cqrs-api-boilerplate
   ```

3. **Add upstream remote**
   ```bash
   git remote add upstream https://github.com/original-owner/cqrs-api-boilerplate.git
   ```

4. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

5. **Install dependencies and build**
   ```bash
   dotnet restore
   dotnet build
   ```

6. **Set up database**
   ```bash
   cd src/UI.API
   dotnet ef database update --project ../Data
   ```

7. **Run the application**
   ```bash
   dotnet run
   ```

8. **Run tests**
   ```bash
   dotnet test
   ```

## Coding Standards

### General Guidelines

- Follow C# coding conventions and .NET best practices
- Write self-documenting code with clear variable and method names
- Add XML documentation comments for public APIs
- Keep methods small and focused (Single Responsibility Principle)
- Use async/await throughout the entire chain

### Project-Specific Patterns

#### CQRS Pattern

All features must follow the CQRS pattern:

**Commands (Write Operations):**
```csharp
// 1. Command class
public class CreateEntityCommand : IRequest<Result<EntityDto>>
{
    public string Name { get; set; }
}

// 2. Command handler
public class CreateEntityCommandHandler : BaseCommandHandler,
    IRequestHandler<CreateEntityCommand, Result<EntityDto>>
{
    // Constructor with dependencies

    public async Task<Result<EntityDto>> Handle(
        CreateEntityCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate
        // 2. Execute business logic
        // 3. Commit via UnitOfWork
        // 4. Return Result<T>
    }
}

// 3. Validator
public class CreateEntityCommandValidator : AbstractValidator<CreateEntityCommand>
{
    public CreateEntityCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}
```

**Queries (Read Operations):**
```csharp
// 1. Query class
public class GetEntityByIdQuery : IRequest<Result<EntityDto>>
{
    public Guid Id { get; set; }
}

// 2. Query handler
public class GetEntityByIdQueryHandler : BaseQueryHandler,
    IRequestHandler<GetEntityByIdQuery, Result<EntityDto>>
{
    // Read-only operations, return DTOs
}
```

#### Result Pattern

Always use the Result pattern for handler returns:

```csharp
// Success
return Result<EntityDto>.Success(dto);

// Validation failure
return Result<EntityDto>.ValidationFailure(errors);

// Not found
return Result<EntityDto>.NotFound("Entity not found");

// Unauthorized
return Result<EntityDto>.Unauthorized("Authentication required");
```

#### Validation

- Every command must have a FluentValidation validator
- Validators are automatically registered via DI
- Use meaningful error messages with localization support

#### Dependency Injection

- Register all services in `IoC/DIBootstrapper.cs`
- Use appropriate service lifetimes (Scoped, Transient, Singleton)
- Inject interfaces, not concrete classes

#### Database

- All entities inherit from `EntityBase<T>`
- Use soft deletes (set `Deleted = true`)
- Create entity mappings in `Data/Mappings/`
- Never call `SaveChangesAsync` directly - use `IUnitOfWork.CommitAsync()`

### Code Style

- Use 4 spaces for indentation (no tabs)
- Place opening braces on new lines
- Use meaningful names - avoid abbreviations
- One class per file
- Organize using statements (remove unused)

### Testing

- Write unit tests for all new features
- Follow AAA pattern (Arrange, Act, Assert)
- Use FluentAssertions for readable assertions
- Mock dependencies with Moq
- Aim for high test coverage on business logic

Example test:
```csharp
[Fact]
public async Task Handle_ValidCommand_ReturnsSuccess()
{
    // Arrange
    var command = new CreateStatusCommand { Nome = "Test" };

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Data.Should().NotBeNull();
}
```

## Commit Guidelines

### Commit Message Format

Follow the Conventional Commits specification:

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, no logic change)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks (dependencies, build process)

**Examples:**
```
feat(auth): add password reset functionality

Implemented password reset flow with email token validation.
Includes unit tests and documentation.

Closes #123
```

```
fix(validation): correct email validation regex

The previous regex didn't handle international domains correctly.
Updated to support all valid email formats per RFC 5322.
```

### Commit Best Practices

- Write clear, concise commit messages
- Use present tense ("add feature" not "added feature")
- Keep commits focused - one logical change per commit
- Reference issues and PRs when relevant

## Pull Request Process

### Before Submitting

1. **Update your fork**
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Ensure all tests pass**
   ```bash
   dotnet test
   ```

3. **Build succeeds with no warnings**
   ```bash
   dotnet build --no-incremental
   ```

4. **Code follows the style guidelines**

5. **Add/update tests** for your changes

6. **Update documentation** if needed (README, XML comments, etc.)

### Submitting the Pull Request

1. **Push to your fork**
   ```bash
   git push origin feature/your-feature-name
   ```

2. **Open a Pull Request** on GitHub

3. **Fill out the PR template** completely

4. **Link related issues** using keywords (Fixes #123, Closes #456)

5. **Request review** from maintainers

### PR Requirements

Your pull request must:

- [ ] Follow the coding standards outlined above
- [ ] Include tests for new functionality
- [ ] Have all tests passing
- [ ] Update documentation as needed
- [ ] Not break existing functionality
- [ ] Have a clear description of what changed and why

### Review Process

1. A maintainer will review your PR within a few days
2. Address any feedback or requested changes
3. Once approved, a maintainer will merge your PR
4. Your contribution will be included in the next release!

### After Your PR is Merged

- Delete your feature branch (GitHub can do this automatically)
- Update your local repository:
  ```bash
  git checkout main
  git pull upstream main
  ```

## Project Architecture

Understanding the architecture will help you contribute effectively.

### Clean Architecture Layers

1. **UI.API** (Presentation) - Controllers, middleware, startup configuration
2. **Services** (Application) - Commands, queries, handlers, validators
3. **Domain** (Domain) - Entities, DTOs, interfaces, business rules
4. **Domain.Core** (Core Domain) - Framework-agnostic base classes and events
5. **Data** (Infrastructure) - Database context, mappings, migrations
6. **Identity** (Infrastructure) - Authentication, authorization, JWT
7. **IoC** (Infrastructure) - Dependency injection configuration
8. **Util** (Infrastructure) - Cross-cutting utilities

### Dependency Rules

- Inner layers don't depend on outer layers
- Services depend on Domain, not on Data or Identity
- UI.API depends on Services, not directly on Data
- Domain.Core has zero external dependencies

### Feature Organization

Features are organized by domain concept in `Services/Features/{FeatureName}/`:

```
Auth/
  Commands/
    Login/
    Register/
    ForgotPassword/
  Queries/
    (if any)

Status/
  Commands/
    CreateStatus/
    UpdateStatus/
    DeleteStatus/
  Queries/
    GetAllStatus/
    GetStatusById/
```

Each command/query has its own folder with:
- Command/Query class
- Handler class
- Validator class (for commands)

## Questions?

Feel free to open an issue with the `question` label if you need help or clarification on anything!

## Recognition

Contributors will be recognized in the project's README. Thank you for helping make this project better!
