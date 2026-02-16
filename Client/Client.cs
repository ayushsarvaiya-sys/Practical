using System.Net.Sockets;
using System.Text;

using var client = new TcpClient("localhost", 7000);
using var stream = client.GetStream();
using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
Console.WriteLine("Client Connected");

while (true)
{
    Console.Write("Enter file path: ");
    string path = Console.ReadLine()!;

    if(path.Equals("exit"))
        break;

    var fileId = Guid.NewGuid().ToString();
    var fileName = Path.GetFileName(path);
    var fileBytes = await File.ReadAllBytesAsync(path);

    int chunkSize = 10240;  // ChunkSize 10KB
    int totalChunks = (int)Math.Ceiling((double)fileBytes.Length / chunkSize);


    await writer.WriteLineAsync($"START|{fileId}|{fileName}|{totalChunks}|{fileBytes.Length}");

    for (int i = 0; i < totalChunks; i++)
    {
        var chunk = fileBytes.Skip(i * chunkSize).Take(chunkSize).ToArray();
        var base64 = Convert.ToBase64String(chunk);

        await writer.WriteLineAsync($"CHUNK|{fileId}|{i + 1}|{base64}");
    }

    await writer.WriteLineAsync($"END|{fileId}");

    Console.WriteLine("Upload Completed");
}
Console.WriteLine("Client Disconnected");