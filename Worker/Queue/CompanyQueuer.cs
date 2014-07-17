﻿namespace Worker.Queue
{
    using King.Azure.BackgroundWorker;
    using King.Azure.BackgroundWorker.Data;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Newtonsoft.Json;
    using System;
    using Worker.Queue;

    public class CompanyQueuer : TaskManager
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
