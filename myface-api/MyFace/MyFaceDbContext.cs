using Microsoft.EntityFrameworkCore;
using MyFace.Models.Database;

namespace MyFace
{
    public class MyFaceDbContext : DbContext
    {
		public MyFaceDbContext()
		{
		}

		public MyFaceDbContext(DbContextOptions<MyFaceDbContext> options) : base(options) {}
        
        public DbSet<Post> Posts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Interaction> Interactions { get; set; }
    }
}