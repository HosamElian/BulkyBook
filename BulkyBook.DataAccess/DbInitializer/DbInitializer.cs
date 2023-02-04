using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }
        public void Initialize()
        {
            // Migrations
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0) 
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception)
            {

                
            }
            // Create Roles if Not Created
            if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Comp)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Indi)).GetAwaiter().GetResult();
            }
            // Create Admin User
            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "Admin@dotnetmastery.com",
                Email = "Admin@dotnetmastery.com",
                Name = "Hossam Mahmoud",
                PhoneNumber = "01004014619",
                StreetAddress = "Al-mesala",
                State = "Egypt",
                PostalCode = "11111",
                City = " Fayuom"
            }, "Admin123*").GetAwaiter().GetResult();

            ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "Admin@dotnetmastery.com");
            _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            return;
        }
    }
}
