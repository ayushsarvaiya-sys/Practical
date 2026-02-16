using Prac.Models;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Prac
{
    public class UploadSession
    {
        public string FileId { get; set; } = "";
        public string FileName { get; set; } = "";
        public int TotalChunks { get; set; }
        public int ReceivedChunks { get; set; }
        public long TotalFileSize { get; set; }
        public FileStream FileStream { get; set; } = null!;
    }


    public class TcpFileUploadServer
    {
        private readonly WebSocketConnectionManager _wsManager;
        private readonly ConcurrentDictionary<string, UploadSession> _sessions = new();
        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        public TcpFileUploadServer(WebSocketConnectionManager wsManager)
        {
            _wsManager = wsManager;

            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);
        }

        public async Task StartAsync()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 7000);
            listener.Start();

            Console.WriteLine("TCP File Upload Server running on port 7000");

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClientAsync(client)); // MULTI CLIENT
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);

            try
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    await ProcessCommand(line);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
        }

        private async Task ProcessCommand(string line)
        {
            var parts = line.Split('|');
            var command = parts[0];

            switch (command)
            {
                case "START":
                    await HandleStart(parts);
                    break;

                case "CHUNK":
                    await HandleChunk(parts);
                    break;

                case "END":
                    await HandleEnd(parts);
                    break;
            }
        }

        private async Task HandleStart(string[] parts)
        {
            var fileId = parts[1];
            var fileName = parts[2];
            var totalChunks = int.Parse(parts[3]);
            var totalSize = long.Parse(parts[4]);

            var path = Path.Combine(_uploadPath, fileName);

            var session = new UploadSession
            {
                FileId = fileId,
                FileName = fileName,
                TotalChunks = totalChunks,
                TotalFileSize = totalSize,
                ReceivedChunks = 0,
                FileStream = new FileStream(path, FileMode.Create, FileAccess.Write)
            };

            _sessions[fileId] = session;

            await BroadcastProgress(session, "Uploading");
        }

        private async Task HandleChunk(string[] parts)
        {
            var fileId = parts[1];
            var base64Data = parts[3];

            if (!_sessions.TryGetValue(fileId, out var session))
                return;

            try
            {
                byte[] bytes = Convert.FromBase64String(base64Data);
                await session.FileStream.WriteAsync(bytes);

                session.ReceivedChunks++;

                await BroadcastProgress(session, "Uploading");
            }
            catch
            {
                Console.WriteLine("Invalid Base64");
            }
        }

        private async Task HandleEnd(string[] parts)
        {
            var fileId = parts[1];

            if (!_sessions.TryRemove(fileId, out var session))
                return;

            session.FileStream.Close();

            await BroadcastProgress(session, "Completed");
        }

        private async Task BroadcastProgress(UploadSession session, string status)
        {
            int progress = (int)((double)session.ReceivedChunks / session.TotalChunks * 100);

            var data = new UploadProgress
            {
                FileId = session.FileId,
                FileName = session.FileName,
                Progress = progress,
                Status = status
            };

            await _wsManager.BroadcastAsync(data);
        }

    }
}
