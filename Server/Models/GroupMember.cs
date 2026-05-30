using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Models
{
    public class GroupMember
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int GroupId { get; set; }
    }
}
