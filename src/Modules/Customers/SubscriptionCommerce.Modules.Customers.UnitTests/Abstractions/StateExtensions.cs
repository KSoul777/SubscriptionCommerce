using System.Linq.Expressions;
using System.Reflection;

namespace SubscriptionCommerce.Modules.Customers.UnitTests.Abstractions;

// Sets identity ids, which in the running system only the database assigns.
internal static class StateExtensions
{
    internal static T Set<T, TValue>(this T entity, Expression<Func<T, TValue>> property, TValue value)
    {
        var member = (PropertyInfo)((MemberExpression)property.Body).Member;

        member.SetValue(entity, value);

        return entity;
    }
}
