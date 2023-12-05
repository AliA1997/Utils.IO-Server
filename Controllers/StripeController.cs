using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Newtonsoft.Json;
using Stripe;
using Utils.IO.Server.Models;
using static Postgrest.Constants;

namespace Utils.IO.Server.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    //[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    public class StripeController : Controller
    {
        
        private readonly ILogger<StripeController> Logger;
        private readonly StripeConfiguration StripeConfiguration;
        private readonly Supabase.Client SupabaseClient;
        public StripeController(
            ILogger<StripeController> logger, 
            StripeConfiguration stripeConfiguration,
            Supabase.Client supabaseClient)
        {
            Logger = logger;
            StripeConfiguration = stripeConfiguration;
            SupabaseClient = supabaseClient;
        }

        [HttpGet]
        public IActionResult Test()
        {
            return Ok("Test endpoint hit!");
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], StripeConfiguration.WebhookSecret, 300, false);
                var stripeEventString = stripeEvent.Data.Object.ToString();
                int jsonStartIndex = stripeEventString.IndexOf("{");
                int jsonEndIndex = stripeEventString.LastIndexOf("}");
                string stripeEventJson = stripeEventString.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);
                var metadata = JsonConvert.DeserializeObject<StripeMetadata>(stripeEventJson);
                if (metadata == null) throw new Exception("Invalid Stripe Event");
                metadata.Email = "novigo.ali@gmail.com";
                var allUserSubscriptions = await SupabaseClient.From<UserSubscription>().Get();
                var userSubscription = allUserSubscriptions.Models.FirstOrDefault(uSub => uSub.UserName == metadata.Email.ToString());

                if (userSubscription == null) throw new Exception("User doesn't exist.");
                // Handle the event
                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    await IndicateMonthHasBeenPaid(userSubscription);
                }
                else if (stripeEvent.Type == Events.CustomerSubscriptionCreated)
                {
                    await StartUserSubscription(userSubscription);
                }
                else if (stripeEvent.Type == Events.PaymentIntentCanceled || stripeEvent.Type == Events.PaymentIntentPaymentFailed)
                {
                   await IndicateMonthHasNotBeenPaid(userSubscription);
                }
                // ... handle other event types
                else
                {
                    // Unexpected event type
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }
                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest();
            }
        }

        private async Task StartUserSubscription(UserSubscription userSubscription)
        {
            var userSubscriptionPlanId = userSubscription.Subscription ?? 0; 
            var userSubscriptionPlan = await SupabaseClient.From<UtilsIOSubscription>().Where(x => x.Id == userSubscriptionPlanId).Single();
            var updateUserSubscriptionPlanRequest = new UpdateUserSubscriptionRequest();

            if (userSubscriptionPlan == null) throw new Exception("User subscription plan not found.");
            if (!string.IsNullOrEmpty(userSubscriptionPlan.SubscriptionName)) updateUserSubscriptionPlanRequest.NumberOfBasicRequests = 1500;
            if (userSubscriptionPlan.SubscriptionName == "Chatbot" || userSubscriptionPlan.SubscriptionName == "Web3") updateUserSubscriptionPlanRequest.NumberOfWeb3Requests = 3000;
            if (userSubscriptionPlan.SubscriptionName == "Chatbot") updateUserSubscriptionPlanRequest.NumberOfChatbotRequests = 5000;

            await SupabaseClient.From<UserSubscription>()
                    .Where(x => x.Id == userSubscription.Id)
                    .Set(x => x.TrialStart, DateTimeOffset.Now)
                    .Set(x => x.TrialEnd, DateTimeOffset.Now.AddDays(7))
                    .Set(x => x.BasicRequestsLeft, updateUserSubscriptionPlanRequest.NumberOfBasicRequests)
                    .Set(x => x.Web3RequestsLeft, updateUserSubscriptionPlanRequest.NumberOfWeb3Requests)
                    .Set(x => x.ChatbotRequestsLeft, updateUserSubscriptionPlanRequest.NumberOfChatbotRequests)
                    .Update();
        }

        private async Task IndicateMonthHasBeenPaid(UserSubscription userSubscription)
        {
            if(userSubscription.TrialFinished == null)
            {
                await SupabaseClient.From<UserSubscription>()
                                    .Where(x => x.Id == userSubscription.Id)
                                    .Set(x => x.TrialFinished, true)
                                    .Set(x => x.MonthPaid, true)
                                    .Update();
            }
            else
            {
                await SupabaseClient.From<UserSubscription>()
                                    .Where(x => x.Id == userSubscription.Id)
                                    .Set(x => x.MonthPaid, true)
                                    .Update();
            }
        }

        private async Task IndicateMonthHasNotBeenPaid(UserSubscription userSubscription)
        {
            if (userSubscription.TrialFinished == null)
            {
                await SupabaseClient.From<UserSubscription>()
                                    .Where(x => x.Id == userSubscription.Id)
                                    .Set(x => x.TrialFinished, true)
                                    .Set(x => x.MonthPaid, false)
                                    .Update();
            }
            else
            {
                await SupabaseClient.From<UserSubscription>()
                                    .Where(x => x.Id == userSubscription.Id)
                                    .Set(x => x.MonthPaid, false)
                                    .Update();
            }
        }
    }
}