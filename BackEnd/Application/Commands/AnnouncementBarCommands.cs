using Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{
    public class CreateAnnouncementBarCommand : IRequest<ServiceResult<IdDto>>
    {
        public string MessageFa { get; set; } = string.Empty;
        public string? MessageEn { get; set; }
        public string? LinkUrl { get; set; }
        public IFormFile? BackgroundImageUrl { get; set; }
        public string? BackgroundColor { get; set; }
        public string? TextColor { get; set; }
        public int? HeightPx { get; set; }
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public int? DisplayOrder { get; set; }
    }

    public class UpdateAnnouncementBarCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
        public string? MessageFa { get; set; }
        public string? MessageEn { get; set; }
        public string? LinkUrl { get; set; }
        public IFormFile? BackgroundImageUrl { get; set; }
        public bool RemoveBackgroundImage { get; set; }
        public string? BackgroundColor { get; set; }
        public string? TextColor { get; set; }
        public int? HeightPx { get; set; }
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public int? DisplayOrder { get; set; }
    }

    /// <summary>نمایش / عدم نمایش نوار.</summary>
    public class ActiveAnnouncementBarCommand : ActiveCommand, IRequest<ServiceResult<IdDto>> { }

    public class DeleteAnnouncementBarCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
    }
}
