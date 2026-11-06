using Application.Dtos;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace Application.Commands
{
    public class IdentityManagerCommands
    {
        public class CreateUserCommand : IRequest<ServiceResult<IdDto>>
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Password { get; set; }
            public IFormFile? UserImageFile { get; set; }
            public string? UserDescription { get; set; }


        }

        public class LoginCommand : IRequest<ServiceResult<LoginDto>>
        {
            public string EmailOrPhone { get; set; } = default!;
            public string Password { get; set; } = default!;
        }

        public class RefreshTokenCommand : IRequest<ServiceResult<LoginDto>>
        {
            public string? RefreshToken { get; set; } = default!;
            public string? AccessToken { get; set; } = default!;

            public string? Ip { get; set; }
            public string? UserAgent { get; set; }
        }
        public class  ChangeUserPasswordByAdminCommand : IRequest<ServiceResult<IdDto>>
        {
            public int Id { get; set; }
            public string NewPassword { get; set; }
            public ChangeUserPasswordByAdminCommand(int id, string newPassword)
            {
                Id = id;
                NewPassword = newPassword;
            }
        }

        public class ChangeUserPasswordCommand : IRequest<ServiceResult<IdDto>>
        {
            public int Id { get; set; }
            public string OldPassword { get; set; }
            public string NewPassword { get; set; }
            public ChangeUserPasswordCommand(int id, string oldPassword, string newPassword)
            {
                Id = id;
                OldPassword = oldPassword;
                NewPassword = newPassword;
            }
        }

        public class SetUserRoleCommand : IRequest<ServiceResult<IdDto>>
        {
            public int UserId { get; set; }   
            public int RoleId { get; set; }

            public SetUserRoleCommand(int userId, int roleId)
            {
                UserId = userId;
                RoleId = roleId;
            }
        }
        public class ActiveUserCommand : ActiveCommand, IRequest<ServiceResult<IdDto>> { }

        public class DeleteUserCommand : IRequest<ServiceResult<IdDto>>
        {
            public int Id { get; set; }
           
        }


    }
}

