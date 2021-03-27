using System.ComponentModel.DataAnnotations;

namespace Sirius.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}