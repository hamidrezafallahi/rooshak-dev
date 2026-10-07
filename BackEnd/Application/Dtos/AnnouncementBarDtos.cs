namespace Application.Dtos
{
    public class AnnouncementBarDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string MessageFa { get; set; } = string.Empty;
        public string MessageEn { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;
        public string BackgroundImageUrl { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
        public string TextColor { get; set; } = string.Empty;
        public int HeightPx { get; set; }
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public int DisplayOrder { get; set; }
    }
}
