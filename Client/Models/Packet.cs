using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Models
{
    public class Packet
    {
        public string Type { get; set; } = "";

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? To { get; set; }

        public string? Group { get; set; }

        public string? Message { get; set; }

        public string? UserToAdd { get; set; }
    }
}
