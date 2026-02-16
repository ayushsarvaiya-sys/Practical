using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Prac
{
    public class WebSocketConnectionManager
    {
        private readonly List<WebSocket> _sockets = new();

        public void AddSocket(WebSocket socket)
        {
            _sockets.Add(socket);
        }

        public async Task BroadcastAsync(object data)
        {
            string json = JsonSerializer.Serialize(data);
            var bytes = Encoding.UTF8.GetBytes(json);

            foreach (var socket in _sockets.ToList())
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(
                        bytes,
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                }
            }
        }
    }
}
