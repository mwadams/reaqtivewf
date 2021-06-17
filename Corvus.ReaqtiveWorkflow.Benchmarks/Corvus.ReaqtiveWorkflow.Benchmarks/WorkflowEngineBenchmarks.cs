// <copyright file="WorkflowEngineBenchmarks.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.ReaqtiveWorkflow.Benchmarks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BenchmarkDotNet.Attributes;
    using Corvus.Commands;
    using Corvus.Workflows;

    /// <summary>
    /// Low-level benchmarks for the workflow engine.
    /// </summary>
    public class WorkflowEngineBenchmarks
    {
        private static readonly Uri WellKnownTriggerType = new ("corvus:workflow/triggers/match");
        private static readonly Uri WellKnownNonMatchTriggerType = new ("corvus:workflow/triggers/nonmatch");

        private readonly Workflow workflow;
        private readonly WorkflowSubjectVersion workflowSubjectVersion;
        private readonly Trigger matchingTrigger;
        private readonly Trigger matchingTypeButNotTopicsTrigger;
        private readonly Trigger nonMatchingTrigger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowEngineBenchmarks"/> class.
        /// </summary>
        public WorkflowEngineBenchmarks()
        {
            string startStateId = Guid.NewGuid().ToString();
            this.workflow = new Workflow(Guid.NewGuid().ToString(), BuildStates(startStateId));
            this.workflowSubjectVersion = BuildWorkflowSubjectVersion(this.workflow, startStateId);
            this.matchingTrigger = new (Guid.NewGuid().ToString(), 2, WellKnownTriggerType, new[] { "Hello", "There", "Everyone" }, new ());
            this.matchingTypeButNotTopicsTrigger = new (Guid.NewGuid().ToString(), 2, WellKnownTriggerType, new[] { "Fi", "Fi", "Fo", "Fum" }, new ());
            this.nonMatchingTrigger = new (Guid.NewGuid().ToString(), 2, WellKnownNonMatchTriggerType, new[] { "Fi", "Fi", "Fo", "Fum" }, new ());
        }

        /// <summary>
        /// A benchmark to determine performance of applying a matching trigger type.
        /// </summary>
        /// <returns><see langword="true"/> if the subject is a match.</returns>
        [Benchmark]
        public bool ApplyTriggerNotmatchingType()
        {
            return this.workflowSubjectVersion.ShouldApply(this.nonMatchingTrigger);
        }

        /// <summary>
        /// A benchmark to determine performance of applying a matching trigger type.
        /// </summary>
        /// <returns><see langword="true"/> if the subject is a match.</returns>
        [Benchmark]
        public bool ApplyTriggerMatchingTypeAndTopics()
        {
            return this.workflowSubjectVersion.ShouldApply(this.matchingTrigger);
        }

        /// <summary>
        /// A benchmark to determine performance of applying a matching trigger type.
        /// </summary>
        /// <returns><see langword="true"/> if the subject is a match.</returns>
        [Benchmark]
        public bool ApplyTriggerMatchingTypeButNotTopics()
        {
            return this.workflowSubjectVersion.ShouldApply(this.matchingTypeButNotTopicsTrigger);
        }

        private static WorkflowSubjectVersion BuildWorkflowSubjectVersion(Workflow workflow, string startStateId)
        {
            if (!workflow.TryGetState(startStateId, out State? startState))
            {
                throw new InvalidOperationException($"Unable to find the start state with ID {startStateId}");
            }

            return new WorkflowSubjectVersion(
                Guid.NewGuid().ToString(),
                1,
                startStateId,
                startState.TriggerTypes,
                new[] { "Foo", "Bar", "Baz", "There" },
                WorkflowSubjectStatus.WaitingForTrigger,
                0,
                new ());
        }

        private static IEnumerable<State> BuildStates(string startStateId)
        {
            State endState = new (
                Guid.NewGuid().ToString(),
                "endState",
                Enumerable.Empty<Transition>(),
                Enumerable.Empty<Func<WorkflowSubjectVersion, Trigger, bool>>(),
                Enumerable.Empty<Func<WorkflowSubjectVersion, Trigger, bool>>(),
                Enumerable.Empty<Func<WorkflowSubjectVersion, Trigger, Command>>(),
                Enumerable.Empty<Func<WorkflowSubjectVersion, Trigger, Command>>(),
                (wfs, t) => Enumerable.Empty<string>());

            State startState = new (
                startStateId,
                "startState",
                BuildTransitionsForStartState(endState.Id, WellKnownTriggerType),
                Enumerable.Empty<Func<WorkflowSubjectVersion, Trigger, bool>>(),
                Enumerable.Empty<Func<WorkflowSubjectVersion, Trigger, bool>>(),
                Enumerable.Empty<Func<WorkflowSubjectVersion, Trigger, Command>>(),
                Enumerable.Empty<Func<WorkflowSubjectVersion, Trigger, Command>>(),
                (wfs, t) => Enumerable.Empty<string>());

            return new[] { startState, endState };
        }

        private static IEnumerable<Transition> BuildTransitionsForStartState(string endStateId, Uri triggerOfInterest)
        {
            var transition = new Transition(
                Guid.NewGuid().ToString(),
                endStateId,
                triggerOfInterest,
                Enumerable.Empty<Func<WorkflowSubjectVersion, Trigger, bool>>(),
                Enumerable.Empty<Func<WorkflowSubjectVersion, Trigger, Command>>(),
                (c, t) => new { });

            return new[] { transition };
        }
    }
}
