// <copyright file="ApplyTriggerExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Workflows.Operators
{
    using System;
    using System.Linq;
    using Corvus.Workflows;
    using Reaqtive;

    /// <summary>
    /// Extension methods for triggers and subjects.
    /// </summary>
    internal static class ApplyTriggerExtensions
    {
        /// <summary>
        /// Subscribe to a given <see cref="ISubscribable{WorkflowSubjectVersion}"/> source, while the <see cref="WorkflowSubjectVersion.Status"/> is <see cref="WorkflowSubjectStatus.WaitingForTrigger"/>,
        /// apply triggers to the waiting for triggers.
        /// </summary>
        /// <param name="workflowSubject">The workflow subject versions for this workflow subject.</param>
        /// <param name="getTriggersFromSequenceNumber">A function which provides a sequence of triggers from the given sequence number.</param>
        /// <returns>A subscribable of <see cref="TriggerAndSubjectVersion"/> for those <see cref="Trigger"/>s that apply to the current <see cref="WorkflowSubjectVersion"/>.</returns>
        public static ISubscribable<TriggerAndSubjectVersion> ApplyTriggersToSubject(this ISubscribable<WorkflowSubjectVersion> workflowSubject, Func<long, ISubscribable<Trigger>> getTriggersFromSequenceNumber)
        {
            return workflowSubject
                .Where(subjectVersion => subjectVersion.Status == WorkflowSubjectStatus.WaitingForTrigger)
                .SelectMany(subjectVersion => subjectVersion.ApplyTriggersToSubjectVersion(getTriggersFromSequenceNumber(subjectVersion.TriggerSequenceNumber + 1), workflowSubject));
        }

        /// <summary>
        /// For a given <see cref="WorkflowSubjectVersion"/>, subscribe to the triggers until a new <see cref="WorkflowSubjectVersion"/> is produced in the
        /// <see cref="ISubscribable{WorkflowSubjectVersion}"/> source, filtering to those triggers whose <see cref="Trigger.Topics"/> intersect
        /// with the <see cref="WorkflowSubjectVersion.Interests"/>.
        /// </summary>
        /// <param name="subjectVersion">The current version of the workflow subject.</param>
        /// <param name="triggers">The triggers.</param>
        /// <param name="workflowSubject">The workflow subject versions for this workflow subject.</param>
        /// <returns>A subscribable of <see cref="TriggerAndSubjectVersion"/> for those <see cref="Trigger"/>s that apply to the current <see cref="WorkflowSubjectVersion"/>.</returns>
        public static ISubscribable<TriggerAndSubjectVersion> ApplyTriggersToSubjectVersion(this WorkflowSubjectVersion subjectVersion, ISubscribable<Trigger> triggers, ISubscribable<WorkflowSubjectVersion> workflowSubject)
        {
            return triggers
                .TakeUntil(workflowSubject)
                .Where(trigger => trigger.Topics.Any(topic => subjectVersion.Interests.Contains(topic)))
                .Select(t => new TriggerAndSubjectVersion(t, subjectVersion));
        }
    }
}
