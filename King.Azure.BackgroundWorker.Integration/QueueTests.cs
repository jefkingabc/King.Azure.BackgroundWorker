﻿namespace King.Azure.BackgroundWorker.Integration
{
    using King.Azure.BackgroundWorker.Data;
    using NUnit.Framework;
    using System;
    using System.Threading.Tasks;

    [TestFixture]
    public class QueueTests
    {
        private readonly string ConnectionString = "UseDevelopmentStorage=true;";

        [Test]
        public async Task CreateIfNotExists()
        {
            var name = 'a' + Guid.NewGuid().ToString().ToLowerInvariant().Replace('-', 'a');
            var storage = new Queue(name, ConnectionString);
            var created = await storage.CreateIfNotExists();

            Assert.IsTrue(created);
        }
    }
}