﻿using InventoryService.Messages;
using System.Threading.Tasks;

namespace InventoryService.Tests
{
    public static class AwaitTaskExtension
    {
        public static IInventoryServiceCompletedMessage WaitAndGetOperationResult(this Task<IInventoryServiceCompletedMessage> task)
        {
            task.ConfigureAwait(false);
            Task.WaitAll(task);
            return task.Result;
        }
    }
}