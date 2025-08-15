using Microsoft.AspNetCore.Mvc;
using System;

namespace Order.WebAPI.DTOs
{
    public class UpdateOrderStatusRequest
    {
        [FromRoute]
        public Guid OrderId { get; set; }

        [FromRoute]
        public string NewStatus { get; set; } = string.Empty;
    }
}