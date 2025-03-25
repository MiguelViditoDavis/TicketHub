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

            // Get Azure Storage Queue settings from appsettings.json
            var connectionString = _config["AzureStorage:ConnectionString"];
            var queueName = _config["AzureStorage:QueueName"];

            // Connect to the Azure queue
            var client = new QueueClient(connectionString, queueName);
            await client.CreateIfNotExistsAsync();

            // Serialize and Base64 encode the message
            var json = JsonSerializer.Serialize(purchase);
            var base64Message = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));

            // Send message to the queue
            await client.SendMessageAsync(base64Message);

            return Ok(new { message = "Purchase successfully queued." });
        }
    }
}
