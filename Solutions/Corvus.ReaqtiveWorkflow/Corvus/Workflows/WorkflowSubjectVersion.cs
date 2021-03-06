// <copyright file="WorkflowSubjectVersion.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Workflows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A version of a workflow subject.
    /// </summary>
    /// <remarks>
    /// <para>
    /// We track/drive instances of "workflow subjects" through their lifecycle defined by a <see cref="Workflow"/>.
    /// </para>
    /// <para>
    /// The current state of a particular workflow subject is recorded as a <see cref="WorkflowSubjectVersion"/>.
    /// </para>
    /// </remarks>
    public sealed class WorkflowSubjectVersion
    {
        private readonly HashSet<Uri> triggerTypes;
        private readonly HashSet<string> interests;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowSubjectVersion"/> class.
        /// </summary>
        /// <param name="id">The id of the workflow subject.</param>
        /// <param name="sequenceNumber">The monotonically increasing sequence number of the workflow subject version.</param>
        /// <param name="stateId">The state ID of the state of the workflow subject at this version.</param>
        /// <param name="triggerTypes">The trigger types to which this workflow subject version can respond.</param>
        /// <param name="interests">The interests of the workflow subject at this version.</param>
        /// <param name="status">The <see cref="WorkflowSubjectStatus"/> of the workflow subject at this version.</param>
        /// <param name="triggerSequenceNumber">The sequence number of the <see cref="Trigger"/> that last caused the last update to the <see cref="StateId"/> in this workflow subject version.</param>
        /// <param name="context">Context information for the workflow subject version.</param>
        public WorkflowSubjectVersion(string id, long sequenceNumber, string stateId, IEnumerable<Uri> triggerTypes, IEnumerable<string> interests, WorkflowSubjectStatus status, long triggerSequenceNumber, object context)
        {
            this.Id = id;
            this.SequenceNumber = sequenceNumber;
            this.StateId = stateId;
            this.interests = new HashSet<string>(interests);
            this.triggerTypes = new HashSet<Uri>(triggerTypes);
            this.Status = status;
            this.TriggerSequenceNumber = triggerSequenceNumber;
            this.Context = context;
        }

        /// <summary>
        /// Gets the ID of the workflow subject.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Gets the state ID of the state of the workflow subject at this version.
        /// </summary>
        public string StateId { get; init; }

        /// <summary>
        /// Gets the <see cref="WorkflowSubjectStatus"/> of the workflow subject at this version.
        /// </summary>
        public WorkflowSubjectStatus Status { get; init; }

        /// <summary>
        /// Gets the monotonically increasing sequence number of this workflow subject version.
        /// </summary>
        public long SequenceNumber { get; init; }

        /// <summary>
        /// Gets the sequence number of the last trigger that applied to the subject to put it into the current state.
        /// </summary>
        /// <remarks>Note that this is not monotonically increasing for this workflow subject. You will typically have several workflow subject versions that
        /// have the same trigger sequence number for the workflow subject version instances created in response to the internal state
        /// changes of the engine (e.g. waiting for trigger [tseq:1, seq: 1] -> {Trigger [tseq:2]} -> waiting for acks [tseq:2, seq: 2]-> {Acks arrive} -> waiting for trigger [tseq: 2, seq: 3]).
        /// </remarks>
        public long TriggerSequenceNumber { get; init; }

        /// <summary>
        /// Gets the custom context metadata for this version.
        /// </summary>
        public object Context { get; init;  }

        /// <summary>
        /// Gets the interests of the workflow subject at this version.
        /// </summary>
        internal IEnumerable<string> Interests => this.interests;

        /// <summary>
        /// Gets the trigger types to which the workflow subject will respond at this version.
        /// </summary>
        internal IEnumerable<Uri> TriggerTypes => this.triggerTypes;

        /// <summary>
        /// Determines if the trigger applies this instance.
        /// </summary>
        /// <param name="trigger">The trigger to test.</param>
        /// <returns><see langword="true"/> if the trigger applies to the topic.</returns>
        public bool ShouldApply(Trigger trigger)
        {
            return this.MatchesTriggerType(trigger.Type) && trigger.Topics.Any(topic => this.MatchesTopic(topic));
        }

        /// <summary>
        /// Gets a value indicating whether this version matches the given trigger type.
        /// </summary>
        /// <param name="triggerType">The trigger type to match.</param>
        /// <returns><see langword="true"/> if the subject version contains the given trigger type.</returns>
        private bool MatchesTriggerType(Uri triggerType)
        {
            return this.triggerTypes.Contains(triggerType);
        }

        /// <summary>
        /// Gets a value indicating whether this version has an interest matching the given topic.
        /// </summary>
        /// <param name="topic">The topic to match.</param>
        /// <returns><see langword="true"/> if the subject version has an interest for the given topic.</returns>
        private bool MatchesTopic(string topic)
        {
            return this.interests.Contains(topic);
        }
    }
}