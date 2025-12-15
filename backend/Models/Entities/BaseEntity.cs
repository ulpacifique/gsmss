using System.ComponentModel.DataAnnotations;

namespace CommunityFinanceAPI.Models.Entities
{
    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}