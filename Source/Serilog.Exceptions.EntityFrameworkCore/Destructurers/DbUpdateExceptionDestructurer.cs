namespace Serilog.Exceptions.EntityFrameworkCore.Destructurers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Destructurers;

    /// <summary>
    /// A destructurer for <see cref="DbUpdateException"/>.
    /// </summary>
    /// <seealso cref="ExceptionDestructurer" />
    public class DbUpdateExceptionDestructurer : ExceptionDestructurer
    {
        /// <inheritdoc />
        public override Type[] TargetTypes => new[] { typeof(DbUpdateException), typeof(DbUpdateConcurrencyException) };

        /// <inheritdoc />
        public override void Destructure(
            Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException)
        {
            base.Destructure(exception, propertiesBag, destructureException);

#pragma warning disable CA1062 // Validate arguments of public methods
            var dbUpdateException = (DbUpdateException)exception;
            var entriesValue = dbUpdateException.Entries?.Select(e => new
            {
                EntryProperties = e.Properties.Select(p => new
                {
                    PropertyName = p.Metadata.Name,
                    p.OriginalValue,
                    p.CurrentValue,
                    p.IsTemporary,
                    p.IsModified
                }),
                e.State
            }).ToList();
            propertiesBag.AddProperty(nameof(DbUpdateException.Entries), entriesValue);
#pragma warning restore CA1062 // Validate arguments of public methods
        }
    }
}