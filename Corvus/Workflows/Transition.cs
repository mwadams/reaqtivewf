// <copyright file="Transition.cs" company="Endjin Limited">
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
    /// A transition between workflow states.
    /// </summary>
    /// <remarks>
    /// The transition is directed from a source state to a <See cref="TargetStateId" />. The transition
    /// can only be made if <see cref="Transition.TestConditions(Workflow, WorkflowSubjectVersion, Trigger, out State?)" /> evaluates to true, and the
    /// <see cref="State.EntryConditions" /> for the state with <see cref="TargetStateId" /> also evaluate to true.
    /// When the transition takes place, it provides <see cref="Actions" /> which contribute to the composite <see cref="Command" />
    /// which is scheduled for  execution by the workflow engine.
    /// </remarks>
    public class Transition
    {
        private readonly ImmutableArray<Func<WorkflowSubjectVersion, Trigger, bool>> conditions;
        private readonly ImmutableArray<Func<WorkflowSubjectVersion, Trigger, Command>> actions;

        /// <summary>
        /// Creates an instance of a <see cref="Transition"/>.
        /// </summary>
        /// <param name="id">The id of the transition.</param>
        /// <param name="targetStateId">The ID of the target state.</param>
        /// <param name="conditions">The conditions for the transition.</param>
        /// <param name="actions">The actions for the transition.</param>
        public Transition(string id, string targetStateId, IEnumerable<Func<WorkflowSubjectVersion, Trigger, bool>> conditions, IEnumerable<Func<WorkflowSubjectVersion, Trigger, Command>> actions)
        {
            this.Id = id;
            this.TargetStateId = targetStateId;
            this.conditions = conditions.ToImmutableArray();
            this.actions = actions.ToImmutableArray();
        }

        /// <summary>
        /// Gets the ID of the transition.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Gets the ID of the target state for this transition.
        /// </summary>
        public string TargetStateId { get; init; }

        /// <summary>
        /// Gets the actions for this transition.
        /// </summary>
        public IEnumerable<Func<WorkflowSubjectVersion, Trigger, Command>> Actions => this.actions;

        /// <summary>
        /// Tests the conditions for the transition and its target state.
        /// </summary>
        /// <param name="workflow">The workflow in which this is a transition.</param>
        /// <param name="subjectVersion">The current workflow subject version.</param>
        /// <param name="trigger">The trigger to be applied.</param>
        /// <param name="targetState">The target state of the transition, if the conditions evaluated to true. Otherwise null.</param>
        /// <returns><see langword="true"/> if the conditions evaluated to true.</returns>
        internal bool TestConditions(Workflow workflow, WorkflowSubjectVersion subjectVersion, Trigger trigger, [NotNullWhen(true)] out State? targetState)
        {
            if (this.conditions.All(condition => condition(subjectVersion, trigger)))
            {
                if (workflow.TryGetState(this.TargetStateId, out State? ts))
                {
                    if (ts.TestEntryConditions(subjectVersion, trigger))
                    {
                        targetState = ts;
                        return true;
                    }
                }
            }

            targetState = null;
            return false;
        }
    }
}