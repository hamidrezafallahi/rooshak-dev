namespace Application.Dtos
{
    public class HomeSlideDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string MobileImageUrl { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;
        public string TitleFa { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public class AboutSectionDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string SectionKey { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public string EyebrowFa { get; set; } = string.Empty;
        public string EyebrowEn { get; set; } = string.Empty;
        public string TitleFa { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string BodyFa { get; set; } = string.Empty;
        public string BodyEn { get; set; } = string.Empty;
        public string LinkTextFa { get; set; } = string.Empty;
        public string LinkTextEn { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;
    }

    public class CustomFontDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        /// <summary>کلید قابل ذخیره در تم: custom-{Id}</summary>
        public string Key { get; set; } = string.Empty;
        public string NameFa { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
    }
}
