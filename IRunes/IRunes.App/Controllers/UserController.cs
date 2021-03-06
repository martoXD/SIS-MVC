﻿using System;
using IRunes.Services.PasswordServices;
using IRunes.Services.UserCookieServices;
using IRunes.Services.UserServices;
using SIS.Framework.ActionResult.Contracts;
using SIS.Framework.Attributes.Methods;
using SIS.HTTP.Cookies;
using SIS.HTTP.Enums;
using SIS.HTTP.Requests.Contracts;
using SIS.HTTP.Responses.Contracts;

namespace IRunes.App.Controllers
{
    public class UserController : BaseController
    {
        private const string RegisterPage = "Register";
        private const string LoginPage = "Login";
        private readonly IUserService UserService;
        private readonly IPasswordService PasswordService;
        private readonly IUserCookieService UserCookieService;

        public UserController(IUserService userService, IPasswordService passwordService, IUserCookieService userCookieService)
        {
            this.UserService = userService;
            this.PasswordService = passwordService;
            this.UserCookieService = userCookieService;
        }

        [HttpGet]
        [HttpPost]
        public IActionResult Register()
        {
            if (this.Request.RequestMethod == HttpRequestMethod.Get)
            {
                this.Model["errorDisplay"] = "none";
                return this.View(RegisterPage);
            }

            var username = this.Request.FormData["username"].ToString().Trim();
            var password = this.Request.FormData["password"].ToString().Trim();
            var confirmPassword = this.Request.FormData["confirm-password"].ToString().Trim();
            var email = this.Request.FormData["email"].ToString().Trim();

            if (!IsValidUserData(username, password, confirmPassword, email))
            {
                
                this.Model["errorDisplay"] = "inline";
                return this.View(RegisterPage);
            }

            var hashedPassword = this.PasswordService.GenerateHash(password);

            this.UserService.Add(username, hashedPassword, email);


            var cookie = this.UserCookieService.GetUserCookie(username);

            this.Response.Cookies.Add(cookie);

            return this.View("Index-user"); ;
        }

        [HttpGet]
        [HttpPost]
        public IActionResult Login()
        {
            if (this.Request.RequestMethod == HttpRequestMethod.Get)
            {
                
                this.Model["errorDisplay"] = "none";
                return this.View(LoginPage);
            }

            var username = this.Request.FormData["username"].ToString().Trim();
            var password = this.Request.FormData["password"].ToString().Trim();

            if (!IsValidUserData(username, password))
            {
                
                this.Model["errorDisplay"] = "inline";
                return this.View(LoginPage);
            }

            var hashedPassword = this.PasswordService.GenerateHash(password);

            if (!this.UserService.Exsist(username, hashedPassword))
            {
                this.Model["errorDisplay"] = "inline";
                return this.View(LoginPage);
            }

            var cookie = this.UserCookieService.GetUserCookie(username);

            this.Response.Cookies.Add(cookie);

            return this.RedirectToAction("/Home/Index"); ;
        }

        [HttpGet]
        public IActionResult Logout(IHttpRequest request)
        {
            var authCookie = request.Cookies.GetCookie(HttpCookie.AuthenticeKey);
            authCookie.Expires = DateTime.UtcNow.AddDays(-1);

            this.Response.Cookies.Add(authCookie);

            return this.RedirectToAction("/Home/Index"); ;
        }

        private bool IsValidUserData(string username, string password, string confirmPassword, string email)
        {
            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(confirmPassword) && !string.IsNullOrEmpty(email) && password == confirmPassword;
        }

        private bool IsValidUserData(string username, string password)
        {
            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
        }
    }
}