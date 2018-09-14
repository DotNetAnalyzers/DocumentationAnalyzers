// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test
{
    using Xunit;

    public class AttributeTests
    {
        [Fact]
        public void TestNoCodeFixAttributeReason()
        {
            string reason = "Reason";
            var attribute = new NoCodeFixAttribute(reason);
            Assert.Same(reason, attribute.Reason);
        }

        [Fact]
        public void TestWorkItemAttribute()
        {
            int id = 1;
            string issueUri = "https://github.com/DotNetAnalyzers/DocumentationAnalyzers/issues/1";
            var attribute = new WorkItemAttribute(id, issueUri);
            Assert.Equal(id, attribute.Id);
            Assert.Same(issueUri, attribute.Location);
        }
    }
}
