namespace Corvus.Workflows
{
    /// <summary>
    /// A tuple of trigger and workflow subject version.
    /// </summary>
    internal class TriggerAndSubjectVersion
    {
        public TriggerAndSubjectVersion(Trigger trigger, WorkflowSubjectVersion subject)
        {
            Trigger = trigger;
            SubjectVersion = subject;
        }

        public Trigger Trigger { get; init; }

        public WorkflowSubjectVersion SubjectVersion { get; init; }
    }
}