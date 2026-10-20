using System.Linq.Expressions;
using System.Reflection;

namespace SubscriptionCommerce.Modules.Customers.UnitTests.Abstractions;

internal static class StateExtensions
{
    internal static T Set<T, TValue>(this T entity, Expression<Func<T, TValue>> property, TValue value)
    {
        var member = (PropertyInfo)((MemberExpression)property.Body).Member;

        member.SetValue(entity, value);

        return entity;
    }
}
