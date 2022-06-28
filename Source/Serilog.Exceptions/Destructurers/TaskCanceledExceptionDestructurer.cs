namespace Serilog.Exceptions.Destructurers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog.Exceptions.Core;

/// <summary>
/// Destructurer for <see cref="TaskCanceledException"/>.
/// </summary>
public class TaskCanceledExceptionDestructurer : OperationCanceledExceptionDestructurer
{
    private static readonly Type[] TargetExceptionTypes =
    {
        typeof(TaskCanceledException),
    };

    /// <inheritdoc cref="IExceptionDestructurer.TargetTypes"/>
    public override Type[] TargetTypes => TargetExceptionTypes;

    /// <inheritdoc cref="IExceptionDestructurer.Destructure"/>
    public override void Destructure(
        Exception exception,
        IExceptionPropertiesBag propertiesBag,
        Func<Exception, IReadOnlyDictionary<string, object?>?> destructureException)
    {
#pragma warning disable CA1062 // Validate arguments of public methods
        base.Destructure(exception, propertiesBag, destructureException);

        var taskCancelledException = (TaskCanceledException)exception;
        propertiesBag.AddProperty(
            nameof(TaskCanceledException.Task),
            DestructureTask(taskCancelledException.Task, destructureException));
#pragma warning restore CA1062 // Validate arguments of public methods
    }

    /// <summary>
    /// Destructures the specified task.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <param name="innerDestructure">The inner destructure.</param>
    /// <returns>The destructured task.</returns>
    internal static object? DestructureTask(
        Task? task,
        Func<Exception, IReadOnlyDictionary<string, object?>?> innerDestructure)
    {
        if (task is null)
        {
            return null;
        }

        var taskStatus = task.Status.ToString("G");
        var taskCreationOptions = task.CreationOptions.ToString("F");

        if (task.IsFaulted && task.Exception is not null)
        {
            return new
            {
                task.Id,
                Status = taskStatus,
                CreationOptions = taskCreationOptions,
                Exception = innerDestructure(task.Exception),
            };
        }

        return new
        {
            task.Id,
            Status = taskStatus,
            CreationOptions = taskCreationOptions,
        };
    }
}
