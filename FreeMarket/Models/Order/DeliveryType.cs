namespace FreeMarket.Models
{
    public class DeliveryType
    {
        public decimal CourierCost { get; set; }
        public decimal PostOfficeCost { get; set; }
        public decimal LocalCourierCost { get; set; }
        public string SelectedDeliveryType { get; set; }
        public bool VirtualOrder
        {
            get
            {
                return CourierCost == 0 && PostOfficeCost == 0 && LocalCourierCost == 0;
            }
            set { }
        }
    }
}