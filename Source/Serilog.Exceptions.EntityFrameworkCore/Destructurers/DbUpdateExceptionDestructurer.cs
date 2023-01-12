namespace Serilog.Exceptions.EntityFrameworkCore.Destructurers;

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
    private readonly int? _entryCountLimit;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbUpdateExceptionDestructurer"/> class.
        /// </summary>
        /// <param name="entryCountLimit">Limit of how many entries will be emitted. Null for unlimited.</param>
        public DbUpdateExceptionDestructurer(int? entryCountLimit = null)
        {
            _entryCountLimit = entryCountLimit;
        }

        /// <inheritdoc />
        public override Type[] TargetTypes => new[]
                                              {
                                                  typeof(DbUpdateException),
                                                  typeof(DbUpdateConcurrencyException)
                                              };

        /// <inheritdoc />
        public override void Destructure(
            Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object?>?> destructureException
        )
        {
            base.Destructure(exception, propertiesBag, destructureException);

            var dbUpdateException = (DbUpdateException)exception;

            if (dbUpdateException.Entries != null)
            {
                propertiesBag.AddProperty("EntryCount", dbUpdateException.Entries.Count);

                var entriesQuery = dbUpdateException.Entries
                                                    .Select(
                                                            e => new
                                                                 {
                                                                     EntryProperties = e.Properties.Select(
                                                                                                           p => new
                                                                                                                {
                                                                                                                    PropertyName = p.Metadata.Name,
                                                                                                                    p.OriginalValue,
                                                                                                                    p.CurrentValue,
                                                                                                                    p.IsTemporary,
                                                                                                                    p.IsModified,
                                                                                                                }),
                                                                     e.State,
                                                                 });

                if (_entryCountLimit != null)
                {
                    entriesQuery = entriesQuery.Take(_entryCountLimit.Value);
                }

                propertiesBag.AddProperty(nameof(DbUpdateException.Entries), entriesQuery.ToList());
            }
        }
}
