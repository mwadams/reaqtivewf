// <copyright file="CommandContext.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Workflows
{
    using Corvus.Commands;

    /// <summary>
    /// A wrapper for a <see cref="WorkflowSubjectVersion.Context"/> that attaches the <see cref="Command.Id"/> to
    /// the context built by the <see cref="Transition.ContextFactory"/>.
    /// </summary>
    /// <remarks>
    /// This is created during <see cref="Workflow.TryApplyTrigger"/>, and unwrapped by <see cref="Workflow.ApplyCommandAck(WorkflowSubjectVersion, Commands.CommandAck)"/>.
    /// </remarks>
    public sealed class CommandContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandContext"/> class.
        /// </summary>
        /// <param name="commandId">The <see cref="Command.Id"/> of the related command.</param>
        /// <param name="context">The context to wrap.</param>
        public CommandContext(string commandId, object context)
        {
            this.CommandId = commandId;
            this.Context = context;
        }

        /// <summary>
        /// Gets the context being wrapped.
        /// </summary>
        public object Context { get; init; }

        /// <summary>
        /// Gets the <see cref="Command.Id"/> of the related command.
        /// </summary>
        public string CommandId { get; }
    }
}
