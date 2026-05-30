using Server.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Data
{
    public class ServerResponse
    {
        public string Type { get; set; } = "";

        public string? From { get; set; }

        public string? Group { get; set; }

        public string? Message { get; set; }

    }
}
