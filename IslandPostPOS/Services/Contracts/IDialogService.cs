using System.Threading.Tasks;

namespace IslandPostPOS.Services.Contracts;

public interface IDialogService
{
    Task<decimal?> ShowCashDialogAsync();
}
