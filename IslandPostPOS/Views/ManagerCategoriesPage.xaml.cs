using IslandPostPOS.ViewModels;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IslandPostPOS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ManagerCategoriesPage : Page
    {
        public MangerCategoriesViewModel ViewModel => DataContext as MangerCategoriesViewModel;
        public ManagerCategoriesPage(MangerCategoriesViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}