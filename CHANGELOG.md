# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
- Ability to configure global exception properties filter
- Benchmark project for performance measurments

### Changed
- `Uri` objects are destructured to plain strings instead of dictionaries
- Adjusted examples using `Serilog.RollingFile` to updated API
- Reflection destructurer caches `PropertyInfo` for each exception type

### Fixed
- Appveyor build

[Unreleased]: https://github.com/RehanSaeed/Serilog.Exceptions/compare/Serilog.Exceptions.3.0.0...HEAD