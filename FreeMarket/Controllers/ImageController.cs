using FreeMarket.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FreeMarket.Controllers
{
    [RequireHttps]
    public class ImageController : Controller
    {
        public FreeMarketEntities db = new FreeMarketEntities();

        public async Task<ActionResult> RenderImage(int id, PictureSize defaultSize = PictureSize.Large)
        {
            byte[] photoBack = new byte[5];

            try
            {
                ProductPicture item = await db.ProductPictures.FindAsync(id);

                if (item == null || item.Picture == null)
                {
                    photoBack = db.SitePictures
                        .Where(c => c.Description == "defaultPicture")
                        .Select(c => c.Picture)
                        .FirstOrDefault();
                }

                photoBack = item.Picture;
            }
            catch (System.Exception ex)
            {
                ExceptionLogging.LogExceptionAsync(ex);

                if (defaultSize == PictureSize.Large)
                    return File(System.Web.HttpContext.Current.Server.MapPath("~/Content/Images/defaultPicture.png"), "image/png");
                else if (defaultSize == PictureSize.Small)
                    return File(System.Web.HttpContext.Current.Server.MapPath("~/Content/Images/defaultPictureSmall.png"), "image/png");
                else
                    return File(System.Web.HttpContext.Current.Server.MapPath("~/Content/Images/defaultPictureMedium.png"), "image/png");
            }

            return File(photoBack, "image/png");
        }

        //[Authorize(Roles="Admin")]
        //public async Task<ActionResult> Manage()
        //{

        //}
    }
}