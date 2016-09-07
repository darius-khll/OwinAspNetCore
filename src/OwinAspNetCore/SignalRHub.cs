using Microsoft.AspNet.SignalR;

namespace OwinAspNetCore
{
    public class SignalRHub : Hub
    {
        public SignalRHub(ISomeDependency someDependency)
        {

        }

        public void Send(string name, string message)
        {
            Clients.All.broadcastMessage(name, message);
        }
    }
}
