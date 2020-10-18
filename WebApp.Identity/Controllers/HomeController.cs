using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using WebApp.Identity.Models;

namespace WebApp.Identity.Controllers
{
    //[ApiController]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<MyUser> userManager;
        private readonly IUserClaimsPrincipalFactory<MyUser> userClaimsPrincipalFactory;

        public HomeController(ILogger<HomeController> logger, UserManager<MyUser> userManager,
            IUserClaimsPrincipalFactory<MyUser> userClaimsPrincipalFactory)
        {
            _logger = logger;
            this.userManager = userManager;
            this.userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await this.userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var token = await this.userManager.GeneratePasswordResetTokenAsync(user);
                    var resetURL = Url.Action("ResetPassword", "Home", new { token = token, email = model.Email },
                        Request.Scheme);

                    //Teste com arquivo, depois incluir o envio de e-mail
                    System.IO.File.WriteAllText("resetLink.txt", resetURL);
                    return View("Success");
                }
                else
                {
                    return View("UsuarioNaoEncontrado");
                }
                //
            }
            return View();
        }

        [HttpGet]
        public IActionResult TwoFactor(string token, string email)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TwoFactor(TwoFactoryModel model)
        {
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.TwoFactorUserIdScheme);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Seu token expirou!");
                return View();
            }
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByIdAsync(result.Principal.FindFirstValue("sub"));
                if (user != null)
                {
                    var isValid = await this.userManager.VerifyTwoFactorTokenAsync(
                        user, result.Principal.FindFirstValue("amr"), 
                        model.Token
                        );
                    if (isValid)
                    {
                        await HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
                        var claimsPrincipal = await this.userClaimsPrincipalFactory.CreateAsync(user);
                        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal);

                        return RedirectToAction("About");
                    }
                    ModelState.AddModelError("", "Token Inválido");
                    return View();
                }
                ModelState.AddModelError("", "Invalid Request");
            }
            return View();
        }

            [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            return View(new ResetPaswordModel { Token = token, Email = email });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPaswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await this.userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await this.userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (!result.Succeeded)
                    {
                        foreach (var erro in result.Errors)
                        {
                            ModelState.AddModelError("", erro.Description);
                        }
                        return View();
                    }
                    return View("Success");
                }
                ModelState.AddModelError("", "Invalid Request");

            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await this.userManager.FindByNameAsync(model.UserName);
                if (user != null && await this.userManager.IsLockedOutAsync(user))
                {
                    //var identity = new ClaimsIdentity("Identity.Application");
                    //identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                    //identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
                    //await HttpContext.SignInAsync("Identity.Application", new ClaimsPrincipal(identity));
                    if (await userManager.CheckPasswordAsync(user, model.Password))
                    {

                        if (!await this.userManager.IsEmailConfirmedAsync(user))
                        {
                            ModelState.AddModelError("", "E-mail não foi validado. Confirme o cadastro em seu e-mail");
                            return View();
                        }
                        await this.userManager.ResetAccessFailedCountAsync(user);

                        if (await this.userManager.GetTwoFactorEnabledAsync(user))
                        {
                            var validator = await this.userManager.GetValidTwoFactorProvidersAsync(user);
                            if (validator.Contains("Email"))
                            {
                                var token = await this.userManager.GenerateTwoFactorTokenAsync(user, "Email");
                                System.IO.File.WriteAllText("email2sv.txt", token);

                                await HttpContext.SignInAsync(IdentityConstants.TwoFactorUserIdScheme,
                                    Store2FA(user.Id, "Email"));

                                return RedirectToAction("TwoFactor");
                            }
                        }

                        var principal = await this.userClaimsPrincipalFactory.CreateAsync(user);
                        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal); //Cookie Padrão
                        return RedirectToAction("About");
                    }
                    else
                    {
                        await this.userManager.AccessFailedAsync(user);

                        if (await this.userManager.IsLockedOutAsync(user))
                        {
                            //Enviar email informando o bloqueio, sugerindo alteração de senha.
                        }
                        ModelState.AddModelError("", "Senha incorreta!");
                    }
                }
                else
                {
                    if (user == null) 
                        ModelState.AddModelError("", "Usuário não encontrado!");
                    else
                        ModelState.AddModelError("", "Usuário bloqueado!");
                }

                ModelState.AddModelError("", "Usuário ou Senha inválida!");
            }
            return View();
        }

        public ClaimsPrincipal Store2FA(string userId, string provider)
        {
            var identity = new ClaimsIdentity(new List<Claim>
            {
                new Claim("sub", userId),
                new Claim("amr", provider)
            }, IdentityConstants.TwoFactorUserIdScheme);
            return new ClaimsPrincipal(identity);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult Success()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmailAddress(string token, string email)
        {
            var user = await this.userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var result = await this.userManager.ConfirmEmailAsync(user, token);

                if (result.Succeeded)
                {
                    return View("Success");
                }
            }
            return View("Error");
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await this.userManager.FindByNameAsync(model.UserName);
                if (user == null)
                {
                    user = new MyUser()
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = model.UserName,
                        Email = model.UserName
                    };

                    var result = await userManager.CreateAsync(user, model.Password);
                    if (result.Errors.Any())
                    {
                        return View();
                    }
                    if (result.Succeeded)
                    {
                        var token = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
                        var confirmationEmail = Url.Action("ConfirmEmailAddress", "Home",
                            new { token = token, email = user.Email }, Request.Scheme);
                        System.IO.File.WriteAllText("confirmationEmail.txt", confirmationEmail);
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                return View("Success");
            }
            return View();
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
