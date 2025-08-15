using System;
using System.Collections.Generic;

namespace Order.WebAPI.DTOs
{
    public class CreateOrderRequest
    {
        public Guid ResellerId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid StatusId { get; set; }

        public List<CreateOrderItemRequest> Items { get; set; }
    }
}
