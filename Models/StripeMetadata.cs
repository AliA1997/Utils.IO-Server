using Newtonsoft.Json;
namespace Utils.IO.Server.Models
{

    public class StripeMetadata
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("address")]
        public object Address { get; set; }

        [JsonProperty("balance")]
        public int Balance { get; set; }

        [JsonProperty("created")]
        public int Created { get; set; }

        [JsonProperty("currency")]
        public object Currency { get; set; }

        [JsonProperty("default_source")]
        public object DefaultSource { get; set; }

        [JsonProperty("delinquent")]
        public bool Delinquent { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("discount")]
        public object Discount { get; set; }

        [JsonProperty("email")]
        public object Email { get; set; }

        [JsonProperty("invoice_prefix")]
        public string InvoicePrefix { get; set; }

        [JsonProperty("invoice_settings")]
        public InvoiceSettings InvoiceSettings { get; set; }

        [JsonProperty("livemode")]
        public bool Livemode { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, object> Metadata { get; set; }

        [JsonProperty("name")]
        public object Name { get; set; }

        [JsonProperty("next_invoice_sequence")]
        public int NextInvoiceSequence { get; set; }

        [JsonProperty("phone")]
        public object Phone { get; set; }

        [JsonProperty("preferred_locales")]
        public List<object> PreferredLocales { get; set; }

        [JsonProperty("shipping")]
        public object Shipping { get; set; }

        [JsonProperty("tax_exempt")]
        public string TaxExempt { get; set; }

        [JsonProperty("test_clock")]
        public object TestClock { get; set; }
    }

    public class InvoiceSettings
    {
        [JsonProperty("custom_fields")]
        public object CustomFields { get; set; }

        [JsonProperty("default_payment_method")]
        public string DefaultPaymentMethod { get; set; }

        [JsonProperty("footer")]
        public object Footer { get; set; }

        [JsonProperty("rendering_options")]
        public object RenderingOptions { get; set; }
    }


}
