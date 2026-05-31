using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Data
{
    // Fixing a bug with history saving via History/Server responses
    public class ChatMessageDto
    {
        public string Content { get; set; } = "";
    }
}
