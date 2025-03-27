using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TicketHub.Models;

namespace TicketHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketPurchaseController : ControllerBase
    {
        private readonly IConfiguration _config;

        public TicketPurchaseController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> Purchase([FromBody] TicketPurchase purchase)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var connectionString = _config["AzureStorageConnectionString"];
            var queueName = "tickethub";

            if (string.IsNullOrEmpty(connectionString))
                return BadRequest("Missing Azure Storage connection string.");

            var client = new QueueClient(connectionString, queueName);
            await client.CreateIfNotExistsAsync();

            var json = JsonSerializer.Serialize(purchase);
            var base64Message = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));

            await client.SendMessageAsync(base64Message);

            return Ok(new { message = "Purchase successfully queued." });
        }
    }
}
