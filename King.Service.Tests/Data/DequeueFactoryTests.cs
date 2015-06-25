﻿namespace King.Service.Tests.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using King.Service.Data;
    using King.Service.Scalability;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class DequeueFactoryTests
    {
        const string ConnectionString = "UseDevelopmentStorage=true";
        
        [Test]
        public void Constructor()
        {
            new DequeueFactory(ConnectionString);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorConnectionStringNull()
        {
            new DequeueFactory(null);
        }

        [Test]
        public void ConstructorThroughputNull()
        {
            new DequeueFactory(ConnectionString, null);
        }

        [Test]
        public void IsIDequeueFactory()
        {
            Assert.IsNotNull(new DequeueFactory(ConnectionString) as IDequeueFactory);
        }

        [Test]
        public void Tasks()
        {
            var setup = new QueueSetup<object>()
            {
                Name = "test",
                Priority = QueuePriority.Low,
            };
            var f = new DequeueFactory(ConnectionString);
            var tasks = f.Tasks(setup);

            Assert.IsNotNull(tasks);
            Assert.AreEqual(2, tasks.Count());

            var t = (from n in tasks
                     where n.GetType() == typeof(InitializeStorageTask)
                     select true).FirstOrDefault();

            Assert.IsTrue(t);
        }

        [Test]
        public void TasksMultiple()
        {
            var random = new Random();
            var count = random.Next(1, 20);
            var setups = new List<IQueueSetup<object>>();
            for (var i = 0; i < count; i++)
            {
                var setup = new QueueSetup<object>()
                {
                    Name = "test",
                    Priority = QueuePriority.Low,
                };

                setups.Add(setup);
            }
            var f = new DequeueFactory(ConnectionString);
            var tasks = f.Tasks(setups);

            Assert.IsNotNull(tasks);
            Assert.AreEqual(count * 2, tasks.Count());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TasksSetupNull()
        {
            var f = new DequeueFactory(ConnectionString);
            var tasks = f.Tasks<object>((QueueSetup<object>)null);

            Assert.IsNotNull(tasks);
            Assert.AreEqual(2, tasks.Count());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TasksSetupsNull()
        {
            var f = new DequeueFactory(ConnectionString);
            var tasks = f.Tasks<object>((IEnumerable<QueueSetup<object>>)null);

            Assert.IsNotNull(tasks);
            Assert.AreEqual(2, tasks.Count());
        }

        [Test]
        public void Dequeue()
        {
            var setup = new QueueSetup<object>()
            {
                Name = "test",
                Priority = QueuePriority.High,
            };

            var random = new Random();
            var scale = new Range<byte>();

            scale.Maximum = (byte)random.Next(byte.MinValue, byte.MaxValue);
            scale.Minimum = (byte)random.Next(byte.MinValue, scale.Maximum);
            
            var throughput = Substitute.For<IQueueThroughput>();
            throughput.Scale(setup.Priority).Returns(scale);
            throughput.CheckScaleEvery(setup.Priority).Returns((byte)random.Next(1, 300));

            var f = new DequeueFactory(ConnectionString, throughput);
            var task = f.Dequeue<object>(setup);

            Assert.IsNotNull(task);
            var scaler = task as StorageQueueAutoScaler<object>;
            Assert.IsNotNull(scaler);
            Assert.AreEqual(scale.Minimum, scaler.Minimum);
            Assert.AreEqual(scale.Maximum, scaler.Maximum);

            throughput.Received().Scale(setup.Priority);
            throughput.Received().CheckScaleEvery(setup.Priority);
        }
    
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DequeueTaskSetupNull()
        {
            var df = new DequeueFactory(ConnectionString);
            df.Dequeue<object>(null);
        }

        [Test]
        public void TasksSimple()
        {
            var f = new DequeueFactory(ConnectionString);
            var tasks = f.Tasks<object>("test", () => { return null; });

            Assert.IsNotNull(tasks);
            Assert.AreEqual(2, tasks.Count());

            var t = (from n in tasks
                     where n.GetType() == typeof(InitializeStorageTask)
                     select true).FirstOrDefault();

            Assert.IsTrue(t);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TasksSimpleQueueNameNull()
        {
            var f = new DequeueFactory(ConnectionString);
            var tasks = f.Tasks<object>(null, () => { return null; });
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TasksSimpleProcessorNull()
        {
            var f = new DequeueFactory(ConnectionString);
            var tasks = f.Tasks<object>("test", null);

            Assert.IsNotNull(tasks);
            Assert.AreEqual(2, tasks.Count());

            var t = (from n in tasks
                     where n.GetType() == typeof(InitializeStorageTask)
                     select true).FirstOrDefault();

            Assert.IsTrue(t);
        }
    }
}