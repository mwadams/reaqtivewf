// <copyright file="ApplyTrigger.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Workflows.Operators
{
    using System;
    using System.Diagnostics;

    using Corvus.Commands;

    using Reaqtive;

    /// <summary>
    /// An operator that projects a <see cref="TriggerAndSubjectVersion"/> source into an <see cref="IObservable{T}"/> of <see cref="Command"/> and
    /// a new <see cref="WorkflowSubjectVersion"/>, by using a <see cref="Workflow"/>.
    /// </summary>
    internal sealed class ApplyTrigger : SubscribableBase<WorkflowSubjectVersion>
    {
        private readonly ISubscribable<TriggerAndSubjectVersion> source;
        private readonly IObserver<Command> commandSink;
        private readonly Workflow workflow;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyTrigger"/> class.
        /// </summary>
        /// <param name="source">The source of <see cref="TriggerAndSubjectVersion"/> instances.</param>
        /// <param name="commandSink">The output sink for <see cref="Command"/> instances.</param>
        /// <param name="workflow">The <see cref="Workflow"/> to which the <see cref="WorkflowSubjectVersion"/> belongs.</param>
        public ApplyTrigger(ISubscribable<TriggerAndSubjectVersion> source, IObserver<Command> commandSink, Workflow workflow)
        {
            Debug.Assert(source != null, $"The {nameof(source)} must not be null.");
            Debug.Assert(workflow != null, $"The {nameof(workflow)} must not be null.");

            this.source = source;
            this.commandSink = commandSink;
            this.workflow = workflow;
        }

        /// <inheritdoc/>
        protected override ISubscription SubscribeCore(IObserver<WorkflowSubjectVersion> observer)
        {
            return new WorkflowEngine(this, observer);
        }

        private sealed class WorkflowEngine : StatefulUnaryOperator<ApplyTrigger, WorkflowSubjectVersion>, IObserver<TriggerAndSubjectVersion>
        {
            public WorkflowEngine(ApplyTrigger parent, IObserver<WorkflowSubjectVersion> observer)
                : base(parent, observer)
            {
            }

            public override string Name => "corvuswf:ApplyTrigger";

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

            public void OnNext(TriggerAndSubjectVersion value)
            {
                try
                {
                    if (this.Params.workflow.TryApplyTrigger(value.SubjectVersion, value.Trigger, out (WorkflowSubjectVersion workflowSubjectVersion, Command command)? result))
                    {
                        // TODO: Log the start of the transition

                        // If this throws an exception, we will not have excecuted any command, or updated the workflow status.
                        this.Params.commandSink.OnNext(result.Value.command);

                        // If this throws an exception, the commands for the transition may still be executed, but we will now be in an OnError situation
                        this.Output.OnNext(result.Value.workflowSubjectVersion);

                        // TODO: Log the fact that we have completed the transition.
                    }
                    else
                    {
                        // TODO: Log the fact that we didn't need to change our state?
                    }
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
