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

            if (task.IsFaulted && task.Exception != null)
            {
                return new
                {
                    Id = task.Id,
                    Status = task.Status.ToString("G"),
                    CreationOptions = task.CreationOptions.ToString("F"),
                    Exception = innerDestructure(task.Exception),
                };
            }

            return new
            {
                Id = task.Id,
                Status = task.Status.ToString("G"),
                CreationOptions = task.CreationOptions.ToString("F"),
            };
        }
    }
}