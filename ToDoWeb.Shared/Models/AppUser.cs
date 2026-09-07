using Postgrest.Attributes;
using Postgrest.Models;

namespace ToDoWeb.Shared.Models
{
    [Table("users")]
    public class AppUser : BaseModel
    {
        [PrimaryKey("id")]
        public string? Id { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
    }
}