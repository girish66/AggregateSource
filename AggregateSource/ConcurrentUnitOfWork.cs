﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AggregateSource {
  /// <summary>
  /// Tracks changes of attached aggregates.
  /// </summary>
  public class ConcurrentUnitOfWork {
    readonly ConcurrentDictionary<Guid, Aggregate> _aggregates;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    public ConcurrentUnitOfWork() {
      _aggregates = new ConcurrentDictionary<Guid, Aggregate>();
    }

    /// <summary>
    /// Attaches the specified aggregate.
    /// </summary>
    /// <param name="aggregate">The aggregate.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="aggregate"/> is null.</exception>
    public void Attach(Aggregate aggregate) {
      if (aggregate == null) throw new ArgumentNullException("aggregate");
      if (!_aggregates.TryAdd(aggregate.Id, aggregate)) 
        throw new AggregateSourceException(
          string.Format("Failed to add the aggregate with root of type {0}, identified by {1}, to the unit of work.", 
          aggregate.Root.GetType().Name, 
          aggregate.Id.ToString("N")));
    }

    /// <summary>
    /// Attempts to get the <see cref="Aggregate"/> using the specified aggregate id.
    /// </summary>
    /// <param name="id">The aggregate id.</param>
    /// <param name="aggregate">The aggregate if found, otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if the aggregate was found, otherwise <c>false</c>.</returns>
    public bool TryGet(Guid id, out Aggregate aggregate) {
      return _aggregates.TryGetValue(id, out aggregate);
    }

    /// <summary>
    /// Determines whether this instance has aggregates with state changes.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if this instance has aggregates with state changes; otherwise, <c>false</c>.
    /// </returns>
    public bool HasChanges() {
      return _aggregates.Values.Any(aggregate => aggregate.Root.HasChanges());
    }

    /// <summary>
    /// Gets the aggregates with state changes.
    /// </summary>
    /// <returns>An enumeration of <see cref="Aggregate"/>.</returns>
    public IEnumerable<Aggregate> GetChanges() {
      return _aggregates.Values.Where(aggregate => aggregate.Root.HasChanges());
    }
  }
}