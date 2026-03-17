using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstate.DTOs.Request;
using RealEstate.Models;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IsCompany = dto.IsCompany
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(new { message = "تم إنشاء الحساب بنجاح، يمكنك الآن تسجيل الدخول." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _signInManager.PasswordSignInAsync(dto.Email, dto.Password, dto.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            return Ok(new
            {
                message = "أهلاً بك في العقار الذكي",
                user = new { user.FirstName, user.LastName, user.Email, user.IsCompany }
            });
        }

        return Unauthorized(new { message = "الإيميل أو كلمة المرور غير صحيحة" });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { message = "تم تسجيل الخروج بنجاح" });
    }



    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return Ok(new { message = "إذا كان الحساب موجوداً، فستصلك رسالة" });

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        return Ok(new { token, message = "استخدم هذا الـ Token في صفحة تغيير الباسورد" });
    }








}