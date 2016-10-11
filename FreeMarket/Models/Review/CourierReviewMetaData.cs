using System.ComponentModel.DataAnnotations;

namespace FreeMarket.Models
{
    [MetadataType(typeof(CourierReviewMetaData))]
    public partial class CourierReview
    {
        public string CourierName { get; set; }
    }

    public class CourierReviewMetaData
    {
        [DataType(DataType.MultilineText)]
        public string ReviewContent { get; set; }
    }
}