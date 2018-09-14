// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test
{
    using Xunit;

    /// <summary>
    /// Defines a collection for tests which cannot run in parallel with other tests.
    /// </summary>
    [CollectionDefinition(nameof(SequentialTestCollection), DisableParallelization = true)]
    internal sealed class SequentialTestCollection
    {
    }
}
