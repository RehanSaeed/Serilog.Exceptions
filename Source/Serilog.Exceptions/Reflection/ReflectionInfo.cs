namespace Serilog.Exceptions.Reflection;

/// <summary>
/// Contains metadata information about a type
/// useful for destructuring process.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ReflectionInfo"/> class.
/// </remarks>
/// <param name="properties">All properties for a type.</param>
/// <param name="propertiesExceptBaseOnes">All properties except of <see cref="Exception"/> properties which are handled separately.</param>
internal class ReflectionInfo(
    ReflectionPropertyInfo[] properties,
    ReflectionPropertyInfo[] propertiesExceptBaseOnes)
{
    /// <summary>
    /// Gets information about all properties to be destructured.
    /// </summary>
    public ReflectionPropertyInfo[] Properties { get; } = properties;

    /// <summary>
    /// Gets information about properties other than base exception properties
    /// which are handled separately.
    /// </summary>
    public ReflectionPropertyInfo[] PropertiesExceptBaseOnes { get; } = propertiesExceptBaseOnes;
}
