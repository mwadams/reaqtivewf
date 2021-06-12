// <copyright file="Workflow.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Workflows
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Corvus.Commands;

    /// <summary>
    /// A definiton of a state machine that defines the lifecycle of a workflow subject.
    /// </summary>
    /// <remarks>
    /// <para>
    /// We track/drive instances of "workflow subjects" through their lifecycle defined by a <see cref="Workflow"/>.
    /// </para>
    /// <para>
    /// The current state of a particular workflow subject is recorded as a <see cref="WorkflowSubjectVersion"/>.
    /// </para>
    /// <para>
    /// A workflow subject is associated with a single workflow for its whole lifecycle. A <see cref="Workflow"/> is immutabe.
    /// A new version of a <see cref="Workflow" /> is therefore a new workflow, and to make use of it a new workflow subject
    /// must be created associated with the new workflow, and the subject that was associted with the old workflow should be terminated.
    /// </para>
    /// <para>
    /// In order to transition a workflow subejct through its lifecycle, <see cref="Trigger"/>s are raised in the system.
    /// A <see cref="Trigger"/> is not associated with any particular workflow but represents something that has happened in the
    /// system.
    /// </para>
    /// <para>
    /// A workflow engine will apply an incoming <see cref="Trigger" /> to every workflow subject in all workflows. It does this by applying
    /// the <see cref="Trigger"/> to the current <see cref="WorkflowSubjectVersion"/> of each subject, regardless of the associated <see cref="Workflow"/>.
    /// </para>
    /// <para>
    /// First, the  engine will match the <see cref="Trigger.Topics" />
    /// with the <see cref="WorkflowSubjectVersion.Interests" />. If there is any
    /// intersection between those two lists, then the trigger is a candidate to be
    /// applied to the <see cref="WorkflowSubjectVersion"/>.
    /// </para>
    /// <para>
    /// We say that the "source state" is the <see cref="State" /> in the <see cref="Workflow.States"/>
    /// that is referenced by the <see cref="WorkflowSubjectVersion.StateId" />.
    /// </para>
    /// <para>
    /// The engine will use <see cref="Workflow.TryApplyTrigger" /> which will look at the source state and determine if all its <see cref="State.ExitConditions" /> evaluate to true.
    /// If so, then it will look through the source state's <see cref="State.Transitions" /> in order, to see if any a transition can be made.
    /// </para>
    /// <para>
    /// The target state for a transition is the <see cref="State" /> in the workflow's set of <see cref="States"/>
    /// that is referenced by the <see cref="Transition.TargetStateId" />.
    /// </para>
    /// <para>
    /// A <see cref="Transition" /> can only be made if <see cref="Transition.TestConditions(Workflow, WorkflowSubjectVersion, Trigger, out State?)"/> evaluates to true, and the
    /// <see cref="State.EntryConditions" /> for the target state for that  also evaluate to true.
    /// </para>
    /// <para>
    /// If it causes a <see cref="Transition" /> from the source state to the target state, then the workflow will produce a new <see cref="WorkflowSubjectVersion" />
    /// for that workflow subject, built from the interests of the target state, and a composite <see cref="Command" /> built from the <see cref="State.ExitActions" /> of the current state,
    /// the <see cref="Transition.Actions" /> and <see cref="State.EntryActions" /> of the target state, in that order.
    /// </para>
    /// <para>
    /// The newly produced <see cref="WorkflowSubjectVersion" /> will have the status <see cref="WorkflowSubjectStatus.WaitingForTransitionCommandAcks" />. Its <see cref="WorkflowSubjectVersion.Context"/>
    /// will be a <see cref="CommandContext"/> whose <see cref="CommandContext.Context"/> is built from the <see cref="Transition.ContextFactory"/> and whose <see cref="CommandContext.CommandId"/> is the
    /// <see cref="Command.Id"/> of the composite command executed as a result of the transition.
    /// </para>
    /// <para>
    /// The engine will then listen for an acknowledgement of that command, and call <see cref="ApplyCommandAck(WorkflowSubjectVersion, Commands.CommandAck)"/>. This produces a new
    /// <see cref="WorkflowSubjectVersion"/> with the status <see cref="WorkflowSubjectStatus.WaitingForTrigger"/> if the command was accepted, and <see cref="WorkflowSubjectStatus.Faulted"/> if the command
    /// was not accepted.
    /// </para>
    /// <para>
    /// If the subject was faulted, the <see cref="WorkflowSubjectVersion.Context"/> will be the <see cref="CommandContext"/> to allow you to identify the command as part of your recovery process. Otherwise
    /// the context will be unwrapped and set to <see cref="CommandContext.Context"/>.
    /// </para>
    /// <para>
    /// Note that the command will not necessarily have been executed at this point. It has merely been accepted for execution. If it is important to wait
    /// for the command to run to completion, you should model this in the workflow and raise triggers from the command handler to which the workflow can respond, for
    /// either success or failure.
    /// </para>
    /// <code>
    /// [<![CDATA[
    ///                    CommandAck
    ///           ┌──────────────────────────┐
    ///           │                          │
    /// ┌─────────▼─────────┐                │
    /// │ WaitingForTrigger │                │
    /// └──▲──────────┬─────┘                │
    ///    │          │                      │
    ///    │          │          ┌───────────┴─────────────────────┐
    ///    │          └──────────► WaitingForTransitionCommandAcks │
    ///    │                     └────┬───────┬────────────────────┘
    ///    │                          │       │
    ///    │       CommandNotAccepted │       │ Timeout
    ///    │                         ┌▼───────▼┐
    ///    └─────────────────────────┤ Faulted │
    ///         RecoveryMessage      └─────────┘
    /// ]]>
    /// </code>
    /// </remarks>
    public sealed class Workflow
    {
        private readonly ImmutableDictionary<string, State> states;

        /// <summary>
        /// Creates an instance of a <see cref="Workflow"/>.
        /// </summary>
        /// <param name="id">The ID of the workflow.</param>
        /// <param name="states">The states in the workflow.</param>
        public Workflow(string id, IEnumerable<State> states)
        {
            this.Id = id;
            this.states = states.ToImmutableDictionary(s => s.Id, s => s);
        }

        /// <summary>
        /// Gets the id of the workflow.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Gets the states in the workflow.
        /// </summary>
        public IEnumerable<State> States => this.states.Values;

        /// <summary>
        /// Applies a <see cref="CommandAck"/> to a <see cref="WorkflowSubjectVersion"/>.
        /// </summary>
        /// <param name="subjectVersion">The current <see cref="WorkflowSubjectVersion"/>.</param>
        /// <param name="ack">The <see cref="CommandAck"/> to apply.</param>
        /// <returns>A new <see cref="WorkflowSubjectVersion"/> with <see cref="WorkflowSubjectStatus.WaitingForTrigger"/> if the <see cref="CommandAck.State"/> was <see cref="CommandState.Accepted"/>,
        /// and <see cref="WorkflowSubjectStatus.Faulted"/> if the <see cref="CommandAck.State"/> was <see cref="CommandState.Rejected"/>.</returns>
        public static WorkflowSubjectVersion ApplyCommandAck(WorkflowSubjectVersion subjectVersion, CommandAck ack)
        {
            Debug.Assert(subjectVersion.Status == WorkflowSubjectStatus.WaitingForTransitionCommandAcks, $"The {nameof(subjectVersion)} must be WaitingForTransitionCommandAcks.");
            if (ack.State == CommandState.Accepted)
            {
                var commandContext = subjectVersion.Context as CommandContext;
                Debug.Assert(commandContext != null, $"The {nameof(subjectVersion.Context)} must be a {nameof(CommandContext)}");

                return new WorkflowSubjectVersion(subjectVersion.Id, DateTime.Now.Ticks, subjectVersion.StateId, subjectVersion.Interests, WorkflowSubjectStatus.WaitingForTrigger, subjectVersion.TriggerSequenceNumber, commandContext.Context);
            }
            else
            {
                return new WorkflowSubjectVersion(subjectVersion.Id, DateTime.Now.Ticks, subjectVersion.StateId, subjectVersion.Interests, WorkflowSubjectStatus.Faulted, subjectVersion.TriggerSequenceNumber, subjectVersion.Context);
            }
        }

        /// <summary>
        /// Try to get a state with the given state ID.
        /// </summary>
        /// <param name="stateId">The ID of the state.</param>
        /// <param name="state">The <see cref="State"/> corresponding to the id.</param>
        /// <returns><see langword="true"/> if a state with the given ID was found.</returns>
        public bool TryGetState(string stateId, [NotNullWhen(true)] out State? state)
        {
            return this.states.TryGetValue(stateId, out state);
        }

        /// <summary>
        /// Finds and executes a transition for the given trigger and workflow subject version, producing a new <see cref="WorkflowSubjectVersion"/> and the <see cref="Command"/> to execute for the transition.
        /// </summary>
        /// <param name="subjectVersion">The subjection version for which to try and execute a transition.</param>
        /// <param name="trigger">The trigger causing the transition.</param>
        /// <param name="result">The resulting workflow subject version, and the command to execute.</param>
        /// <returns><see langword="true" /> if a suitable transition was found, otherwise <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="Command"/> produced is a <see cref="CompositeCommand"/> which contains the <see cref="State.ExitActions"/> of the source state, the <see cref="Transition.Actions"/> and the <see cref="State.EntryActions"/> of the target state.
        /// </para>
        /// <para>
        /// If there were any commands produced in the composite command, then the context produced is an instance of a <see cref="CommandContext"/> which wraps the context produced by the <see cref="Transition.ContextFactory"/>, and associates the ID of the <see cref="Command"/> it created.
        /// The workflow version will have the <see cref="WorkflowSubjectVersion.Status"/> <see cref="WorkflowSubjectStatus.WaitingForTransitionCommandAcks"/>.
        /// </para>
        /// <para>
        /// If there were no commands in the composite command, then it is not queued, and the context produced is just the context produced by the <see cref="Transition.ContextFactory"/>.
        /// The workflow version will have the <see cref="WorkflowSubjectVersion.Status"/> <see cref="WorkflowSubjectStatus.WaitingForTransitionCommandAcks"/>.
        /// </para>
        /// </remarks>
        public bool TryApplyTrigger(WorkflowSubjectVersion subjectVersion, Trigger trigger, [NotNullWhen(true)] out (WorkflowSubjectVersion, Command?)? result)
        {
            Debug.Assert(subjectVersion.Status == WorkflowSubjectStatus.WaitingForTrigger, $"The {nameof(subjectVersion)} must be WaitingForTrigger.");

            if (this.TryGetState(subjectVersion.StateId, out State? startState))
            {
                if (startState.TryFindTransitionAndTargetState(this, subjectVersion, trigger, out (Transition Transition, State TargetState)? transitionAndTargetState))
                {
                    Command? command = CreateCommand(subjectVersion, trigger, startState, transitionAndTargetState.Value.Transition, transitionAndTargetState.Value.TargetState);
                    result = (
                        CreateWorkflowSubjectVersion(subjectVersion, trigger, transitionAndTargetState.Value.Transition, transitionAndTargetState.Value.TargetState, command),
                        command);
                    return true;
                }
            }

            result = null;
            return false;
        }

        private static Command? CreateCommand(WorkflowSubjectVersion subjectVersion, Trigger trigger, State startState, Transition transition, State endState)
        {
            Command[] commands = startState.ExitActions.Select(a => a(subjectVersion, trigger))
                            .Concat(
                                transition.Actions.Select(a => a(subjectVersion, trigger)))
                            .Concat(
                                endState.EntryActions.Select(a => a(subjectVersion, trigger))).ToArray();

            if (commands.Length == 0)
            {
                return null;
            }

            return CompositeCommand.Create(
                subjectVersion.Id,
                Guid.NewGuid().ToString(),
                commands);
        }

        private static WorkflowSubjectVersion CreateWorkflowSubjectVersion(WorkflowSubjectVersion subjectVersion, Trigger trigger, Transition transition, State targetState, Command? command)
        {
            if (command is Command c)
            {
                // TODO: Figure out the best strategy for a monotonically increasing sequence number - this is probably fine :-).
                return new WorkflowSubjectVersion(subjectVersion.Id, DateTime.Now.Ticks, targetState.Id, targetState.Interests(subjectVersion, trigger), WorkflowSubjectStatus.WaitingForTransitionCommandAcks, trigger.SequenceNumber, new CommandContext(c.Id, transition.ContextFactory(subjectVersion, trigger)));
            }

            return new WorkflowSubjectVersion(subjectVersion.Id, DateTime.Now.Ticks, targetState.Id, targetState.Interests(subjectVersion, trigger), WorkflowSubjectStatus.WaitingForTrigger, trigger.SequenceNumber, transition.ContextFactory(subjectVersion, trigger));
        }
    }
}
