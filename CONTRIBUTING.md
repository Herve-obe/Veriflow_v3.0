# Contributing to Veriflow 3.0

Thank you for considering contributing to Veriflow! This document provides guidelines for contributing to the project.

## Code of Conduct

Be respectful, professional, and constructive in all interactions.

## How to Contribute

### Reporting Bugs

1. Check if the bug has already been reported
2. Use the bug report template
3. Include:
   - Steps to reproduce
   - Expected behavior
   - Actual behavior
   - System information
   - Screenshots/logs

### Suggesting Features

1. Check if the feature has been suggested
2. Use the feature request template
3. Describe:
   - Use case
   - Proposed solution
   - Alternatives considered

### Pull Requests

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Write/update tests
5. Update documentation
6. Submit PR

## Development Setup

```bash
# Clone your fork
git clone https://github.com/YOUR_USERNAME/Veriflow_v3.0.git

# Add upstream remote
git remote add upstream https://github.com/Herve-obe/Veriflow_v3.0.git

# Install dependencies
dotnet restore

# Build
dotnet build

# Run tests
dotnet test
```

## Coding Standards

- Follow C# conventions
- Use meaningful names
- Add XML documentation
- Write unit tests
- Keep methods small
- Avoid code duplication

## Commit Messages

Format: `<type>: <description>`

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `test`: Tests
- `refactor`: Code refactoring
- `style`: Formatting
- `chore`: Maintenance

Example:
```
feat: Add waveform synchronization
fix: Resolve checksum calculation bug
docs: Update user guide
```

## Testing

- Write tests for new features
- Maintain >75% code coverage
- Run all tests before submitting PR

## Documentation

- Update README if needed
- Add XML comments to public APIs
- Update user guide for UI changes
- Update developer guide for architecture changes

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
