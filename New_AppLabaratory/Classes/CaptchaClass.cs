using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_AppLabaratory
{
    internal class CaptchaClass
    {
        public static string GenerateCaptcha()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            // string numbers = "0123456789";
            Random random = new Random();
            string captcha = new string(
            Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray());
            return captcha;
        }
    }
}
