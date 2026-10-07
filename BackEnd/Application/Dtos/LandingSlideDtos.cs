using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class LandingSliderDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsHero { get; set; }
        public string BannerUrl { get;  set; } = string.Empty;
        public string FirstUrl { get;  set; } = string.Empty;
        public string SecondUrl { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public string MobileBannerUrl { get; set; } = string.Empty;
        public string MobileVideoUrl { get; set; } = string.Empty;
        public string BannerTitle { get; set; } = string.Empty;
        public string BannerDescription { get; set; } = string.Empty;


    }
}
