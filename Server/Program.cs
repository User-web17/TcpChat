using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Server.Contexts;
using Server.Data;
using Server.Models;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;

// BCrypt.Net.BCrypt is a class within the popular BCrypt.
// Net-Next NuGet Package used for securely hashing and verifying passwords in .NET applications.
// It is an implementation of OpenBSD's Blowfish-based password hashing scheme.
// The library protects against brute-force attacks by integrating a randomized salt and an adjustable CPU work factor.

namespace Server
{
    public static class ConnectedClients
    {
        // a thread-safe collection of key/value pairs designed for multi-threaded scenarios
        // The main reason for using it,
        // is simply its manual locking in specialized atomic methods
        public static ConcurrentDictionary<string, TcpClient> Clients = new();
    }

    public class Program
    {

        static async Task Main(string[] args)
        {
            using (var db = new ChatDbContext())
            {
                db.Database.EnsureCreated();
            }

            using TcpListener server = new TcpListener(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 777));
            server.Start();
            Console.WriteLine($"Server is running: {server.LocalEndpoint}");

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
                _ = Task.Run(() => HandleClient(client));
            }
        }

        private static async Task HandleClient(TcpClient client)
        {
            string? currentUser = null;
            try
            {
                using var stream = client.GetStream();

                while (client.Connected)
                {
                    byte[] buffer = new byte[8192];

                    int count =
                        await stream.ReadAsync(buffer);

                    if (count == 0)
                        break;

                    string json =
                        Encoding.UTF8.GetString(
                            buffer, 0, count);

                    // the standard and most performant way to handle JSON data
                    Packet? packet =
                        JsonSerializer.Deserialize<Packet>(
                            json);

                    if (packet == null)
                        continue;

                    switch (packet.Type)
                    {
                        case "register":
                            bool success1 = await Register(packet);
                            await SendPacket(client,
                            new ServerResponse
                            {
                                Type = "register",
                                Message = success1 ? "success" : "exists"
                            });
                            break;

                        case "login":
                            bool success = await Login(packet, client);

                            if (success)
                                currentUser = packet.Username;

                            await SendPacket(client,
                            new ServerResponse
                            {
                                Type = "login",
                                Message = success ? "success" : "failed"
                            });

                            Console.WriteLine(
                                success
                                    ? $"{packet.Username} logged in"
                                    : $"Failed login: {packet.Username}");
                            break;

                        case "private":
                            await HandlePrivate(packet);
                            break;

                        case "create_group":
                            await CreateGroup(packet);
                            break;

                        case "group":
                            await HandleGroupMessage(packet);
                            break;

                        case "add_to_group":
                            await AddUserToGroup(packet);
                            break;

                        case "history":
                            await SendHistory(packet, client);
                            break;

                        case "group_history":
                            await SendGroupHistory(packet, client);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                client.Close();
                if (currentUser != null)
                {
                    // trying to remove a user,
                    // out value is a discard operator because then it becomes useless
                    ConnectedClients.Clients.TryRemove(
                        currentUser,
                        out _);
                }
            }
        }

        private static async Task<bool> Register(Packet packet)
        {
            if (string.IsNullOrWhiteSpace(packet.Username) ||
    string.IsNullOrWhiteSpace(packet.Password))
            {
                return false;
            }
            using var db = new ChatDbContext();

            bool exists =
                await db.Users.AnyAsync(x =>
                    x.Username == packet.Username);

            if (exists)
                return false;

            db.Users.Add(new User
            {
                // used to securely hash passwords in .NET applications.
                Username = packet.Username!,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                    packet.Password)
            });

            await db.SaveChangesAsync();
            return true;
        }

        private static async Task<bool> Login(Packet packet, TcpClient client)
        {
            if (string.IsNullOrWhiteSpace(packet.Username) ||
    string.IsNullOrWhiteSpace(packet.Password))
            {
                return false;
            }
            using var db = new ChatDbContext();

            var user = await db.Users
                .FirstOrDefaultAsync(x => x.Username == packet.Username);

            if (user == null)
                return false;

            bool success = BCrypt.Net.BCrypt.Verify(
                packet.Password,
                user.PasswordHash);

            if (success)
            {
                ConnectedClients.Clients.TryAdd(packet.Username!, client);
            }

            return success;
        }
        
        private static async Task SaveMessage(Packet packet)
        {

            using var db = new ChatDbContext();

            var sender = await db.Users.FirstOrDefaultAsync(x => x.Username == packet.Username);

            var receiver = await db.Users.FirstOrDefaultAsync(x => x.Username == packet.To);

            if (sender == null || receiver == null)
                return;

            db.PrivateMessages.Add(
            new PrivateMessage
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Content = packet.Message!
            });

            await db.SaveChangesAsync();
        }

        private static async Task HandlePrivate(Packet packet)
        {
            await SaveMessage(packet);

            if (ConnectedClients.Clients.TryGetValue(
                packet.To!,
                out TcpClient? receiver))
            {
                await SendPacket(receiver,
                    new ServerResponse
                    {
                        Type = "private",
                        From = packet.Username,
                        Message = packet.Message
                    });
            }
        }

        private static async Task SendPacket(TcpClient client, object packet)
        {
            string json =
                JsonSerializer.Serialize(packet);

            byte[] data =
                Encoding.UTF8.GetBytes(json);

            await client.GetStream()
                .WriteAsync(data);
        }

        private static async Task HandleGroupMessage(Packet packet)
        {
            using var db = new ChatDbContext();

            var sender =
                await db.Users.FirstOrDefaultAsync(
                    x => x.Username == packet.Username);

            var group =
                await db.Groups.FirstOrDefaultAsync(
                    x => x.Name == packet.Group);

            if (sender == null || group == null)
                return;

            db.GroupMessages.Add(
                new GroupMessage
                {
                    SenderId = sender.Id,
                    GroupId = group.Id,
                    Content = packet.Message!
                });

            await db.SaveChangesAsync();

            var members =
                await db.GroupMembers
                    .Where(x => x.GroupId == group.Id)
                    .ToListAsync();

            foreach (var member in members)
            {
                var user =
                    await db.Users
                        .FirstOrDefaultAsync(
                            x => x.Id == member.UserId);

                if (user == null)
                    continue;

                if (ConnectedClients.Clients.TryGetValue(
                    user.Username,
                    out TcpClient? client))
                {
                    await SendPacket(client,
                    new ServerResponse
                    {
                        Type = "group",
                        Group = packet.Group,
                        From = packet.Username,
                        Message = packet.Message
                    });
                }
            }
        }

        private static async Task CreateGroup(Packet packet)
        {
            using var db = new ChatDbContext();

            bool exists = await db.Groups
                .AnyAsync(x => x.Name == packet.Group);

            if (exists)
                return;

            db.Groups.Add(new ChatGroup
            {
                Name = packet.Group!
            });



            await db.SaveChangesAsync();

            var group = await db.Groups
    .FirstOrDefaultAsync(x => x.Name == packet.Group);

            var creator = await db.Users
                .FirstOrDefaultAsync(x => x.Username == packet.Username);

            if (group != null && creator != null)
            {
                db.GroupMembers.Add(new GroupMember
                {
                    UserId = creator.Id,
                    GroupId = group.Id
                });

                await db.SaveChangesAsync();
            }

            foreach (var client in ConnectedClients.Clients)
            {
                await SendPacket(client.Value, new ServerResponse
                {
                    Type = "group_created",
                    Group = packet.Group,
                    Message = "new_group"
                });
            }
        }

        private static async Task AddUserToGroup(Packet packet)
        {
            using var db = new ChatDbContext();

            var user = await db.Users
                .FirstOrDefaultAsync(x =>
                    x.Username == packet.UserToAdd);

            var group = await db.Groups
                .FirstOrDefaultAsync(x =>
                    x.Name == packet.Group);

            if (user == null)
            {
                Console.WriteLine("User not found");
                return;
            }

            if (group == null)
            {
                Console.WriteLine("Group not found");
                return;
            }

            bool exists =
                await db.GroupMembers.AnyAsync(x =>
                    x.UserId == user.Id &&
                    x.GroupId == group.Id);

            if (exists)
                return;

            db.GroupMembers.Add(
                new GroupMember
                {
                    UserId = user.Id,
                    GroupId = group.Id
                });

            await db.SaveChangesAsync();


        }

        private static async Task SendGroupHistory(
   Packet packet,
   TcpClient client)
        {
            using var db = new ChatDbContext();

            var group = await db.Groups
                .FirstOrDefaultAsync(x => x.Name == packet.Group);

            if (group == null)
                return;

            var messages = await db.GroupMessages
                .Where(x => x.GroupId == group.Id)
                .OrderBy(x => x.SentAt)
                .ToListAsync();

            await SendPacket(client, new HistoryResponse
            {
                Type = "group_history",
                Messages = messages.Select(x => new ChatMessageDto
                {
                    Content = x.Content
                }).ToList()
            });
        }

        private static async Task SendHistory(
    Packet packet,
    TcpClient client)
        {
            using var db = new ChatDbContext();

            var user1 =
                await db.Users.FirstOrDefaultAsync(
                    x => x.Username == packet.Username);

            var user2 =
                await db.Users.FirstOrDefaultAsync(
                    x => x.Username == packet.To);

            if (user1 == null || user2 == null)
                return;

            var messages =
                await db.PrivateMessages
                    .Where(x =>
                        (x.SenderId == user1.Id &&
                         x.ReceiverId == user2.Id)
                         ||
                        (x.SenderId == user2.Id &&
                         x.ReceiverId == user1.Id))
                    .OrderBy(x => x.SentAt)
                    .ToListAsync();

            await SendPacket(client, new HistoryResponse
            {
                Type = "history",
                Messages = messages.Select(x => new ChatMessageDto
                {
                    Content = x.Content
                }).ToList()
            });
        }
    }
}

