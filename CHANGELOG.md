# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
- New recommended fluent API for destructuring process configuration based on `DestructuringOptionsBuilder`
- Ability to configure global exception properties filter
- Ability to configure root property name of destructured exception
- Ability to configure depth of reflection based destructuring
- Benchmark project for performance measurments
- Documentation comments for the whole public API

### Changed
- `Uri` objects are destructured to plain strings instead of dictionaries
- Adjusted examples using `Serilog.RollingFile` to updated API
- Reflection destructurer caches `PropertyInfo` for each exception type
- `WithExceptionDetails` overload with no parameters, sets default filter ignoring `StackTrace` and `TargetSite`
- `IExceptionDestructurer` interface changed, instead of `Dictionary` it accepts `IExceptionPropertiesBag`

### Fixed
- Appveyor build

### Removed
- `LoggerEnrichmentConfiguration` not used extension methods: `WithProperties` and `WithLazyProperties`

### Deprecated 
- Configuration of destructurers using a raw destructurers list, usage of new fluent API is recommended

## [3.0.0] - 2017-11-17

### Added
- netcoreapp2.0 target

### Fixed
- Fix adding exception type description if "Type" property is present

### Change
- Move SqlExceptionDestructurer to Serilog.Exceptions.SqlServer assembly

### Remove
- Remove System.Data types destructurer

## [2.5.0] - 2017-08-20

### Fixed
- Fix destructuing of cyclic structures

### Change
- Update Serilog to 2.5.0

[Unreleased]: https://github.com/RehanSaeed/Serilog.Exceptions/compare/Serilog.Exceptions.3.0.0...HEAD
[3.0.0]: https://github.com/RehanSaeed/Serilog.Exceptions/compare/Serilog.Exceptions.2.5.0...Serilog.Exceptions.3.0.0
[2.5.0]: https://github.com/RehanSaeed/Serilog.Exceptions/compare/2.4.1...Serilog.Exceptions.2.5.0