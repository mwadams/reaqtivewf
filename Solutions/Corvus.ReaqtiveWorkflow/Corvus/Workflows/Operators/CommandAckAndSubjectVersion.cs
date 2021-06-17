// <copyright file="CommandAckAndSubjectVersion.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Workflows.Operators
{
    using Corvus.Commands;

    /// <summary>
    /// A tuple of <see cref="CommandAck"/> and <see cref="WorkflowSubjectVersion"/>.
    /// </summary>
    internal class CommandAckAndSubjectVersion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAckAndSubjectVersion"/> class.
        /// </summary>
        /// <param name="commandAck">The command ack.</param>
        /// <param name="subjectVersion">The workflow subject version.</param>
        public CommandAckAndSubjectVersion(CommandAck commandAck, WorkflowSubjectVersion subjectVersion)
        {
            this.CommandAck = commandAck;
            this.SubjectVersion = subjectVersion;
        }

        /// <summary>
        /// Gets the command ack.
        /// </summary>
        public CommandAck CommandAck { get; init; }

        /// <summary>
        /// Gets the subject version.
        /// </summary>
        public WorkflowSubjectVersion SubjectVersion { get; init; }
    }
}