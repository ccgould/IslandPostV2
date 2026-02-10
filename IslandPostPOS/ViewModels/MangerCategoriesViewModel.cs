using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IslandPostPOS.Services;
using IslandPostPOS.Shared.DTOs;
using System.Threading.Tasks;

namespace IslandPostPOS.ViewModels
{
    public partial class MangerCategoriesViewModel : ObservableObject
    {
        [ObservableProperty] private bool isEditing;
        [ObservableProperty] private CategoryDTO currentCategory;
        public ProductService service { get; set; }

        public MangerCategoriesViewModel(ProductService service)
        {
            this.service = service;
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(CurrentCategory.Description))
            {
                //Send Message
                return;
            }

            if(CurrentCategory.IdCategory > 0)
            {
                await service.UpdateCategoryAsync(CurrentCategory);
            }
            else
            {
                CurrentCategory.IsActive = true;
                await service.AddCategoryAsync(CurrentCategory);
            }

            IsEditing = false;
        }

        [RelayCommand]
        private async Task Back()
        {
            IsEditing = false;
        }

        [RelayCommand]
        private async Task AddNewProduct()
        {

            IsEditing = true;
        }

        [RelayCommand]
        private async Task Edit(CategoryDTO c)
        {
            try
            {
                CurrentCategory = c;
                IsEditing = true;
            }
            catch (System.Exception ex)
            {

            }
        }

        [RelayCommand]
        private async Task Delete(CategoryDTO c)
        {
            try
            {
                await service.DeleteCategoryAsync(c.IdCategory);
            }
            catch (System.Exception ex)
            {

            }
        }
    }
}
