using IslandPostPOS.Services.Contracts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace IslandPostPOS.Services;

public class DialogService : IDialogService
{
    private readonly XamlRoot _xamlRoot;
    public DialogService(XamlRoot xamlRoot) => _xamlRoot = xamlRoot;

    public async Task<decimal?> ShowCashDialogAsync()
    {
        var amountBox = new TextBox { PlaceholderText = "Enter cash amount" };

        var dialog = new ContentDialog
        {
            Title = "Cash Payment",
            Content = amountBox,
            PrimaryButtonText = "OK",
            CloseButtonText = "Cancel",
            XamlRoot = _xamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary &&
            decimal.TryParse(amountBox.Text, out var amount))
        {
            return amount;
        }
        return null;
    }
}
