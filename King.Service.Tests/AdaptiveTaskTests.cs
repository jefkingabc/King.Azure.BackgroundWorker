﻿namespace King.Service.Tests.Unit
{
    using King.Service.Timing;
    using NUnit.Framework;

    [TestFixture]
    public class AdaptiveTaskTests
    {
        [Test]
        public void Constructor()
        {
            using (new AdaptiveHelper())
            {
            }
        }
    }
}