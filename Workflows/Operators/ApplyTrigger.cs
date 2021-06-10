namespace Corvus.Workflows.Operators
{
    using System;
    using System.Diagnostics;

    using Corvus.Commands;

    using Reaqtive;

    internal sealed class ApplyTrigger : SubscribableBase<WorkflowSubjectVersion>
    {
        private readonly ISubscribable<TriggerAndSubjectVersion> source;
        private readonly IObserver<Command> commandSink;
        private readonly Workflow workflow;

        public ApplyTrigger(ISubscribable<TriggerAndSubjectVersion> source, IObserver<Command> commandSink, Workflow workflow)
        {
            Debug.Assert(source != null);
            Debug.Assert(workflow != null);

            this.source = source;
            this.commandSink = commandSink;
            this.workflow = workflow;
        }

        protected override ISubscription SubscribeCore(IObserver<WorkflowSubjectVersion> observer)
        {
            return new _(this, observer);
        }

        private sealed class _ : StatefulUnaryOperator<ApplyTrigger, WorkflowSubjectVersion>, IObserver<TriggerAndSubjectVersion>
        {
            public _(ApplyTrigger parent, IObserver<WorkflowSubjectVersion> observer)
                : base(parent, observer)
            {
            }

            public override string Name => "corvuswf:ApplyTrigger";

            public override Version Version => Versioning.v0_9;

            public void OnCompleted()
            {
                Output.OnCompleted();
                Dispose();
            }

            public void OnError(Exception error)
            {
                Output.OnError(error);
                Dispose();
            }

            public void OnNext(TriggerAndSubjectVersion value)
            {
                try
                {
                    if (Params.workflow.TryExecuteTransition(value, out (WorkflowSubjectVersion workflowSubjectVersion, Command command)? result))
                    {
                        // TODO: Log the start of the transition
                        // If this throws an exception, we will not have excecuted any command, or updated the workflow status.
                        Params.commandSink.OnNext(result.Value.command.Value);
                        // If this throws an exception, the commands for the transition may still be executed, but we will now be in an OnError situation
                        Output.OnNext(result.Value.workflowSubjectVersion);
                        // TODO: Log the fact that we have completed the transition.
                    }
                    else
                    {
                        // TODO: Log the fact that we didn't need to change our state?
                    }
                }
                catch (Exception exception)
                {
                    Output.OnError(exception);
                    Dispose();
                    return;
                }

            }

            protected override ISubscription OnSubscribe()
            {
                return Params.source.Subscribe(this);
            }
        }
    }
}
