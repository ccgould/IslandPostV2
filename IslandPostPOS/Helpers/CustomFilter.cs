using IslandPostPOS.Services;
using IslandPostPOS.Shared.DTOs;
using IslandPostPOS.Shared.Helpers;
using Syncfusion.UI.Xaml.Editors;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IslandPostPOS.Helpers
{
    public class CustomFiltering : IAutoCompleteFilterBehavior
    {
        private readonly ProductService productService;
        private readonly DebounceDispatcher _debouncer = new();


        public CustomFiltering(ProductService productService)
        {
            this.productService = productService;
        }
        public async Task<object> GetMatchingItemsAsync(SfAutoComplete source, AutoCompleteFilterInfo filterInfo)
        {
            if (string.IsNullOrWhiteSpace(filterInfo.Text) || filterInfo.Text.Length < 2)
                return new List<ProductDTO>();

            var result = await productService.SearchForProductsAsync(filterInfo.Text);

            // Return as IEnumerable<ProductDTO> (not boxed)
            return result.AsEnumerable();

        }
    }
}