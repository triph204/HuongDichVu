using Microsoft.AspNetCore.SignalR;

namespace Order.API.Hubs
{
    public class OrderHub : Hub
    {
        public async Task SendOrderUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveOrderUpdate", message);
        }

        public async Task SendNewOrder(object orderData)
        {
            await Clients.All.SendAsync("ReceiveNewOrder", orderData);
        }

        public async Task SendOrderStatusChanged(object statusData)
        {
            await Clients.All.SendAsync("OrderStatusChanged", statusData);
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"?? Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"?? Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
