﻿using System.IO.Compression;
using AnimeTickets.Models;
using AnimeTickets.Models.Static;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AnimeTickets.Controllers;

public class AccountController: Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly AppDbContext _context;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AppDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }
    
    public async Task<IActionResult> Users()
    {
        var users = await _context.Users.ToListAsync();
        return View(users);
    }

    public IActionResult Login()
    {
        var response = new LoginVM();
        return View(response);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginVM loginVm)
    {
        if (!ModelState.IsValid) return View(loginVm);

        var user = await _userManager.FindByEmailAsync(loginVm.EmailAddress);
        if (user != null)
        {
            var passwordCheck = await _userManager.CheckPasswordAsync(user, loginVm.Password);
            if (passwordCheck)
            {
                var result = await _signInManager.PasswordSignInAsync(user, loginVm.Password, false, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Movies");
                }
            }
            TempData["Error"] = "Wrong credentials. Try again !";
            return View(loginVm);
        }
        TempData["Error"] = "Wrong credentials. Try again !";
        return View(loginVm);
    }
    
    public IActionResult Register()
    {
        var response = new RegisterVM();
        return View(response);
    }

    [HttpPost]

    public async Task<IActionResult> Register(RegisterVM registerVm)
    {
        if (!ModelState.IsValid) return View(registerVm);

        var user = await _userManager.FindByEmailAsync(registerVm.EmailAddress);
        if (user != null)
        {
            TempData["Error"] = "This email address is already in use";
            return View(registerVm);
        }

        var newUser = new ApplicationUser()
        {
            FullName = registerVm.FullName,
            Email = registerVm.EmailAddress,
            UserName = registerVm.EmailAddress
        };
        var newUserResponse = await _userManager.CreateAsync(newUser, registerVm.Password);
        if (newUserResponse.Succeeded)
            await _userManager.AddToRoleAsync(newUser, UserRoles.User);
        return View("RegisterCompleted");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Movies");
    }

    public IActionResult AccessDenied(string ReturnUrl)
    {
        return View();
    }
    

}