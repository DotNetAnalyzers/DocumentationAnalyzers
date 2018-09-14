// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Helpers
{
    using System.Threading.Tasks;

    internal static class SpecializedTasks
    {
        internal static Task CompletedTask { get; } = Task.FromResult(default(VoidResult));

        private struct VoidResult
        {
        }
    }
}
