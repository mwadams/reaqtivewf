// <copyright file="ApplyCommandAckExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Workflows.Operators
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Corvus.Commands;
    using Corvus.Workflows;
    using Reaqtive;

    /// <summary>
    /// Extension methods for command acknowledgements.
    /// </summary>
    internal static class ApplyCommandAckExtensions
    {
        /// <summary>
        /// Subscribe to a given <see cref="ISubscribable{WorkflowSubjectVersion}"/> source, while the <see cref="WorkflowSubjectVersion.Status"/> is <see cref="WorkflowSubjectStatus.WaitingForTransitionCommandAcks"/>,
        /// apply triggers to the waiting for triggers.
        /// </summary>
        /// <param name="workflowSubject">The workflow subject versions for this workflow subject.</param>
        /// <param name="getCommandAcksFromSourceIdAndCommandId">A function which provides an <see cref="ISubscribable{CommandAck}"/> from the given source ID and command ID.</param>
        /// <returns>A subscribable of <see cref="CommandAckAndSubjectVersion"/> for those <see cref="Trigger"/>s that apply to the current <see cref="WorkflowSubjectVersion"/>.</returns>
        public static ISubscribable<CommandAckAndSubjectVersion> ApplyCommandAcksToSubject(this ISubscribable<WorkflowSubjectVersion> workflowSubject, Func<string, string, ISubscribable<CommandAck>> getCommandAcksFromSourceIdAndCommandId)
        {
            return workflowSubject
                .Where(subjectVersion => subjectVersion.Status == WorkflowSubjectStatus.WaitingForTransitionCommandAcks)
                .SelectMany(subjectVersion => subjectVersion.ApplyCommandAcksToSubjectVersion(getCommandAcksFromSourceIdAndCommandId(subjectVersion.Id, GetCommandId(subjectVersion)), workflowSubject));
        }

        /// <summary>
        /// For a given <see cref="WorkflowSubjectVersion"/>, subscribe to the triggers until a new <see cref="WorkflowSubjectVersion"/> is produced in the
        /// <see cref="ISubscribable{WorkflowSubjectVersion}"/> source, filtering to those triggers whose <see cref="Trigger.Topics"/> intersect
        /// with the <see cref="WorkflowSubjectVersion.Interests"/>.
        /// </summary>
        /// <param name="subjectVersion">The current version of the workflow subject.</param>
        /// <param name="commandAcks">The triggers.</param>
        /// <param name="workflowSubject">The workflow subject versions for this workflow subject.</param>
        /// <returns>A subscribable of <see cref="TriggerAndSubjectVersion"/> for those <see cref="Trigger"/>s that apply to the current <see cref="WorkflowSubjectVersion"/>.</returns>
        public static ISubscribable<CommandAckAndSubjectVersion> ApplyCommandAcksToSubjectVersion(this WorkflowSubjectVersion subjectVersion, ISubscribable<CommandAck> commandAcks, ISubscribable<WorkflowSubjectVersion> workflowSubject)
        {
            return commandAcks
                .TakeUntil(workflowSubject)
                .Select(ack => new CommandAckAndSubjectVersion(ack, subjectVersion));
        }

        private static string GetCommandId(WorkflowSubjectVersion subjectVersion)
        {
            var commandContext = subjectVersion.Context as CommandContext;
            Debug.Assert(commandContext != null, $"The {nameof(subjectVersion.Context)} must be a {nameof(CommandContext)}");
            return commandContext.CommandId;
        }
    }
}
