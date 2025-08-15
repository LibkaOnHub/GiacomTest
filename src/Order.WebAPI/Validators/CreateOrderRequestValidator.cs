using FluentValidation;
using Order.WebAPI.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Order.Service.Validators
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        private readonly IOrderService _orderService;

        public CreateOrderRequestValidator(IOrderService orderService)
        {
            _orderService = orderService;

            RuleFor(x => x.StatusId)
                .NotEmpty()
                .MustAsync(StatusExists).WithMessage("Status with the given Id does not exist.");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Order must contain at least one item.")
                .ForEach(item => item.SetValidator(new OrderItemValidator(_orderService)));
        }

        private async Task<bool> StatusExists(Guid statusId, CancellationToken cancellationToken)
        {
            return await _orderService.CheckStatusExistsAsync(statusId);
        }
    }
}