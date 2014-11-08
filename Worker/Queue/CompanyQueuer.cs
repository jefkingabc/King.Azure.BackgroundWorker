﻿namespace Worker.Queue
{
    using King.Azure.Data;
    using King.Service;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Newtonsoft.Json;
    using System;

    /// <summary>
    /// Example of Task class which adds a company to a queue
    /// </summary>
    public class CompanyQueuer : RecurringTask
    {
        private int id = 0;

        public override void Run()
        {
            var company = new CompanyModel()
            {
                Id = Guid.NewGuid(),
                Name = string.Format("company-{0}", id),
            };

            var queue = new StorageQueue("queue", "UseDevelopmentStorage=true;");
            var task = queue.Save(new CloudQueueMessage(JsonConvert.SerializeObject(company)));
            task.Wait();

            id++;
        }
    }
}
