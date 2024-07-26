using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
namespace Web.Hubs;

public class InteropHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}
