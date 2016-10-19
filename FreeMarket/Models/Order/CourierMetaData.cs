using System.ComponentModel.DataAnnotations;

namespace FreeMarket.Models
{
    [MetadataType(typeof(CourierMetaData))]
    public partial class Courier
    {
        public int CourierReviewsCount { get; set; }
    }
    public class CourierMetaData
    {
    }
}