// <copyright file="Trigger.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Workflows
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// An external trigger which will be applied to zero or more
    /// workflow subjects to drive them through their <see cref="Workflow" />.
    /// </summary>
    /// <remarks>
    /// The workflow engine will match the <see cref="Topics" /> in the trigger
    /// with the <see cref="WorkflowSubjectVersion.Interests" />. If there is any.
    /// intersection between those two lists, then the trigger is a candidate to be
    /// applied to the workflow subject.
    /// </remarks>
    public class Trigger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Trigger"/> class.
        /// </summary>
        /// <param name="id">The ID of the trigger.</param>
        /// <param name="sequenceNumber">The sequence number of the trigger.</param>
        /// <param name="topics">The topics of the trigger.</param>
        public Trigger(string id, long sequenceNumber, IEnumerable<string> topics)
        {
            this.Id = id;
            this.SequenceNumber = sequenceNumber;
            this.Topics = topics.ToImmutableArray();
        }

        /// <summary>
        /// Gets the unique Id of this trigger.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Gets the monotonically increasing sequence number of this trigger.
        /// </summary>
        public long SequenceNumber { get; init; }

        /// <summary>
        /// Gets the topics of this trigger.
        /// </summary>
        /// <remarks>
        /// These are matched with the <see cref="WorkflowSubjectVersion.Interests" /> to determine
        /// if this trigger should be applied to this interest.
        /// </remarks>
        public IEnumerable<string> Topics { get; init; }
    }
}