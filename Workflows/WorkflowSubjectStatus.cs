namespace Corvus.Workflows
{
    /// <summary>
    /// The status of the workflow subject version.
    /// </summary>
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