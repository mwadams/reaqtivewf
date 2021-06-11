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
        /// <summary>
        /// Creates an instance of a <see cref="WorkflowSubjectVersion" />
        /// </summary>
        /// <param name="id">The id of the subject</param>
        /// <param name="sequenceNumber">The monotonically increasing sequence number of this version.</param>
        /// <param name="stateId">The <see cref="State.Id"/> of the current state of this workflow subject.</param>
        /// <param name="interests">The current interests of this workflow subject, which are mateched to the <see cref="Trigger.Topics"/> to determine if a trigger applies to this subject version.</param>
        /// <param name="status">The current status of this workflow subject, in the engine lifecycle. It is used to manage the subject through the asynchronous phases of transitions, and to fault the subject.</param>
        /// <param name="triggerSequenceNumber">The sequence numnber of the last trigger to cause a state change for this subject.</param>
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