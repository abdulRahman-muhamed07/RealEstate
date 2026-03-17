using System.ComponentModel.DataAnnotations;

namespace RealEstate.DTOs.Request
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "الإيميل مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة الإيميل غير صحيحة")]
        public string Email { get; set; }

        [Required(ErrorMessage = "الـ Token مطلوب")]
        public string Token { get; set; }

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        [MinLength(6, ErrorMessage = "كلمة المرور يجب أن لا تقل عن 6 أحرف")]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "كلمة المرور غير متطابقة")]
        public string ConfirmPassword { get; set; }
    }
}
