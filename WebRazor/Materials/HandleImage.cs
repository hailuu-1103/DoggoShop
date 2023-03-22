using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebRazor.Materials
{
    public class HandleImage
    {
        string convertImageToDisplay(Byte[] image)
        {
            if (image == null)
            {
                return "";
            }
            var base64 = Convert.ToBase64String(image);
            return  string.Format("data:img/jpg;base64,{0}", base64);
        }
    }
}
