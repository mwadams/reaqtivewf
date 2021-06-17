// <copyright file="Program.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.ReaqtiveWorkflow.Benchmarks
{
    using BenchmarkDotNet.Running;

    /// <summary>
    /// Main program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args">Program arguments.</param>
        public static void Main(string[] args)
        {
            BenchmarkDotNet.Reports.Summary summary = BenchmarkRunner.Run<WorkflowEngineBenchmarks>();
        }
    }
}
