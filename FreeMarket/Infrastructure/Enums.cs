using System.Diagnostics;

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

    public enum ReportType
    {
        OrderConfirmation,
        DeliveryInstructions,
        StruisbaaiOrderConfirmation,
        PostalConfirmation,
        PostalInstructions,
        Refund
    }

    public class FreeMarketObject
    {
        public FreeMarketResult Result { get; set; }
        public object Argument { get; set; }
        public string Message { get; set; }

        public FreeMarketObject()
        {
            Result = FreeMarketResult.NoResult;
            Argument = null;

            if (!string.IsNullOrEmpty(Message))
                Debug.Write(Message);
        }
    }
}