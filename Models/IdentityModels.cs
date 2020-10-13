using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Lagsoba94.Areas.Vote.Models.Data;
using Lagsoba94.Models.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using static Lagsoba94.Models.DbContext;

namespace Lagsoba94.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class User : IdentityUser<int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OtherNames { get; set; }
        public string Profile { get; set; }
        public string Gender { get; set; }
        public System.DateTime? DOB { get; set; }
        public int AddressId { get; set; }
        public string Profession { get; set; }
        public bool Active { get; set; }
        public int OfficeId { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class DbContext : IdentityDbContext<User, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public DbContext()
            : base("DatabaseConnection")
        {
            //disable initializer
            Database.SetInitializer<DbContext>(null);
        }


        public static DbContext Create()
        {
            return new DbContext();
        }


        public class CustomUserRole : IdentityUserRole<int> { }
        public class CustomUserClaim : IdentityUserClaim<int> { }
        public class CustomUserLogin : IdentityUserLogin<int> { }
        public class CustomRole : IdentityRole<int, CustomUserRole>
        {
            public CustomRole() { }
            public CustomRole(string name) { Name = name; }
        }
        public class CustomUserStore : UserStore<User, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>
        {
            public CustomUserStore(DbContext context)
            : base(context)
            {
            }
        }

        public class CustomRoleStore : RoleStore<CustomRole, int, CustomUserRole>
        {
            public CustomRoleStore(DbContext context)
            : base(context)
            {
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("tblUsers").Property(p => p.Id).HasColumnName("UserId");
            modelBuilder.Entity<CustomUserRole>().ToTable("tblUserRoles");
            modelBuilder.Entity<CustomUserLogin>().ToTable("tblUserLogins");
            modelBuilder.Entity<CustomUserClaim>().ToTable("tblUserClaims").Property(p => p.Id).HasColumnName("UserClaimId");
            modelBuilder.Entity<CustomRole>().ToTable("tblRoles").Property(p => p.Id).HasColumnName("RoleId");
        }

        // Other Database Sets
        public DbSet<MembershipRequestDTO> MembershipRequest { get; set; }
        public DbSet<ImagesDTO> Images { get; set; }
        public DbSet<AddressesDTO> Address { get; set; }
        public DbSet<StatesDTO> States { get; set; }
        public DbSet<PageContentDTO> PageContent { get; set; }
        public DbSet<NewsDTO> News { get; set; }
        public DbSet<NewsTypeDTO> NewsType { get; set; }
        public DbSet<GalleryDTO> Gallery { get; set; }
        public DbSet<OfficeDTO> Office { get; set; }



        // Database Sets for VoteApp
        public DbSet<VoteUserDTO> VoteUsers { get; set; }
        public DbSet<PositionDTO> Position { get; set; }
        public DbSet<VoteDTO> Votes { get; set; }
        public DbSet<ElectionDTO> Election { get; set; }
    }   
}