using System;
using System.Collections.Generic;

namespace JetAccess.Models.Services.JetRestService.GetOrderWithOutShipmentDetail
{
	internal class GetOrderWithoutShipmentDetailResponse
	{
		public string MerchantOrderId{ get; set; }
		public string ReferenceOrderId{ get; set; }
		public string FulFillmentNode{ get; set; }
		public DateTime OrderPlacedDate{ get; set; }
		public DateTime OrderTransmitionDate{ get; set; }
		public string Status{ get; set; }
		public IEnumerable< OrderItem > OrderItems{ get; set; }
		public bool HasShipments{ get; set; }
		public string ShippingToAddress1{ get; set; }
		public string ShippingToAddress2{ get; set; }
		public string ShippingToCity{ get; set; }
		public string ShippingToSate{ get; set; }
		public string ShippingToZipCode{ get; set; }
	}

	internal class OrderItem
	{
		public decimal BasePrice{ get; set; }
		public decimal ItemShippingCost{ get; set; }
		public decimal ItemShippingTax{ get; set; }
		public decimal ItemTax{ get; set; }
		public string OrderItemId{ get; set; }
		public string MerchantSku{ get; set; }
		public decimal RequestOrderQuantity{ get; set; }
		public string ProductTitle{ get; set; }
		public string Url{ get; set; }
	}
}