namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Serilog.Exceptions.Core;

    public class TaskCanceledExceptionDestructurer : OperationCanceledExceptionDestructurer
    {
        private static readonly Type[] TargetExceptionTypes =
        {
            typeof(TaskCanceledException)
        };

        public override Type[] TargetTypes => TargetExceptionTypes;

        public override void Destructure(Exception exception, IExceptionPropertiesBag propertiesBag, Func<Exception, IReadOnlyDictionary<string, object>> innerDestructure)
        {
            var tce = (TaskCanceledException)exception;
            base.Destructure(exception, propertiesBag, innerDestructure);
            propertiesBag.AddProperty(nameof(TaskCanceledException.Task), DestructureTask(tce.Task, innerDestructure));
        }

        internal static object DestructureTask(Task task, Func<Exception, IReadOnlyDictionary<string, object>> innerDestructure)
        {
            if (task == null)
            {
                return "null";
            }

            var taskProperties = new SortedList<string, object>(task.Exception != null ? 4 : 3)
            {
                [nameof(Task.Id)] = task.Id,
                [nameof(Task.Status)] = task.Status.ToString("G"),
                [nameof(Task.CreationOptions)] = task.CreationOptions.ToString("F")
            };

            if (task.IsFaulted)
            {
                taskProperties[nameof(Task.Exception)] = innerDestructure(task.Exception);
            }

            return taskProperties;
        }
    }
}