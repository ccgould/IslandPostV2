using IslandPostPOS.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace IslandPostApi.Mapper;

public static class ProductApiMapper
{
    public static ProductDTO EnrichWithUrls(this ProductDTO dto, IUrlHelper urlHelper)
    {
        if (dto == null) return null;

        dto.PhotoUrl = urlHelper.Action("GetProductPhoto", "Product", new { id = dto.IdProduct });
        dto.ThumbnailUrl = urlHelper.Action("GetProductThumbnail", "Product", new { id = dto.IdProduct });

        return dto;
    }

public static List<ProductDTO> EnrichWithUrls(this IEnumerable<ProductDTO> dtos, IUrlHelper urlHelper)
{
    return dtos?.Select(dto => dto.EnrichWithUrls(urlHelper)).ToList() ?? new List<ProductDTO>();
}
}
