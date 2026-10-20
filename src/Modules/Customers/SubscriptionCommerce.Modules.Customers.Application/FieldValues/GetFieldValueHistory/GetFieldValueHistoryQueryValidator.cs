using FluentValidation;

namespace SubscriptionCommerce.Modules.Customers.Application.FieldValues.GetFieldValueHistory;

internal sealed class GetFieldValueHistoryQueryValidator : AbstractValidator<GetFieldValueHistoryQuery>
{
    public GetFieldValueHistoryQueryValidator()
    {
        RuleFor(q => q.TenantId).GreaterThan(0);
        RuleFor(q => q.CustomerId).GreaterThan(0);
        RuleFor(q => q.FieldId).GreaterThan(0);
        RuleFor(q => q.After).GreaterThan(0).When(q => q.After is not null);
        RuleFor(q => q.Limit).InclusiveBetween(1, GetFieldValueHistoryQuery.MaxLimit);
    }
}
