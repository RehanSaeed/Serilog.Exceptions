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

            string taskStatus = task.Status.ToString("G");
            string taskCreationOptions = task.CreationOptions.ToString("F");

            if (task.IsFaulted && task.Exception != null)
            {
                return new
                {
                    Id = task.Id,
                    Status = taskStatus,
                    CreationOptions = taskCreationOptions,
                    Exception = innerDestructure(task.Exception),
                };
            }

            return new
            {
                Id = task.Id,
                Status = taskStatus,
                CreationOptions = taskCreationOptions,
            };
        }
    }
}