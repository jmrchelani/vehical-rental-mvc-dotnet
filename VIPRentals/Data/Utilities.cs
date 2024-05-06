using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VIPRentals.Models;

namespace VIPRentals.Data
{
    public class Utilities
    {
        public static string UploadFile(HttpContext context, string envPath)
        {
            string uniqueFileName = null, filePath = null;
            var files = context.Request.Form.Files;
            var file = files[0];
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(envPath, "images");
                var extension = Path.GetExtension(files[0].FileName);
                uniqueFileName = fileName + extension;
                filePath = Path.Combine(uploads, fileName + extension);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }
                filePath = Path.Combine("images", uniqueFileName);
            }

            return filePath;
        }


        public static string getImageSrc(string image, [FromServices]HttpContext context)
        {
            return context.Request.Scheme + "://" + context.Request.Host + "/" + image;
        }

        public static string GetUserId([FromServices]UserManager<UserModel> userManager, [FromServices]HttpContext context)
        {
            return userManager.GetUserId(context.User);
        }

        public static string getNameFromId([FromServices]UserManager<UserModel> userManager, string id)
        {
            var user = userManager.FindByIdAsync(id).Result;
            return user.FullName;
        }
        public static string getFullName([FromServices] UserManager<UserModel> userManager, [FromServices] HttpContext context)
        {
            var user = userManager.GetUserAsync(context.User).Result;
            return user.FullName;
        }
        public static string getNameFromUsername([FromServices] UserManager<UserModel> userManager, string username)
        {
            var user = userManager.FindByNameAsync(username).Result;
            return user.FullName;
        }

    }
}
