using FreeMarket.Models;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FreeMarket.Controllers
{
    public class ImageController : Controller
    {
        public FreeMarketEntities db = new FreeMarketEntities();

        public async Task<ActionResult> RenderImage(int id)
        {
            ProductPicture item = await db.ProductPictures.FindAsync(id);

            byte[] photoBack = item.Picture;

            return File(photoBack, "image/png");
        }
    }
}