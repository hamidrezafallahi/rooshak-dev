namespace Application.Dtos
{
    public class ThemeSettingDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; } = string.Empty;

        public string PrimaryColor { get; set; } = string.Empty;
        public string SecondaryColor { get; set; } = string.Empty;
        public string HighlightColor { get; set; } = string.Empty;
        public string NeutralColor { get; set; } = string.Empty;
        public string SuccessColor { get; set; } = string.Empty;
        public string ErrorColor { get; set; } = string.Empty;
        public string WarningColor { get; set; } = string.Empty;
        public string InfoColor { get; set; } = string.Empty;

        public string SurfaceColor { get; set; } = string.Empty;
        public string SurfaceMutedColor { get; set; } = string.Empty;
        public string BorderColor { get; set; } = string.Empty;
        public string TextColor { get; set; } = string.Empty;
        public string TextMutedColor { get; set; } = string.Empty;
    }
}
