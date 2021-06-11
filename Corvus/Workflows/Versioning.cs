// <copyright file="Versioning.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Workflows
{
    using System;

    /// <summary>
    /// Exposed version numbers of operator state.
    /// </summary>
    internal static class Versioning
    {
#pragma warning disable SA1310 // Field names should not contain underscore

        /// <summary>
        /// Version 0.9 of operator state.
        /// </summary>
        public static readonly Version V0_9 = new Version(0, 9, 0, 0);

#pragma warning restore SA1310 // Field names should not contain underscore
    }
}
