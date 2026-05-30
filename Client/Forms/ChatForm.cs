using Client.Models;
using Client.Network;
using Server.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace Client
{
    public partial class ChatForm : Form
    {
        private string? Prompt(string text)
        {
            return Microsoft.VisualBasic.Interaction.InputBox(
                text,
                "Input",
                "");
        }

        private readonly ChatClient _client;

        private readonly string _username;

        private string? _selectedChat;

        public ChatForm(
    string username,
    ChatClient client)
        {
            InitializeComponent();

            _username = username;
            _client = client;

            _ = ReceiveLoop();
        }

        private void lstChats_SelectedIndexChanged(
    object sender,
    EventArgs e)
        {
            if (lstChats.SelectedItem == null)
                return;


            _selectedChat =
                lstChats.SelectedItem.ToString();

            if (_selectedChat.StartsWith("GROUP:"))
            {
                _ = _client.SendAsync(new Packet
                {
                    Type = "group_history",
                    Group = _selectedChat.Replace("GROUP:", "")
                });
            }
            else
            {
                _ = _client.SendAsync(new Packet
                {
                    Type = "history",
                    Username = _username,
                    To = _selectedChat
                });
            }

            rtxtChatHistory.Clear();
        }

        private async void btnSend_Click(
    object sender,
    EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMessage.Text))
                return;

            if (string.IsNullOrWhiteSpace(_selectedChat))
            {
                MessageBox.Show(
                    "Select user");
                return;
            }

            if (_selectedChat.StartsWith("GROUP:"))
            {
                await _client.SendAsync(
                    new Packet
                    {
                        Type = "group",
                        Username = _username,
                        Group = _selectedChat.Replace("GROUP:", ""),
                        Message = txtMessage.Text
                    });

                rtxtChatHistory.AppendText(
                    $"[Me]: {txtMessage.Text}\n");
            }
            else
            {
                await _client.SendAsync(
                    new Packet
                    {
                        Type = "private",
                        Username = _username,
                        To = _selectedChat,
                        Message = txtMessage.Text
                    });

                rtxtChatHistory.AppendText(
                    $"Me: {txtMessage.Text}\n");
            }

            txtMessage.Clear();
        }

        private async Task ReceiveLoop()
        {
            while (true)
            {
                try
                {
                    string json = await _client.ReceiveRawAsync();

                    using JsonDocument doc = JsonDocument.Parse(json);

                    string type =
                        doc.RootElement.GetProperty("Type").GetString()!;

                    if (type == null)
                        continue;

                    if (type == "private")
                    {
                        var response =
                            JsonSerializer.Deserialize<ServerResponse>(json);

                        Invoke(() =>
                        {
                            rtxtChatHistory.AppendText(
                                $"<{response.From}>: {response.Message}\n");
                        });
                    }
                    else if (type == "group_created")
                    {
                        MessageBox.Show("group_created received");

                        var response =
                            JsonSerializer.Deserialize<ServerResponse>(json);

                        Invoke(() =>
                        {
                            string name = "GROUP:" + response.Group;

                            if (!lstChats.Items.Contains(name))
                                lstChats.Items.Add(name);
                        });
                    }
                    else if (type == "group")
                    {
                        var response =
                            JsonSerializer.Deserialize<ServerResponse>(json);

                        Invoke(() =>
                        {
                            if (_selectedChat == $"GROUP:{response.Group}")
                            {
                                rtxtChatHistory.AppendText(
                                    $"[{response.Group}] {response.From}: {response.Message}\n");
                            }
                        });
                    }
                    else if (type == "history")
                    {
                        var history =
                            JsonSerializer.Deserialize<HistoryResponse>(json);

                        Invoke(() =>
                        {
                            rtxtChatHistory.Clear();

                            foreach (var msg in history.Messages)
                            {
                                rtxtChatHistory.AppendText(
                                    $"{msg.Content}\n");
                            }
                        });
                    }
                    else if (type == "group_history")
                    {
                        var history =
                            JsonSerializer.Deserialize<HistoryResponse>(json);

                        Invoke(() =>
                        {
                            rtxtChatHistory.Clear();

                            foreach (var msg in history.Messages)
                            {
                                rtxtChatHistory.AppendText(
                                    $"{msg.Content}\n");
                            }
                        });
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        private void btnAddContact_Click(object sender, EventArgs e)
        {
            string user = Prompt("Enter username:")!;

            if (string.IsNullOrWhiteSpace(user))
                return;

            if (!lstChats.Items.Contains(user))
                lstChats.Items.Add(user);

            MessageBox.Show("Contact added");
        }

        private async void btnCreateGroup_Click(object sender, EventArgs e)
        {
            string group = Prompt("Enter group name:")!;

            if (string.IsNullOrWhiteSpace(group))
                return;

            string name = "GROUP:" + group;



            if (!lstChats.Items.Contains(name))
                lstChats.Items.Add(name);

            await _client.SendAsync(new Packet
            {
                Type = "create_group",
                Username = _username,
                Group = group
            });

            MessageBox.Show("Group created");
        }

        private async void btnAddToGroup_Click(object sender, EventArgs e)
        {
            string group = Prompt("Group name:")!;
            string user = Prompt("User to add:")!;

            if (string.IsNullOrWhiteSpace(group) ||
                string.IsNullOrWhiteSpace(user))
                return;

            await _client?.SendAsync(new Packet
            {
                Type = "add_to_group",
                Group = group,
                UserToAdd = user
            });

            MessageBox.Show("User added to group");
        }
    }
}
