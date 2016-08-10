namespace FreeMarket.Models
{
    public enum PictureSize
    {
        Large,
        Medium,
        Small
    }

    public enum FreeMarketResult
    {
        Success,
        Exception,
        Failure,
        NoResult
    }

    public class FreeMarketObject
    {
        public FreeMarketResult Result { get; set; }
        public object Argument { get; set; }

        public FreeMarketObject()
        {
            Result = FreeMarketResult.NoResult;
            Argument = null;
        }
    }
}