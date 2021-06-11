// <copyright file="CompositeCommand.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Builds a command whose payload is an ordered list of commands.
    /// </summary>
    public static class CompositeCommand
    {
        /// <summary>
        /// The command type of the composite command.
        /// </summary>
        public static readonly Uri CompositeCommmandType = new Uri("corvus:commands/compositecommand");

        /// <summary>
        /// Creates a composite command.
        /// </summary>
        /// <param name="sourceId">The id of the source of the request to execute the command.</param>
        /// <param name="id">The unique ID of the command.</param>
        /// <param name="commands">The command to create.</param>
        /// <returns>A command representing an atomic command which executes the ordered set of commands.</returns>
        public static Command Create(string sourceId, string id, IEnumerable<Command> commands)
        {
            return new Command(sourceId, id, CompositeCommmandType, commands.ToImmutableArray());
        }
    }
}
