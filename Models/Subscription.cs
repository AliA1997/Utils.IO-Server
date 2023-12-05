using Postgrest.Attributes;
using Postgrest.Models;
using System.ComponentModel;

namespace Utils.IO.Server.Models
{
    public class UpdateUserSubscriptionRequest
    {
        public int NumberOfBasicRequests { get; set; } = 0;
        public int NumberOfWeb3Requests { get; set; } = 0;
        public int NumberOfChatbotRequests { get; set; } = 0;
    }

    [Table("subscriptions")]
    public class UtilsIOSubscription: BaseModel
    {
        [PrimaryKey("id")]
        public long Id { get; set; }

        [Column("created_at")]
        [DefaultValue(typeof(DateTimeOffset), "0001-01-01T00:00:00")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Column("subscription_name")]
        public string SubscriptionName { get; set; }

        [Column("total_requests")]
        public int? TotalRequests { get; set; }

    }
}
