// <copyright file="CommandState.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Commands
{
    /// <summary>
    /// The acknowledgement state of a <see cref="Command"/>.
    /// </summary>
    public enum CommandState
    {
        /// <summary>
        /// The request to execute the command has been accepted.
        /// </summary>
        Accepted,

        /// <summary>
        /// The request to execute the command has been rejected.
        /// </summary>
        Rejected,
    }
}