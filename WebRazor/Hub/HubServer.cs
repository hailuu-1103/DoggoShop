using Microsoft.AspNetCore.SignalR;

namespace WebRazor
{
    public class HubServer: Hub
    {
        public void HasNewData()
        {
            Clients.All.SendAsync("Reload");
        }
    }
}
