using OnlineShop.Domain.Common;
using Domain.Enums;

namespace OnlineShop.Domain.Entities
{
    public class Comment : BaseEntity
    {
        private Comment() { }

        public int UserId { get; private set; }
        public User User { get; private set; }

        public int TargetId { get; private set; }
        public EnumTargetType TargetType { get; private set; }

        public string Content { get; private set; } = string.Empty;
        public string UserName { get; private set; } = string.Empty;
        public string TargetTitle { get; private set; } = string.Empty;

        public string? ParentName { get; private set; } = string.Empty;

        public bool IsApproved { get; private set; } = false;

        // Reply (اختیاری ولی آینده‌نگرانه)
        public int? ParentId { get; private set; }
        public Comment? Parent { get; private set; }

        public ICollection<Comment> Replies { get; private set; } = new List<Comment>();

        // ===== Factory =====
        public static Comment Create(
            int userId,
            string userName,
            int targetId,
            string targetTitle,
            EnumTargetType targetType,
            string content,
            int currentUserId,
            int? parentId = null,
            string? parentName=null
        )
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new DomainException("متن نظر الزامی است.");

            var comment = new Comment
            {
                UserId = userId,
                UserName= userName,
                TargetId = targetId,
                TargetType = targetType,
                Content = content,
                ParentId = parentId,
                TargetTitle= targetTitle,
                ParentName = parentName
            };

            comment.MarkCreated(currentUserId);
            return comment;
        }

        public void Approve(bool isApproved ,int UserId)
        {
            IsApproved= isApproved;
            MarkUpdated(UserId);
        }

        public void Edit(string content, int userId)
        {
            if (userId != UserId)
                throw new DomainException("فقط نویسندهٔ نظر می‌تواند آن را ویرایش کند.");

            Content = content;
            MarkUpdated(userId);
        }
      
    }
}
