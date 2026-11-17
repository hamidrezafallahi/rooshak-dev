// ProductImageCommand.cs
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Adds images to a product's gallery. Existing images are kept; when one of the new
/// files is flagged as main it takes the main role from the current main image.
/// </summary>
public class AddProductImageCommand : IRequest<ServiceResult<IdDto>>
{
    public int ProductId { get; set; }
    public List<IFormFile>? Images { get; set; } = new();
    public List<bool>? IsMainImages { get; set; } = new();
}

/// <summary>Swaps the file of one gallery image; its main/active state stays as it was.</summary>
public class ReplaceProductImageFileCommand : IRequest<ServiceResult<IdDto>>
{
    public int Id { get; set; }
    public IFormFile? Image { get; set; }
}

/// <summary>Makes one image the product's main image and clears the flag on the others.</summary>
public class SetMainProductImageCommand : IRequest<ServiceResult<IdDto>>
{
    public int Id { get; set; }
}

public class ActiveProductImageCommand : ActiveCommand, IRequest<ServiceResult<IdDto>> { }

public class DeleteProductImageCommand : IRequest<ServiceResult<IdDto>>
{
    public int Id { get; set; }
}
