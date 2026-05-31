using Client.Models;
using Client.Network;
using System.Text.Json;

namespace Client;

public partial class LoginForm : Form
{
    private readonly ChatClient _client = new();

    public LoginForm()
    {
        InitializeComponent();
    }

    private async void Form1_Load(object sender, EventArgs e)
    {
        try
        {
            await _client.ConnectAsync();
        }
        catch
        {
            MessageBox.Show("Cannot connect to server");
            Close();
        }
    }


    private async void btnLogin_Click(object sender, EventArgs e)
    {
        bool success = await Login();

        if (!success)
            return;

        ChatForm chat = new ChatForm(
            txtLogin.Text,
            _client);

        Hide(); // Hides a window after login

        chat.ShowDialog();

        Close();
    }

    private async void btnRegister_Click(object sender, EventArgs e)
    {
        bool success = await Register();

        if (!success)
            return;

        MessageBox.Show("Now login with your account");
    }

    private async Task<bool> Login()
    {
        if (string.IsNullOrWhiteSpace(txtLogin.Text) ||
            string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            MessageBox.Show("Fill all fields");
            return false;
        }

        await _client.SendAsync(new Packet
        {
            Type = "login",
            Username = txtLogin.Text,
            Password = txtPassword.Text
        });

        var response =
            await _client.ReceiveAsync<ServerResponse>();

        if (response == null)
        {
            MessageBox.Show("Server unavailable");
            return false;
        }

        if (response.Message == "success")
            return true;

        MessageBox.Show("Invalid login or password");

        return false;
    }

    private async Task<bool> Register()
    {
        if (string.IsNullOrWhiteSpace(txtLogin.Text) ||
            string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            MessageBox.Show("Fill all fields");
            return false;
        }

        await _client.SendAsync(new Packet
        {
            Type = "register",
            Username = txtLogin.Text,
            Password = txtPassword.Text
        });

        var response =
            await _client.ReceiveAsync<ServerResponse>();

        if (response == null)
        {
            MessageBox.Show("Server unavailable");
            return false;
        }

        if (response.Message == "success")
        {
            MessageBox.Show("Registration successful");
            return true;
        }

        MessageBox.Show("User already exists");

        return false;
    }
}