using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Contexts
{
    public class ChatDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();

        public DbSet<PrivateMessage> PrivateMessages => Set<PrivateMessage>();

        public DbSet<ChatGroup> Groups => Set<ChatGroup>();

        public DbSet<GroupMember> GroupMembers => Set<GroupMember>();

        public DbSet<GroupMessage> GroupMessages => Set<GroupMessage>();

        protected override void OnConfiguring(
            DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=chat.db");
        }
    }
}
