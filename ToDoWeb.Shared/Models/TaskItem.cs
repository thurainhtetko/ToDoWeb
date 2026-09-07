using Postgrest.Attributes;
using Postgrest.Models;

namespace ToDoWeb.Shared.Models
{
    [Table("tasks")]
    public class TaskItem : BaseModel
    {
        [PrimaryKey("id")]
        public string? Id { get; set; }

        [Column("userid")]
        public string? UserId { get; set; }

        [Column("title")]
        public string? Title { get; set; }

        [Column("detail")]
        public string? Detail { get; set; }

        [Column("priority")]
        public string? Priority { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
    }
}