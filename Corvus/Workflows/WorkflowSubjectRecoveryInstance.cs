// <copyright file="WorkflowSubjectRecoveryInstance.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Workflows
{
    /// <summary>
    /// A type that puts a <see cref="WorkflowSubjectVersion"/> whose <see cref="WorkflowSubjectVersion.Status"/> is
    /// <see cref="WorkflowSubjectStatus.Faulted"/> explicitly another state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The system will apply this recovery version to the workflow subject if the sequence number of the
    /// <see cref="WorkflowSubjectVersion"/> is less than the sequence number of this recovery version. This protects
    /// you against applying a recovery version when the system has advanced the state of the subject for some other reason
    /// (e.g. an out-of-order execution of multiple recovery requests.)
    /// </para>
    /// <para>
    /// Because the new <see cref="WorkflowSubjectVersion"/> contains the <see cref="WorkflowSubjectVersion.TriggerSequenceNumber"/>
    /// that nominally applied it, you can control which <see cref="Trigger.SequenceNumber"/> to resume from. Just use <c>-1</c> if you want
    /// to resume from the "next" trigger.
    /// </para>
    /// </remarks>
    public class WorkflowSubjectRecoveryInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowSubjectRecoveryInstance"/> class.
        /// </summary>
        /// <param name="id">The id of the recovery request.</param>
        /// <param name="version">The new workflow subject version.</param>
        public WorkflowSubjectRecoveryInstance(string id, WorkflowSubjectVersion version)
        {
            this.Id = id;
            this.Version = version;
        }

        /// <summary>
        /// Gets the ID of the recovery request.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Gets the new <see cref="WorkflowSubjectVersion"/> for the subject.
        /// </summary>
        public WorkflowSubjectVersion Version { get; init; }
    }
}
