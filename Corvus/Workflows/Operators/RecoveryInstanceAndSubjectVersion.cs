// <copyright file="RecoveryInstanceAndSubjectVersion.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Workflows.Operators
{
    /// <summary>
    /// A tuple of <see cref="WorkflowSubjectRecoveryInstance"/> and <see cref="WorkflowSubjectVersion"/>.
    /// </summary>
    internal class RecoveryInstanceAndSubjectVersion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryInstanceAndSubjectVersion"/> class.
        /// </summary>
        /// <param name="recoveryInstance">The command ack.</param>
        /// <param name="subjectVersion">The workflow subject version.</param>
        public RecoveryInstanceAndSubjectVersion(WorkflowSubjectRecoveryInstance recoveryInstance, WorkflowSubjectVersion subjectVersion)
        {
            this.RecoveryInstance = recoveryInstance;
            this.SubjectVersion = subjectVersion;
        }

        /// <summary>
        /// Gets the recovery instance.
        /// </summary>
        public WorkflowSubjectRecoveryInstance RecoveryInstance { get; init; }

        /// <summary>
        /// Gets the current subject version.
        /// </summary>
        public WorkflowSubjectVersion SubjectVersion { get; init; }
    }
}