using Microsoft.AspNetCore.Mvc;

namespace Order.WebAPI.DTOs
{
    public class GetOrdersByStatusNameRequest
    {
        [FromRoute]
        public string StatusName { get; set; } = string.Empty;
    }
}