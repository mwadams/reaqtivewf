namespace Corvus.Commands
{
    using System;

    public class Command
    {
        /// <summary>
        /// Creates an instance of the command with the given id, sequence number, commandType and payload.
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="id"></param>
        /// <param name="commandType"></param>
        /// <param name="payload"></param>
        public Command(string sourceId, string id, Uri commandType, object payload)
        {
            this.SourceId = sourceId;
            this.Id = id;
            this.CommandType = commandType;
            this.Payload = payload;
        }

        /// <summary>
        /// Gets the ID of the source of the command.
        /// </summary>
        public string SourceId { get; init; }

        /// <summary>
        /// The unique ID of the command.
        /// </summary>
        public string Id { get; init; }


        /// Gets the type of the command
        /// </summary>
        public Uri CommandType { get; init; }

        /// <summary>
        /// Gets the payload for the command.
        /// </summary>
        /// <remarks>
        /// TODO: Figure out how we represent an anonoymous payload that the command handler knows how to deal with.
        /// I think we just do our usual content pattern for command handling, but we'll see.
        /// </remarks>
        public object Payload { get; init; }
    }
}
