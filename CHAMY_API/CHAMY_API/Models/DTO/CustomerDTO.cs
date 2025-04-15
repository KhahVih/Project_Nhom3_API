namespace CHAMY_API.Models.DTO
{
    public class CustomerDTO
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Fullname { get; set; }
        public string Address { get; set; }
        public string? Phone { get; set; }
        public bool Gender { get; set; }
        public bool? IsClone { get; set; } = false;
        public DateTime? CreatedAt { get; set; }
        public DateTime? Date { get; set; }
        // Optional: Include comment count or simplified comment info
        public int CommentCount { get; set; }

        public List<CommentDTO> Comments { get; set; } = new List<CommentDTO>();
        public List<CartItem>? CartIteams { get; set; }
    }
}
