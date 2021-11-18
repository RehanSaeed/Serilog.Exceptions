namespace Serilog.Exceptions.Test.Formatting;

using System;
using System.IO;
using FluentAssertions.Execution;
using Serilog.Events;
using Serilog.Exceptions.Formatting;
using Serilog.Parsing;
using Xunit;

public class StructuredExceptionFormatterTest
{
    [Fact]
    public void Format_EventNoProperties_CorrectJson() =>
        Format_CorrectJson(
            "{\"Timestamp\":\"2021-12-01T13:15:00.0000000Z\",\"Message\":\"Hello!\",\"Level\":\"Debug\"}",
            new DateTimeOffset(2021, 12, 1, 12, 15, 0, 0, TimeSpan.FromHours(-1)),
            LogEventLevel.Debug,
            "Hello!");

    [Fact]
    public void Format_EventWithProperties_CorrectJson() =>
        Format_CorrectJson(
            "{\"Timestamp\":\"1999-01-01T04:15:12.0550000Z\",\"Message\":\"Hello, \\\"Kathy\\\"!\",\"Level\":\"Information\",\"Properties\":{\"Person\":\"Kathy\",\"Extra\":[\"more\",\"data\"]}}",
            new DateTimeOffset(1999, 1, 1, 4, 15, 12, 55, TimeSpan.Zero),
            LogEventLevel.Information,
            "Hello, {Person}!",
            new("Person", new ScalarValue("Kathy")),
            new("Extra", new SequenceValue(new ScalarValue[] { new("more"), new("data") })));

    [Fact]
    public void Format_EventWithException_CorrectJson() =>
        Format_CorrectJson(
            "{\"Timestamp\":\"2001-01-01T22:30:12.0000000Z\",\"Message\":\"Uh, oh!\",\"Level\":\"Error\",\"Exception\":{\"Message\":\"Bad stuff\",\"HResult\":1234}}",
            new DateTimeOffset(2001, 1, 2, 1, 30, 12, 0, TimeSpan.FromHours(3)),
            LogEventLevel.Error,
            "Uh, oh!",
            new LogEventProperty("ExceptionDetail", new StructureValue(new LogEventProperty[] { new("Message", new ScalarValue("Bad stuff")), new("HResult", new ScalarValue(1234)) })));

    private static void Format_CorrectJson(
        string expected,
        DateTimeOffset timestamp,
        LogEventLevel level,
        string template,
        params LogEventProperty[] properties)
    {
        using var output = new StringWriter();
        var ev = new LogEvent(timestamp, level, null, new MessageTemplateParser().Parse(template), properties);

        new StructuredExceptionFormatter().Format(ev, output);
        Assert.Equal(expected + Environment.NewLine, output.ToString());
    }
}
