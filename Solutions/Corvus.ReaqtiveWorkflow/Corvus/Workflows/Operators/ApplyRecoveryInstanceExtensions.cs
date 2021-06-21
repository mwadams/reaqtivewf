// <copyright file="ApplyRecoveryInstanceExtensions.cs" company="Endjin Limited">
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
    internal static class ApplyRecoveryInstanceExtensions
    {
        /// <summary>
        /// Subscribe to a given <see cref="ISubscribable{WorkflowSubjectVersion}"/> source, while the <see cref="WorkflowSubjectVersion.Status"/> is <see cref="WorkflowSubjectStatus.Faulted"/>,
        /// apply recovery instances for that subject as they occur.
        /// </summary>
        /// <param name="workflowSubject">The workflow subject versions for this workflow subject.</param>
        /// <param name="getRecoveryInstances">A function which provides a sequence of recovery instances.</param>
        /// <returns>A subscribable of <see cref="RecoveryInstanceAndSubjectVersion"/> for those <see cref="WorkflowSubjectRecoveryInstance"/>s that apply to the current <see cref="WorkflowSubjectVersion"/>.</returns>
        public static ISubscribable<RecoveryInstanceAndSubjectVersion> ApplyRecoveryInstancesToSubject(this ISubscribable<WorkflowSubjectVersion> workflowSubject, Func<ISubscribable<WorkflowSubjectRecoveryInstance>> getRecoveryInstances)
        {
            return workflowSubject
                .Where(subjectVersion => subjectVersion.Status == WorkflowSubjectStatus.Faulted)
                .SelectMany(subjectVersion => subjectVersion.ApplyRecoveryInstancesToSubjectVersion(getRecoveryInstances(), workflowSubject));
        }

        /// <summary>
        /// For a given <see cref="WorkflowSubjectVersion"/>, subscribe to the recovery instances until a new <see cref="WorkflowSubjectVersion"/> is produced in the
        /// <see cref="ISubscribable{WorkflowSubjectVersion}"/> source, filtering to those recovery instances whose <see cref="WorkflowSubjectRecoveryInstance.Version"/> has a <see cref="WorkflowSubjectVersion.SequenceNumber"/> that is greater than
        /// that of the current version..
        /// </summary>
        /// <param name="subjectVersion">The current version of the workflow subject.</param>
        /// <param name="recoveryInstances">The recovery instances.</param>
        /// <param name="workflowSubject">The workflow subject versions for this workflow subject.</param>
        /// <returns>A subscribable of <see cref="RecoveryInstanceAndSubjectVersion"/> for those <see cref="WorkflowSubjectRecoveryInstance"/>s that apply to the current <see cref="WorkflowSubjectVersion"/>.</returns>
        public static ISubscribable<RecoveryInstanceAndSubjectVersion> ApplyRecoveryInstancesToSubjectVersion(this WorkflowSubjectVersion subjectVersion, ISubscribable<WorkflowSubjectRecoveryInstance> recoveryInstances, ISubscribable<WorkflowSubjectVersion> workflowSubject)
        {
            return recoveryInstances
                .TakeUntil(workflowSubject)
                .Where(recoveryInstance => subjectVersion.Id == recoveryInstance.Version.Id && subjectVersion.SequenceNumber < recoveryInstance.Version.SequenceNumber)
                .Select(t => new RecoveryInstanceAndSubjectVersion(t, subjectVersion));
        }
    }
}
