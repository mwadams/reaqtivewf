// <copyright file="ApplyRecoveryInstance.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Workflows.Operators
{
    using System;
    using System.Diagnostics;

    using Reaqtive;

    /// <summary>
    /// An operator that projects a <see cref="RecoveryInstanceAndSubjectVersion"/> source into
    /// a new <see cref="WorkflowSubjectVersion"/> for a workflow subject whose current <see cref="WorkflowSubjectVersion.Status"/> is
    /// <see cref="WorkflowSubjectStatus.Faulted"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This provides a mechanism to recover a faulted workflow subject. The only requirement for consistency is that the
    /// <see cref="WorkflowSubjectVersion.SequenceNumber"/> in the recovery instance is greater than that of the current version.
    /// This provides some level of idempotency in recovery (if e.g. the recovery events are replayed).
    /// </para>
    /// </remarks>
    internal sealed class ApplyRecoveryInstance : SubscribableBase<WorkflowSubjectVersion>
    {
        private readonly ISubscribable<RecoveryInstanceAndSubjectVersion> source;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyRecoveryInstance"/> class.
        /// </summary>
        /// <param name="source">The source of <see cref="RecoveryInstanceAndSubjectVersion"/> instances.</param>
        public ApplyRecoveryInstance(ISubscribable<RecoveryInstanceAndSubjectVersion> source)
        {
            Debug.Assert(source != null, $"The {nameof(source)} must not be null.");
            this.source = source;
        }

        /// <inheritdoc/>
        protected override ISubscription SubscribeCore(IObserver<WorkflowSubjectVersion> observer)
        {
            return new WorkflowEngine(this, observer);
        }

        private sealed class WorkflowEngine : StatefulUnaryOperator<ApplyRecoveryInstance, WorkflowSubjectVersion>, IObserver<RecoveryInstanceAndSubjectVersion>
        {
            public WorkflowEngine(ApplyRecoveryInstance parent, IObserver<WorkflowSubjectVersion> observer)
                : base(parent, observer)
            {
            }

            public override string Name => "corvuswf:ApplyRecoveryInstance";

            public override Version Version => Versioning.V0_9;

            public void OnCompleted()
            {
                this.Output.OnCompleted();
                this.Dispose();
            }

            public void OnError(Exception error)
            {
                this.Output.OnError(error);
                this.Dispose();
            }

            public void OnNext(RecoveryInstanceAndSubjectVersion value)
            {
                try
                {
                    // TODO: Log the start of the transition
                    Debug.Assert(value.RecoveryInstance.Version.SequenceNumber > value.SubjectVersion.SequenceNumber, "The recovery instance must have a later sequence number than the current version.");
                    Debug.Assert(value.RecoveryInstance.Version.Id == value.SubjectVersion.Id, "The recovery instance must have the same ID as the current version.");
                    Debug.Assert(value.SubjectVersion.Status == WorkflowSubjectStatus.Faulted, "The current version must be faulted.");
                    this.Output.OnNext(value.RecoveryInstance.Version);

                    // TODO: Log the fact that we have completed the transition.
                }
                catch (Exception exception)
                {
                    this.Output.OnError(exception);
                    this.Dispose();
                    return;
                }
            }

            protected override ISubscription OnSubscribe()
            {
                return this.Params.source.Subscribe(this);
            }
        }
    }
}
