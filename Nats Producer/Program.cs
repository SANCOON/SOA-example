namespace Nats_Producer;
using NATS.Net;
using System.Threading.Tasks;

internal class Program
{
    static async Task Main(string[] args)
    {
        await using var client = new NatsClient(url: "nats", name: "sample producer");
        
        await client.PublishAsync<string>(subject: "login", data: "Hello from NATS!");
    }
}
