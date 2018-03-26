using FreeMarket.Models;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FreeMarket.Controllers
{
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
                    if (defaultSize == PictureSize.Large)
                        return File(System.Web.HttpContext.Current.Server.MapPath("~/Content/Images/defaultPicture.png"), "image/png");
                    else if (defaultSize == PictureSize.Small)
                        return File(System.Web.HttpContext.Current.Server.MapPath("~/Content/Images/defaultPictureSmall.png"), "image/png");
                    else
                        return File(System.Web.HttpContext.Current.Server.MapPath("~/Content/Images/defaultPictureMedium.png"), "image/png");
                }

                photoBack = item.Picture;
            }
            catch (System.Exception ex)
            {
                ExceptionLogging.LogExceptionAsync(ex);
            }

            return File(photoBack, "image/png");
        }

        public async Task<ActionResult> RenderDepartmentImage(int id, PictureSize defaultSize = PictureSize.Large)
        {
            byte[] photoBack = new byte[5];

            try
            {
                DepartmentPicture item = await db.DepartmentPictures.FindAsync(id);

                if (item == null || item.Picture == null)
                {
                    if (defaultSize == PictureSize.Large)
                        return File(System.Web.HttpContext.Current.Server.MapPath("~/Content/Images/defaultPicture.png"), "image/png");
                    else if (defaultSize == PictureSize.Small)
                        return File(System.Web.HttpContext.Current.Server.MapPath("~/Content/Images/defaultPictureSmall.png"), "image/png");
                    else
                        return File(System.Web.HttpContext.Current.Server.MapPath("~/Content/Images/defaultPictureMedium.png"), "image/png");
                }

                photoBack = item.Picture;
            }
            catch (System.Exception ex)
            {
                ExceptionLogging.LogExceptionAsync(ex);
            }

            return File(photoBack, "image/png");
        }

        public async Task<ActionResult> RenderExternalWebsiteImage(int id, PictureSize defaultSize = PictureSize.Large)
        {
            byte[] photoBack = new byte[5];

            try
            {
                ExternalWebsitePicture item = await db.ExternalWebsitePictures.FindAsync(id);

                if (item == null || item.Picture == null)
                {
                    if (defaultSize == PictureSize.Large)
                        return File(System.Web.HttpContext.Current.Server.MapPath("~/Content/Images/defaultPicture.png"), "image/png");
                    else if (defaultSize == PictureSize.Small)
                        return File(System.Web.HttpContext.Current.Server.MapPath("~/Content/Images/defaultPictureSmall.png"), "image/png");
                    else
                        return File(System.Web.HttpContext.Current.Server.MapPath("~/Content/Images/defaultPictureMedium.png"), "image/png");
                }

                photoBack = item.Picture;
            }
            catch (System.Exception ex)
            {
                ExceptionLogging.LogExceptionAsync(ex);
            }

            return File(photoBack, "image/png");
        }
    }
}