using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Text.Json;
using Client.Models;

namespace Client.Network
{
    public class ChatClient
    {
        private readonly TcpClient _client = new();
        // provides the underlying stream of data for network access over stream sockets
        private NetworkStream? _stream;

        public async Task ConnectAsync()
        {
            await _client.ConnectAsync("127.0.0.1", 777);
            _stream = _client.GetStream();
        }

        public async Task SendAsync(Packet packet)
        {
            string json = JsonSerializer.Serialize(packet);

            byte[] data = Encoding.UTF8.GetBytes(json);

            await _stream!.WriteAsync(data);
        }

        public async Task<T?> ReceiveAsync<T>()
        {
            byte[] buffer = new byte[8192];

            int count = await _stream!.ReadAsync(buffer);

            if (count == 0)
                return default;

            string json = Encoding.UTF8.GetString(buffer, 0, count);

            return JsonSerializer.Deserialize<T>(json);
        }

        public async Task<string?> ReceiveRawAsync()
        {
            byte[] buffer = new byte[8192];

            int count = await _stream!.ReadAsync(buffer);

            if (count == 0)
                return null;

            return Encoding.UTF8.GetString(buffer, 0, count);
        }
    }
}
