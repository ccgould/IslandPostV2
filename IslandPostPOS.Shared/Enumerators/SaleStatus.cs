namespace IslandPostPOS.Shared.Enumerators
{
    public enum SaleStatus
    {
        Active = 0,      // Sale in progress
        Parked = 1,      // Temporarily saved, not yet resumed
        Retrieved = 2,   // Loaded from parked list, being edited
        Completed = 3,   // Finalized and checked out
        Cancelled = 4    // Optional: voided sale
    }
}
