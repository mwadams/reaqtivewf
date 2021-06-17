// <copyright file="CommandAck.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Commands
{
    using System;

    /// <summary>
    /// An acknowledgement of a request to execute a command.
    /// </summary>
    public class CommandAck
    {
        /// <summary>
        /// Creates an instance of the command with the given id, sequence number, commandType and payload.
        /// </summary>
        /// <param name="sourceId">The ID of the source of the command.</param>
        /// <param name="id">The id of the command.</param>
        /// <param name="commandType">The type of the command.</param>
        /// <param name="state">The state of the command.</param>
        public CommandAck(string sourceId, string id, Uri commandType, CommandState state)
        {
            this.SourceId = sourceId;
            this.Id = id;
            this.CommandType = commandType;
            this.State = state;
        }

        /// <summary>
        /// Gets the ID of the source of the command.
        /// </summary>
        public string SourceId { get; init; }

        /// <summary>
        /// Gets the unique ID of the command.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Gets the type of the command.
        /// </summary>
        public Uri CommandType { get; init; }

        /// <summary>
        /// Gets the acknowledged state of the command.
        /// </summary>
        public CommandState State { get; init; }
    }
}
