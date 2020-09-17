using KtTest.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KtTest.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, int>
    {
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestItem> TestItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<QuestionCategory> QuestionCategories { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<UserTest> UserTests { get; set; }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Question>().HasOne(x => x.Answer).WithOne().IsRequired();
            builder.Entity<ChoiceAnswer>().HasMany(x => x.Choices).WithOne().HasForeignKey(x => x.ChoiceAnswerId);
            builder.Entity<WrittenAnswer>();

            builder.Entity<TestItem>().HasKey(x => new { x.TestId, x.QuestionId });
            builder.Entity<Test>().HasMany(x => x.TestItems).WithOne(x => x.Test).HasForeignKey(x => x.TestId);
            builder.Entity<Question>().HasMany(x => x.TestItems).WithOne(x => x.Question).HasForeignKey(x => x.QuestionId);

            builder.Entity<Category>().HasMany(x => x.QuestionCategories).WithOne(x => x.Category).HasForeignKey(x => x.CategoryId);
            builder.Entity<Question>().HasMany(x => x.QuestionCategories).WithOne(x => x.Question).HasForeignKey(x => x.QuestionId);
            builder.Entity<QuestionCategory>().HasKey(x => new { x.QuestionId, x.CategoryId });

            builder.Entity<UserAnswer>().HasKey(x => new { x.TestId, x.QuestionId, x.UserId });
            builder.Entity<WrittenUserAnswer>();
            builder.Entity<ChoiceUserAnswer>();

            builder.Entity<Invitation>().Property(x => x.InvitedBy).IsRequired();
            builder.Entity<Invitation>().Property(x => x.Email).IsRequired();
            builder.Entity<Invitation>().Property(x => x.Code).IsRequired();

            builder.Entity<Group>().HasMany(x => x.GroupMembers).WithOne(x=>x.Group).HasForeignKey(x=>x.GroupId);
            builder.Entity<AppUser>().HasMany(x => x.GroupMembers).WithOne(x => x.User).HasForeignKey(x => x.UserId);
            builder.Entity<GroupMember>().HasKey(x => new { x.GroupId, x.UserId });

            builder.Entity<Test>().HasMany(x => x.UserTests).WithOne(x => x.Test).HasForeignKey(x => x.TestId);
            builder.Entity<UserTest>().HasKey(x => new { x.UserId, x.TestId });
        }
    }
}
