// See https://aka.ms/new-console-template for more information
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OrderProcessor.Models;

Console.WriteLine("Hello, World!");

Console.WriteLine("Order Procesor App");

IConfiguration configuration = new ConfigurationBuilder()
      .AddJsonFile("appsettings.json", true, true)
      .Build();

var config = new ConsumerConfig
{
    BootstrapServers = configuration.GetSection("KafkaSettings").GetSection("Server").Value,
    GroupId = "tester",
    AutoOffsetReset = AutoOffsetReset.Earliest
};

var topic = "Latihan4";
CancellationTokenSource cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // prevent the process from terminating.
    cts.Cancel();
};

using (var consumer = new ConsumerBuilder<string, string>(config).Build())
{
    Console.WriteLine("Connected");
    consumer.Subscribe(topic);
    try
    {
        while (true)
        {
            var cr = consumer.Consume(cts.Token); // blocking
            Console.WriteLine($"Consumed record with key: {cr.Message.Key} and value: {cr.Message.Value}");

            // EF

            using (var context = new Latihan4Context())
            {
                Order order = JsonConvert.DeserializeObject<Order>(cr.Message.Value, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                });
                //var list= new List<order.OrderDetails>();
                
                foreach (var item in order.OrderDetails)
                {
                    var detial = new OrderDetail
                    {
                        OrderId = Convert.ToInt32( order.Id),
                        ProductId = Convert.ToInt32(item.ProductId),
                        Quantity = Convert.ToInt32(item.Quantity)
                    };
                    order.OrderDetails.Add(detial);
                }
                context.Orders.Add(order);
                context.SaveChanges();
                Console.WriteLine("Order Submitted");
            }
        }
    }
    catch (OperationCanceledException)
    {
        // Ctrl-C was pressed.
    }
    finally
    {
        consumer.Close();
    }

}