using Common;
using MediatR;
using Microsoft.AspNetCore.Http;


namespace Application.Commands
{
    public class CreateSlideCommand : IRequest<ServiceResult<IdDto>>
    {
        public IFormFile BannerUrl { get; set; }
        /// <summary>Optional hero video (mp4/webm, max ~30 MB).</summary>
        public IFormFile? VideoUrl { get; set; }
        /// <summary>Optional mobile poster / video (fall back to the desktop ones when absent).</summary>
        public IFormFile? MobileBannerUrl { get; set; }
        public IFormFile? MobileVideoUrl { get; set; }
        /// <summary>Optional; hidden on the hero when empty.</summary>
        public string? BannerTitle { get; set; }
        /// <summary>Optional; hidden on the hero when empty.</summary>
        public string? BannerDescription { get; set; }
        public string FirstUrl { get; set; }
        /// <summary>Optional second link; needs SecondUrlTextFa and SecondUrlTextEn.</summary>
        public string? SecondUrl { get; set; }
        public string? SecondUrlTextFa { get; set; }
        public string? SecondUrlTextEn { get; set; }
        /// <summary>0..100: opacity of the banner title and description; null = unchanged (100 on create).</summary>
        public int? TitleOpacity { get; set; }
    }

    public class UpdateSlideCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
        public IFormFile? BannerUrl { get; set; }
        public IFormFile? VideoUrl { get; set; }
        public bool RemoveVideo { get; set; }
        public IFormFile? MobileBannerUrl { get; set; }
        public IFormFile? MobileVideoUrl { get; set; }
        public bool RemoveMobileBanner { get; set; }
        public bool RemoveMobileVideo { get; set; }
        public string? BannerTitle { get; set; }
        public string? BannerDescription { get; set; }
        public string? FirstUrl { get; set; }
        public string? SecondUrl { get; set; }
        public string? SecondUrlTextFa { get; set; }
        public string? SecondUrlTextEn { get; set; }
        /// <summary>0..100: opacity of the banner title and description; null = unchanged (100 on create).</summary>
        public int? TitleOpacity { get; set; }
    }

    public class DeleteSlideCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
    }

    public class ActiveSlideCommand : ActiveCommand, IRequest<ServiceResult<IdDto>> { }
    public class SetHeroBannerCommand : IRequest<ServiceResult<IdDto>>
    {
        public int Id { get; set; }
    }
}
