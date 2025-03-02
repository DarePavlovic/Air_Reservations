using AirReservationsApp.Data;
using AirReservationsApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirReservationsApp.Controllers
{

    public class UserController : Controller
    {
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ApplicationDbContext dbContext;
        public UserController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {

                var user = await userManager.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    var result = await signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        var roles = await userManager.GetRolesAsync(user);
                        if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else if (user.UserType == "Agent")
                        {
                            Console.WriteLine("Agent");

                            return RedirectToAction("Index", "Agent");
                        }
                        else if (user.UserType == "Viewer")
                        {
                            return RedirectToAction("Index", "Viewer");
                        }

                        return RedirectToAction("Login");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Wrong username or password!";
                        ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
                    }
                }
                else{
                    TempData["ErrorMessage"] = "Username doesn't exist!";
                    ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Adds security against CSRF attacks
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "User"); // Redirecting to Login page
        }
        

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var users = new User
                {
                    Name = model.Name,
                    Lastname = model.Lastname,
                    UserName = model.UserName,
                    UserType = model.UserType,
                    Email = $"{model.UserName}@gmail.com",
                    EmailConfirmed = true
                };
                Console.WriteLine(users);
                var result = await userManager.CreateAsync(users, model.Password);
                if (result.Succeeded)
                {
                    // Assign role based on user type
                    string role = model.UserType;
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }

                    await userManager.AddToRoleAsync(users, role);
                    Console.WriteLine("User created");
                    TempData["SuccessMessage"] = "User registered successfully!";
                    return RedirectToAction("Register");
                }
                Console.WriteLine("User not created");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error.Description); // Debug output
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                // await dbContext.Users.AddAsync(users);
                // await dbContext.SaveChangesAsync();
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var users = await dbContext.Users.ToListAsync();
            return View(users);
        }
    }
}