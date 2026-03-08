using IslandPostApi.Models;
using IslandPostPOS.Shared.Enumerators;
using System;

namespace IslandPostApi.Services
{
    public static class SaleStateMachine
    {
        public static void Park(Sale sale)
        {
            if (sale.Status != SaleStatus.Active)
                throw new InvalidOperationException("Only active sales can be parked.");

            sale.Status = SaleStatus.Parked;
        }

        public static void Retrieve(Sale sale)
        {
            if (sale.Status != SaleStatus.Parked)
                throw new InvalidOperationException("Only parked sales can be retrieved.");

            sale.Status = SaleStatus.Retrieved;
        }

        public static void Complete(Sale sale)
        {
            if (sale.Status != SaleStatus.Retrieved && sale.Status != SaleStatus.Active)
                throw new InvalidOperationException("Only active or retrieved sales can be completed.");

            sale.Status = SaleStatus.Completed;
        }

        public static void Cancel(Sale sale)
        {
            if (sale.Status == SaleStatus.Completed)
                throw new InvalidOperationException("Completed sales cannot be cancelled.");

            sale.Status = SaleStatus.Cancelled;
        }
    }
}