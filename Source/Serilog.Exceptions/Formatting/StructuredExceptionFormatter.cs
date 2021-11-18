namespace Serilog.Exceptions.Formatting;

using System;
using System.IO;
using Serilog.Events;
using Serilog.Exceptions.Core;
using Serilog.Formatting;
using Serilog.Formatting.Json;

/// <summary>
/// A JSON text formatter using structured properties for exceptions.
/// </summary>
/// <remarks>
/// Avoids the redundancy of <see cref="JsonFormatter"/> when used with <see cref="ExceptionEnricher"/>.
/// </remarks>
public class StructuredExceptionFormatter : ITextFormatter
{
    private readonly string rootName;
    private readonly JsonValueFormatter valueFormatter;

    /// <summary>
    /// Initializes a new instance of the <see cref="StructuredExceptionFormatter"/> class.
    /// </summary>
    /// <param name="rootName">The root name used by the enricher, if different from the default.</param>
    /// <param name="valueFormatter">A custom JSON formatter to use for underlying properties, if any.</param>
    public StructuredExceptionFormatter(string? rootName = null, JsonValueFormatter? valueFormatter = null)
    {
        this.rootName = rootName ?? new DestructuringOptionsBuilder().RootName;
        this.valueFormatter = valueFormatter ?? new();
    }

    /// <inheritdoc />
    public void Format(LogEvent logEvent, TextWriter output)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(output);
#else
        if (logEvent is null)
        {
            throw new ArgumentNullException(nameof(logEvent));
        }

        if (output is null)
        {
            throw new ArgumentNullException(nameof(output));
        }
#endif

        output.Write("{\"Timestamp\":\"");
        output.Write(logEvent.Timestamp.UtcDateTime.ToString("O"));

        output.Write("\",\"Message\":");
        var message = logEvent.MessageTemplate.Render(logEvent.Properties);
        JsonValueFormatter.WriteQuotedJsonString(message, output);

        output.Write(",\"Level\":\"");
        output.Write(logEvent.Level);
        output.Write('\"');

        var propCount = logEvent.Properties.Count;

        if (logEvent.Properties.TryGetValue(this.rootName, out var exceptionProperty))
        {
            output.Write(",\"Exception\":");
            this.valueFormatter.Format(exceptionProperty, output);
            propCount--;
        }

        if (propCount > 0)
        {
            output.Write(",\"Properties\":{");
            var comma = false;

            foreach (var property in logEvent.Properties)
            {
                if (property.Key == this.rootName)
                {
                    continue;
                }

                if (comma)
                {
                    output.Write(',');
                }
                else
                {
                    comma = true;
                }

                JsonValueFormatter.WriteQuotedJsonString(property.Key, output);
                output.Write(':');
                this.valueFormatter.Format(property.Value, output);
            }

            output.Write("}");
        }

        output.WriteLine('}');
    }
}
