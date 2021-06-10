namespace Corvus.Workflows
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// A version of a workflow subject.
    /// </summary>
    /// <remarks>
    /// This represents the current state of a workflow subject being driven by a <see cref="Workflow" />.
    /// </remarks>
    public class WorkflowSubjectVersion
    {
        public WorkflowSubjectVersion(string id, long sequenceNumber, string stateId, IEnumerable<string> interests, WorkflowSubjectStatus status, long triggerSequenceNumber)
        {
            this.Id = id;
            this.SequenceNumber = sequenceNumber;
            this.StateId = stateId;
            this.Interests = interests.ToImmutableList();
            this.Status = status;
            this.TriggerSequenceNumber = triggerSequenceNumber;
        }

        public string Id { get; init; }

        public string StateId { get; init; }

        public IEnumerable<string> Interests { get; init; }

        public WorkflowSubjectStatus Status { get; init; }

        /// <summary>
        /// The monotonically increasing sequence number of this workflow subject version.
        /// </summary>
        public long SequenceNumber { get; init; }

        /// <summary>
        /// Gets the sequence number of the last trigger that applied to the subject to put it into the current state.
        /// </summary>
        /// <remarks>Note that this is not monotonically increasing for this workflow subject. You will typically have several workflow subject versions that
        /// have the same trigger sequence number for the workflow subject version instances created in response to the internal state
        /// changes of the engine (e.g. waiting for trigger [tseq:1, seq: 1] -> {Trigger [tseq:2]} -> waiting for acks [tseq:2, seq: 2]-> {Acks arrive} -> waiting for trigger [tseq: 2, seq: 3])
        /// </remarks>
        public long TriggerSequenceNumber { get; init; }
    }
}