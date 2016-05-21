﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryService.Storage
{
    public class InMemory : IInventoryStorage
    {
        private readonly Dictionary<string, Tuple<int, int>> _productInventories =
            new Dictionary<string, Tuple<int, int>>();

        public async Task<Tuple<int, int>> ReadInventory(string productId)
        {
            return _productInventories[productId];
        }

        public async Task<bool> WriteQuantity(string productId, int quantity)
        {
            if (!_productInventories.ContainsKey(productId))
                _productInventories.TryAdd(productId, new Tuple<int, int>(quantity, 0));
            else
                _productInventories[productId] = new Tuple<int, int>(quantity, _productInventories[productId].Item2);
            return true;
        }

        public async Task<bool> WriteReservations(string productId, int reservationQuantity)
        {
            if (!_productInventories.ContainsKey(productId))
                _productInventories.TryAdd(productId, new Tuple<int, int>(0, reservationQuantity));
            else
                _productInventories[productId] = new Tuple<int, int>(_productInventories[productId].Item1,
                    reservationQuantity);
            return true;
        }

        public async Task<bool> WriteInventory(string productId, int quantity, int reservationQuantity)
        {
            _productInventories.AddOrUpdate(productId, new Tuple<int, int>(quantity, reservationQuantity),
                (key, oldValue) => new Tuple<int, int>(quantity, reservationQuantity));
            return true;
        }

        public async Task Flush()
        {
        }

        public Task Flush(string productId)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}