namespace Corvus.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public static class CompositeCommand
    {
        private static Uri CompositeCommmandUri = new Uri("corvuswf://commands/compositeCommand");

        /// <summary>
        /// Creates a composite command.
        /// </summary>
        /// <param name="id">The unique ID of the command.</param>
        /// <param name="commands">The command to create.</param>
        /// <returns>A command representing an atomic command which executes the ordered set of commands.</returns>
        public static Command Create(string id, IEnumerable<Command> commands)
        {
            return new Command(id, CompositeCommmandUri, commands.ToImmutableArray());
        }
    }
}
