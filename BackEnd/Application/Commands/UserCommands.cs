using Application.Dtos;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using System.Text.Json.Serialization;

// ===== به‌روزرسانی پروفایل =====
public class UpdateUserProfileCommand : IRequest<ServiceResult<IdDto>>
{
    public int UserId { get; set; }
    public string FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public IFormFile? UserImageFile { get; set; }
    public string? UserDescription { get; set; }
}

// ===== افزودن آدرس =====
public class AddUserAddressCommand : UserAddressDto, IRequest<ServiceResult<IdDto>>
{
}

public class ActiveUserAddressCommand : ActiveCommand, IRequest<ServiceResult<IdDto>> { }
public class DeleteUserAddressCommand : IRequest<ServiceResult<IdDto>>
{
    public int Id { get; set; }
}


public class SetUserRoleCommand : IRequest<ServiceResult<IdDto>>
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}



public class UpdateUserAddressCommand : UserAddressDto, IRequest<ServiceResult<IdDto>>
{
}


public class SetDefaultUserAddressCommand : IRequest<ServiceResult<IdDto>>
{
    public int Id { get; set; }
}
