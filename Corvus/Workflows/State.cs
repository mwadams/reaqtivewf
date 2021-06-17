// <copyright file="State.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Workflows
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Corvus.Commands;

    /// <summary>
    /// A state in a <see cref="Workflow" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When a <see cref="Trigger" /> is applied to a state, its <see cref="ExitConditions" /> are
    /// tested to determine if it can leave the state at all. The engine then looks through the ordered set of
    /// <see cref="Transitions" /> for the state, until it finds the first one whose conditions permit it to be executed.
    /// </para>
    /// <para>
    /// When a state change occurs, a composite <see cref="Command" /> is created from the <see cref="ExitActions" /> of the
    /// state that is being left, the <see cref="Transition.Actions" /> and then the <see cref="EntryActions" /> of the <see cref="State" />
    /// corresponding to the <see cref="Transition.TargetStateId" />.
    /// </para>
    /// </remarks>
    public sealed class State
    {
        private readonly ImmutableDictionary<string, Transition> transitions;
        private readonly ImmutableArray<Func<WorkflowSubjectVersion, Trigger, bool>> exitConditions;
        private readonly ImmutableArray<Func<WorkflowSubjectVersion, Trigger, bool>> entryConditions;
        private readonly ImmutableArray<Func<WorkflowSubjectVersion, Trigger, Command>> entryActions;
        private readonly ImmutableArray<Func<WorkflowSubjectVersion, Trigger, Command>> exitActions;
        private readonly Func<WorkflowSubjectVersion, Trigger, IEnumerable<string>> interests;

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        /// <param name="id">The ID of the state.</param>
        /// <param name="name">The name of the state.</param>
        /// <param name="transitions">The transitions from the state.</param>
        /// <param name="entryConditions">The entry conditions for the state.</param>
        /// <param name="exitConditions">The exit conditions for the state.</param>
        /// <param name="entryActions">The entry actions for the state.</param>
        /// <param name="exitActions">The exit actions for the state.</param>
        /// <param name="interests">The interests of the state.</param>
        public State(string id, string name, IEnumerable<Transition> transitions, IEnumerable<Func<WorkflowSubjectVersion, Trigger, bool>> entryConditions, IEnumerable<Func<WorkflowSubjectVersion, Trigger, bool>> exitConditions, IEnumerable<Func<WorkflowSubjectVersion, Trigger, Command>> entryActions, IEnumerable<Func<WorkflowSubjectVersion, Trigger, Command>> exitActions, Func<WorkflowSubjectVersion, Trigger, IEnumerable<string>> interests)
        {
            this.Id = id;
            this.Name = name;
            this.interests = interests;
            this.transitions = transitions.ToImmutableDictionary(t => t.Id, t => t);
            this.entryConditions = entryConditions.ToImmutableArray();
            this.exitConditions = exitConditions.ToImmutableArray();
            this.entryActions = entryActions.ToImmutableArray();
            this.exitActions = exitActions.ToImmutableArray();
            this.TriggerTypes = this.transitions.Values.Select(s => s.TriggerType).ToImmutableHashSet();
        }

        /// <summary>
        /// Gets the globally unique ID of the state.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Gets the human-readable name of the state.
        /// </summary>
        /// <remarks>This does not have to be unique, but it is usually helpful if it is!.</remarks>
        public string Name { get; init; }

        /// <summary>
        /// Gets the entry conditions for the state.
        /// </summary>
        public IEnumerable<Func<WorkflowSubjectVersion, Trigger, bool>> EntryConditions => this.entryConditions;

        /// <summary>
        /// Gets the exit conditions for the state.
        /// </summary>
        public IEnumerable<Func<WorkflowSubjectVersion, Trigger, bool>> ExitConditions => this.exitConditions;

        /// <summary>
        /// Gets the entry actions for the state.
        /// </summary>
        /// <remarks>
        /// These functions generate a command from the current workflow subject version and the trigger.
        /// </remarks>
        public IEnumerable<Func<WorkflowSubjectVersion, Trigger, Command>> EntryActions => this.entryActions;

        /// <summary>
        /// Gets the exit actions for the state.
        /// </summary>
        /// <remarks>
        /// These functions generate a command from the current workflow subject version and the trigger.
        /// </remarks>
        public IEnumerable<Func<WorkflowSubjectVersion, Trigger, Command>> ExitActions => this.exitActions;

        /// <summary>
        /// Gets the interests for this state.
        /// </summary>
        public Func<WorkflowSubjectVersion, Trigger, IEnumerable<string>> Interests => this.interests;

        /// <summary>
        /// Gets the transitions from this state.
        /// </summary>
        public IEnumerable<Transition> Transitions => this.transitions.Values;

        /// <summary>
        /// Gets the trigger types to which this state will respond.
        /// </summary>
        public ImmutableHashSet<Uri> TriggerTypes { get; init; }

        /// <summary>
        /// Try to find the transition that applies to this subject version and trigger.
        /// </summary>
        /// <param name="workflow">The workflow of which this is a state.</param>
        /// <param name="subjectVersion">The current subject version.</param>
        /// <param name="trigger">The trigger to be applied.</param>
        /// <param name="transitionAndTargetState">A tuple of the <see cref="Transition"/> that has been found and the <see cref="State" /> referenced by its <see cref="Transition.TargetStateId" />.</param>
        /// <returns><see langword="true"/> if a transition is found.</returns>
        /// <remarks>
        /// <para>
        /// When a <see cref="Trigger" /> is applied, our <see cref="ExitConditions" /> are
        /// tested to determine if we can leave the state at all. We then look through the ordered set of
        /// <see cref="Transitions" /> until we find the first one whose <see cref="Transition.TestConditions(Workflow, WorkflowSubjectVersion, Trigger, out State?)" /> permits it to be made.
        /// </para>
        /// </remarks>
        public bool TryFindTransitionAndTargetState(Workflow workflow, WorkflowSubjectVersion subjectVersion, Trigger trigger, [NotNullWhen(true)] out (Transition, State)? transitionAndTargetState)
        {
            if (this.TestExitConditions(subjectVersion, trigger))
            {
                foreach (Transition candidate in this.Transitions)
                {
                    if (candidate.TestConditions(workflow, subjectVersion, trigger, out State? targetState))
                    {
                        transitionAndTargetState = (candidate, targetState);
                        return true;
                    }
                }
            }

            transitionAndTargetState = null;
            return false;
        }

        /// <summary>
        /// Tests the entry conditions of the state for the subject version and trigger.
        /// </summary>
        /// <param name="subjectVersion">The current <see cref="WorkflowSubjectVersion"/>.</param>
        /// <param name="trigger">The trigger to apply.</param>
        /// <returns><see langword="true"/> if all the conditions pass.</returns>
        internal bool TestEntryConditions(WorkflowSubjectVersion subjectVersion, Trigger trigger)
        {
            return this.entryConditions.All(condition => condition(subjectVersion, trigger));
        }

        private bool TestExitConditions(WorkflowSubjectVersion subjectVersion, Trigger trigger)
        {
            return this.exitConditions.All(condition => condition(subjectVersion, trigger));
        }
    }
}