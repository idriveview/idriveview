using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDriveView.Models
{
    class UserIDrive
    {
        [Required(ErrorMessage = "Name - обязательное поле")]
        [RegularExpression(@"^\S+$", ErrorMessage = "Name - не должнo содержать пробелы")]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "Длина Name должна быть от 3 до 15 символов")]
        public string Name { get; set; }


        [Required(ErrorMessage = "Email - обязательное поле")]
        [EmailAddress(ErrorMessage = "Что-то не так с Email")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Endpoint - обязательное поле")]
        [RegularExpression(@"^\S+$", ErrorMessage = "Endpoint не должен содержать пробелы")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "Что-то не так с Endpoint")]
        public string Endpoint { get; set; }


        [Required(ErrorMessage = "AccessKeyID - обязательное поле")]
        [RegularExpression(@"^\S+$", ErrorMessage = "AccessKeyID не должен содержать пробелы")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Что-то не так с AccessKeyID")]
        public string AccessKeyID { get; set; }


        [Required(ErrorMessage = "SecretAccessKey - обязательное поле")]
        [RegularExpression(@"^\S+$", ErrorMessage = "SecretAccessKey не должен содержать пробелы")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Что-то не так с SecretAccessKey")]
        public string SecretAccessKey { get; set; }


        [Required(ErrorMessage = "Password - обязательное поле")]
        [RegularExpression(@"^\S+$", ErrorMessage = "Password не должен содержать пробелы")]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "Password должен содержать от 4 дл 15 символов")]
        public string Password { get; set; }

        public string RememberMe { get; set; }

        public UserIDrive(string name, string email, string endpoint, string accessKeyID, string secretAccessKey, string password, string rememberMe)
        {
            Name = name;
            Email = email;
            Endpoint = endpoint;
            AccessKeyID = accessKeyID;
            SecretAccessKey = secretAccessKey;
            Password = password;
            RememberMe = rememberMe;
        }
    }
}
