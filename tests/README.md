# Veriflow 3.0 - Test Suite

## Overview
Comprehensive test suite for Veriflow 3.0 professional video/audio workflow application.

## Test Structure

```
tests/
└── Veriflow.Tests/
    ├── Services/
    │   ├── SessionServiceTests.cs
    │   └── OffloadServiceTests.cs
    ├── Integration/
    │   └── TranscodeEngineTests.cs
    └── Models/
        └── ModelTests.cs
```

## Running Tests

### All Tests
```bash
dotnet test
```

### Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~SessionServiceTests"
```

### With Coverage
```bash
dotnet test /p:CollectCoverage=true
```

## Test Categories

### Unit Tests
- **SessionService**: Session creation, save/load
- **OffloadService**: Job creation, checksum validation
- **Models**: Data model validation

### Integration Tests
- **TranscodeEngine**: Preset management
- **MediaService**: FFmpeg integration (requires FFmpeg)
- **SyncEngine**: Waveform correlation (requires FFmpeg)

## Test Coverage Goals
- **Core Services**: 80%+
- **Models**: 90%+
- **Infrastructure**: 70%+

## Dependencies
- xUnit
- FluentAssertions
- Moq

## CI/CD Integration
Tests run automatically on:
- Pull requests
- Main branch commits
- Release tags
