using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetAccess.Models.Services.JetRestService.GetOrderWithShipmentDetail;
using Newtonsoft.Json;

namespace JetAccess.Services.Parsers
{
	internal class GetOrderWithShipmentDetailResponseParser: JsonResponseParser< GetOrderWithShipmentDetailResponse >
	{
		private class ServerResponse
		{
#pragma warning disable 0649
			public string merchant_order_id;
			public string reference_order_id;
			public string fulfillment_node;
			public DateTime order_placed_date;
			public DateTime order_transmission_date;
			public string status;
			public OrderItem[] order_items;
#pragma warning restore 0649
		}

		private class OrderItem
		{
#pragma warning disable 0649
			public string order_item_id;
			public string merchant_sku;
			public decimal request_order_quantity;
			public item_price item_price;
			public string product_title;
			public string url;
#pragma warning restore 0649
		}

		private class item_price
		{
#pragma warning disable 0649
			public decimal base_price;
			public decimal item_tax;
			public decimal item_shipping_cost;
			public decimal item_shipping_tax;
#pragma warning restore 0649
		}

		private static class OrderConverter
		{
			public static GetOrderWithShipmentDetailResponse From( ServerResponse deserializeObject )
			{
				var res = new GetOrderWithShipmentDetailResponse
				{
					MerchantOrderId = deserializeObject.merchant_order_id,
					ReferenceOrderId = deserializeObject.reference_order_id,
					FulFillmentNode = deserializeObject.fulfillment_node,
					OrderPlacedDate = deserializeObject.order_placed_date,
					OrderTransmitionDate = deserializeObject.order_transmission_date,
					Created = deserializeObject.status,
					OrderItems = new List< Models.Services.JetRestService.GetOrderWithShipmentDetail.OrderItem >(),
				};

				for( var i = 0; i < deserializeObject.order_items.Count(); i++ )
				{
					var item = new Models.Services.JetRestService.GetOrderWithOutShipmentDetail.OrderItem
					{
						OrderItemId = deserializeObject.order_items[ i ].order_item_id,
						MerchantSku = deserializeObject.order_items[ i ].merchant_sku,
						RequestOrderQuantity = deserializeObject.order_items[ i ].request_order_quantity,
						ProductTitle = deserializeObject.order_items[ i ].product_title,
						Url = deserializeObject.order_items[ i ].url,
						BasePrice = deserializeObject.order_items[ i ].item_price.base_price,
						ItemShippingCost = deserializeObject.order_items[ i ].item_price.item_shipping_cost,
						ItemShippingTax = deserializeObject.order_items[ i ].item_price.item_shipping_tax,
						ItemTax = deserializeObject.order_items[ i ].item_price.item_tax,
					};

					( ( List< Models.Services.JetRestService.GetOrderWithOutShipmentDetail.OrderItem > )res.OrderItems ).Add( item );
				}

				return res;
			}
		}

		public override GetOrderWithShipmentDetailResponse Parse( Stream stream, bool keepStreamPos = true )
		{
			var streamPos = stream.Position;
			var streamReader = new StreamReader( stream );
			var streamStr = streamReader.ReadToEnd();
			var deserializeObject = JsonConvert.DeserializeObject< ServerResponse >( streamStr );

			if( keepStreamPos )
				stream.Seek( streamPos, SeekOrigin.Begin );

			return OrderConverter.From( deserializeObject );
		}
	}
}