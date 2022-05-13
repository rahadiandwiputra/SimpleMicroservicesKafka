using HotChocolate.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrderService.Models;
using System.Security.Claims;

namespace OrderService.GraphQL
{
    public class Mutation
    {
        [Authorize]
        public async Task<OrderOutput> AddOrderAsync(
            OrderData input,
            ClaimsPrincipal claimsPrincipal,
            [Service] Latihan4Context context, [Service] IOptions<KafkaSettings> settings)
        {
            using var transaction = context.Database.BeginTransaction();
            var userName = claimsPrincipal.Identity.Name;

            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (user!=null)
            {

                input.Code = Guid.NewGuid().ToString();
                input.UserId = user.Id;
                var dts = DateTime.Now.ToString();
                var key = "order-" + dts;
                var val = JObject.FromObject(input).ToString(Formatting.None);/*JsonConvert.SerializeObject(input);*/
                var result = await KafkaHelper.SendMessage(settings.Value, "Latihan4", key, val);

                OrderOutput resp = new OrderOutput
                {
                    TransactionDate = dts,
                    Message = "Order was submitted successfully"
                };
                if (!result)
                    resp.Message = "Failed to submit data";
                return await Task.FromResult(resp);
            }
            
            OrderOutput respp = new OrderOutput
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = "Error, User Not Found"
            };
            return await Task.FromResult(respp);
        }
    }
}
