// <copyright file="WorkflowSubjectStatus.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Workflows
{
    /// <summary>
    /// The status of the <see cref="WorkflowSubjectVersion"/>.
    /// </summary>
    /// <remarks>
    /// <para>>
    /// The engine pushes a workflow subject through an internal state machine
    /// as it works through its lifecycle.
    /// </para>
    /// <para>
    /// <see cref="Workflow.TryApplyTrigger"/> geenrates a command and a new <see cref="WorkflowSubjectVersion"/>.
    /// That subject version is in the state <see cref="WaitingForTransitionCommandAcks"/>. The engine then queues
    /// the command and commits that new version. It does not apply any further triggers until it receieves an
    /// acknowledgement that the command has been accepted. When it gets that ack, it emits a new <see cref="WorkflowSubjectVersion"/>
    /// with the status <see cref="WaitingForTrigger"/>. If, on the other hand, it times out, or receives a "command not accepted" event,
    /// it will generate a new <see cref="WorkflowSubjectVersion"/> instance with the status <see cref="Faulted"/>. See <see cref="Workflow"/>
    /// for more information on this lifecylce.
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
    public enum WorkflowSubjectStatus
    {
        /// <summary>
        /// The workflow subject is waiting for a trigger.
        /// </summary>
        WaitingForTrigger,

        /// <summary>
        /// The workflow subject has transitioned and is waiting for the transition command to be accepted.
        /// </summary>
        WaitingForTransitionCommandAcks,

        /// <summary>
        /// The workflow subject has faulted.
        /// </summary>
        Faulted,
    }
}