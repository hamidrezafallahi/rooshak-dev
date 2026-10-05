namespace Application.Dtos
{
    public class ContactRequestDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? PreferredContactTime { get; set; }
        public string? Message { get; set; }
        public bool IsReviewed { get; set; }
        public string? AdminNote { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
