using FluentValidation;
using Order.WebAPI.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Order.Service.Validators
{
    public class OrderItemValidator : AbstractValidator<CreateOrderItemRequest>
    {
        private readonly IOrderService _orderService;

        public OrderItemValidator(IOrderService orderService)
        {
            _orderService = orderService;

            RuleFor(x => x.ProductId)
                .NotEmpty()
                .MustAsync(ProductExists).WithMessage("Product with the given Id does not exist.");

            RuleFor(x => x.ServiceId)
                .NotEmpty()
                .MustAsync(ServiceExists).WithMessage("Service with the given Id does not exist.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        }

        private async Task<bool> ProductExists(Guid productId, CancellationToken cancellationToken)
        {
            return await _orderService.CheckProductExistsAsync(productId);
        }

        private async Task<bool> ServiceExists(Guid serviceId, CancellationToken cancellationToken)
        {
            return await _orderService.CheckServiceExistsAsync(serviceId);
        }
    }
}