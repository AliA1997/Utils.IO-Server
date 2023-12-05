using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;
using System;
using System.ComponentModel;

namespace Utils.IO.Server.Models
{ 
    [Table("user_subscriptions")]
    public class UserSubscription: BaseModel
    {
        [PrimaryKey("id")]
        public long Id { get; set; }

        [Column("username")]
        public string UserName { get; set; }

        [Column("country_of_residence")]
        public string CountryOfResidence { get; set; }

        [Column("avatar")]
        public string Avatar { get; set; }

        [Column("dob")]
        public DateTime? Dob { get; set; }

        [Column("belief_system")]
        public string BeliefSystem { get; set; }

        [Column("subscription")]
        public long? Subscription { get; set; }

        [Column("stripe_customer_id")]
        public string StripeCustomerId { get; set; }

        [Column("last_logged_in")]
        public DateTime LastLoggedIn { get; set; }
        
        [Column("basic_requests_left")]
        public int? BasicRequestsLeft { get; set; }

        [Column("web3_requests_left")]
        public int? Web3RequestsLeft { get; set; }

        [Column("chatbot_requests_left")]
        public int? ChatbotRequestsLeft { get; set; }

        [Column("tokens_used")]
        public int? TokensUsed { get; set; }

        [Column("service_last_used")]
        public bool? ServiceLastUsed { get; set; }

        [Column("activated")]
        public bool? Activated { get; set; }

        [Column("trial_start")]
        public DateTimeOffset? TrialStart { get; set; }

        [Column("trial_end")]
        public DateTimeOffset? TrialEnd { get; set; }

        [Column("trial_finished")]
        public bool? TrialFinished { get; set; }

        [Column("month_paid")]
        public bool? MonthPaid { get; set; }

        [Column("created_at")]
        [DefaultValue(typeof(DateTimeOffset), "0001-01-01T00:00:00")]
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
