// <copyright file="TriggerAndSubjectVersion.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Workflows.Operators
{
    /// <summary>
    /// A tuple of <see cref="Trigger"/> and <see cref="WorkflowSubjectVersion"/>.
    /// </summary>
    internal class TriggerAndSubjectVersion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerAndSubjectVersion"/> class.
        /// </summary>
        /// <param name="trigger">The trigger.</param>
        /// <param name="subjectVersion">The workflow subject version.</param>
        public TriggerAndSubjectVersion(Trigger trigger, WorkflowSubjectVersion subjectVersion)
        {
            this.Trigger = trigger;
            this.SubjectVersion = subjectVersion;
        }

        /// <summary>
        /// Gets the trigger.
        /// </summary>
        public Trigger Trigger { get; init; }

        /// <summary>
        /// Gets the subject version.
        /// </summary>
        public WorkflowSubjectVersion SubjectVersion { get; init; }
    }
}