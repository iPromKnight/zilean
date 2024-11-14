using System.Reflection;
using Zilean.Shared.Features.Expressions;

namespace Zilean.Shared.Extensions;

// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable CommentTypo
// ReSharper disable NotResolvedInText
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global

/// <summary>
/// Provides utility methods for building and combining predicates.
/// </summary>
public static class PredicateBuilder
{
    /// <summary>
    /// Combines two predicates using the logical AND operation.
    /// </summary>
    /// <typeparam name="T">Type of the object being evaluated.</typeparam>
    /// <param name="expr1">The first predicate.</param>
    /// <param name="expr2">The second predicate.</param>
    /// <returns>The combined predicate using the AND operation.</returns>
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
        return Expression.Lambda<Func<T, bool>>
            (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
    }

    /// <summary>
    /// Returns a predicate that always evaluates to false.
    /// </summary>
    /// <typeparam name="T">Type of the object being evaluated.</typeparam>
    /// <returns>A predicate that always returns false.</returns>
    public static Expression<Func<T, bool>> False<T>() => _ => false;

    /// <summary>
    /// Combines two predicates using the logical NAND operation.
    /// </summary>
    /// <typeparam name="T">Type of the object being evaluated.</typeparam>
    /// <param name="expr1">The first predicate.</param>
    /// <param name="expr2">The second predicate.</param>
    /// <returns>The combined predicate using the NAND operation.</returns>
    public static Expression<Func<T, bool>> Nand<T>(this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var andExpr = expr1.And(expr2);
        return andExpr.Not();
    }

    /// <summary>
    /// Combines two predicates using the logical NOR operation.
    /// </summary>
    /// <typeparam name="T">Type of the object being evaluated.</typeparam>
    /// <param name="expr1">The first predicate.</param>
    /// <param name="expr2">The second predicate.</param>
    /// <returns>The combined predicate using the NOR operation.</returns>
    public static Expression<Func<T, bool>> Nor<T>(this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var orExpr = expr1.Or(expr2);
        return orExpr.Not();
    }

    /// <summary>
    /// Negates a given predicate.
    /// </summary>
    /// <typeparam name="T">Type of the object being evaluated.</typeparam>
    /// <param name="expr">The predicate to negate.</param>
    /// <returns>The negated predicate.</returns>
    public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expr)
    {
        var notExpr = Expression.Not(expr.Body);
        return Expression.Lambda<Func<T, bool>>(notExpr, expr.Parameters);
    }

    /// <summary>
    /// Combines two predicates using the logical OR operation.
    /// </summary>
    /// <typeparam name="T">Type of the object being evaluated.</typeparam>
    /// <param name="expr1">The first predicate.</param>
    /// <param name="expr2">The second predicate.</param>
    /// <returns>The combined predicate using the OR operation.</returns>
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
        return Expression.Lambda<Func<T, bool>>
            (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
    }

    /// <summary>
    /// Returns a predicate that always evaluates to true.
    /// </summary>
    /// <typeparam name="T">Type of the object being evaluated.</typeparam>
    /// <returns>A predicate that always returns true.</returns>
    public static Expression<Func<T, bool>> True<T>() => _ => true;

    /// <summary>
    /// Combines two predicates using the logical XNOR operation.
    /// </summary>
    /// <typeparam name="T">Type of the object being evaluated.</typeparam>
    /// <param name="expr1">The first predicate.</param>
    /// <param name="expr2">The second predicate.</param>
    /// <returns>The combined predicate using the XNOR operation.</returns>
    public static Expression<Func<T, bool>> Xnor<T>(this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var xorExpr = expr1.Xor(expr2);
        return xorExpr.Not();
    }

    /// <summary>
    /// Combines two predicates using the logical XOR operation.
    /// </summary>
    /// <typeparam name="T">Type of the object being evaluated.</typeparam>
    /// <param name="expr1">The first predicate.</param>
    /// <param name="expr2">The second predicate.</param>
    /// <returns>The combined predicate using the XOR operation.</returns>
    public static Expression<Func<T, bool>> Xor<T>(this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var xorExpression = Expression.AndAlso(
            Expression.OrElse(Expression.Invoke(expr1, parameter), Expression.Invoke(expr2, parameter)),
            Expression.Not(Expression.AndAlso(Expression.Invoke(expr1, parameter), Expression.Invoke(expr2, parameter)))
        );
        return Expression.Lambda<Func<T, bool>>(xorExpression, parameter);
    }

    /// <summary>
    /// Checks if a specified property/field is null.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the property/field.</typeparam>
    /// <typeparam name="TProp">The type of the property/field to check for null.</typeparam>
    /// <param name="selector">Expression that selects the property/field to check.</param>
    /// <returns>An expression that represents the condition if the property/field is null.</returns>
    /// <remarks>This method generates an expression that checks if the selected property/field is null.</remarks>
    public static Expression<Func<T, bool>> IsNull<T, TProp>(this Expression<Func<T, TProp>> selector)
    {
        var body = Expression.Equal(selector.Body, Expression.Constant(null, typeof(TProp)));
        return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }

    /// <summary>
    /// Checks if a specified property/field is not null.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the property/field.</typeparam>
    /// <typeparam name="TProp">The type of the property/field to check for not null.</typeparam>
    /// <param name="selector">Expression that selects the property/field to check.</param>
    /// <returns>An expression that represents the condition if the property/field is not null.</returns>
    public static Expression<Func<T, bool>> IsNotNull<T, TProp>(this Expression<Func<T, TProp>> selector)
    {
        var body = Expression.NotEqual(selector.Body, Expression.Constant(null, typeof(TProp)));
        return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }

    /// <summary>
    /// Checks if a string property/field starts with a specific substring.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the property/field.</typeparam>
    /// <param name="selector">Expression that selects the string property/field to check.</param>
    /// <param name="substring">The substring to check for at the start of the string property/field.</param>
    /// <returns>An expression that represents the condition if the string property/field starts with the specified substring.</returns>
    public static Expression<Func<T, bool>> StartsWith<T>(this Expression<Func<T, string>> selector, string substring)
    {
        var method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) }) ?? throw new ArgumentNullException("typeof(string).GetMethod(\"StartsWith\", new[] { typeof(string) })");
        var body = Expression.Call(selector.Body, method, Expression.Constant(substring));
        return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }

    /// <summary>
    /// Checks if a string property/field ends with a specific substring.
    /// </summary>
    public static Expression<Func<T, bool>> EndsWith<T>(this Expression<Func<T, string>> selector, string substring)
    {
        var method = typeof(string).GetMethod("EndsWith", new[] { typeof(string) }) ?? throw new ArgumentNullException("typeof(string).GetMethod(\"EndsWith\", new[] { typeof(string) })");
        var body = Expression.Call(selector.Body, method, Expression.Constant(substring));
        return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }

    /// <summary>
    /// Checks if a string property/field contains a specific substring.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the string property/field.</typeparam>
    /// <param name="selector">Expression that selects the string property/field to check.</param>
    /// <param name="substring">The substring to check for within the string property/field.</param>
    /// <returns>An expression that represents the condition if the string property/field contains the specified substring.</returns>
    public static Expression<Func<T, bool>> Contains<T>(this Expression<Func<T, string>> selector, string substring)
    {
        var method = typeof(string).GetMethod("Contains", new[] { typeof(string) }) ?? throw new ArgumentNullException("typeof(string).GetMethod(\"Contains\", new[] { typeof(string) })");
        var body = Expression.Call(selector.Body, method, Expression.Constant(substring));
        return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }

    /// <summary>
    /// Checks if a numeric property/field is between two values.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the property/field.</typeparam>
    /// <param name="selector">Expression that selects the numeric property/field to check.</param>
    /// <param name="low">The lower bound of the range.</param>
    /// <param name="high">The upper bound of the range.</param>
    /// <returns>An expression that represents the condition if the numeric property/field is between the specified values.</returns>

    public static Expression<Func<T, bool>> Between<T>(this Expression<Func<T, int>> selector, int low, int high)
    {
        var greaterThanLow = Expression.GreaterThanOrEqual(selector.Body, Expression.Constant(low));
        var lessThanHigh = Expression.LessThanOrEqual(selector.Body, Expression.Constant(high));
        var body = Expression.AndAlso(greaterThanLow, lessThanHigh);
        return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }

    /// <summary>
    /// Checks if a collection property/field is empty.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the collection property/field.</typeparam>
    /// <typeparam name="TCollection">The type of the collection property/field.</typeparam>
    /// <param name="selector">Expression that selects the collection property/field to check.</param>
    /// <returns>An expression that represents the condition if the collection property/field is empty.</returns>
    public static Expression<Func<T, bool>> IsEmpty<T, TCollection>(this Expression<Func<T, ICollection<TCollection>>> selector)
    {
        var property = Expression.Property(selector.Body, "Count");
        var zero = Expression.Constant(0);
        var body = Expression.Equal(property, zero);
        return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }

    /// <summary>
    /// Checks if a collection property/field is not empty.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the collection property/field.</typeparam>
    /// <typeparam name="TCollection">The type of the collection property/field.</typeparam>
    /// <param name="selector">Expression that selects the collection property/field to check.</param>
    /// <returns>An expression that represents the condition if the collection property/field is not empty.</returns>
    public static Expression<Func<T, bool>> IsNotEmpty<T, TCollection>(this Expression<Func<T, ICollection<TCollection>>> selector)
    {
        var property = Expression.Property(selector.Body, "Count");
        var zero = Expression.Constant(0);
        var body = Expression.NotEqual(property, zero);
        return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }

    /// <summary>
    /// Checks if a value is in a given list of values.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the value property/field.</typeparam>
    /// <typeparam name="TValue">The type of the value property/field.</typeparam>
    /// <param name="selector">Expression that selects the value property/field to check.</param>
    /// <param name="values">The list of values to check against.</param>
    /// <returns>An expression that represents the condition if the value property/field is in the list of values.</returns>
    public static Expression<Func<T, bool>> In<T, TValue>(this Expression<Func<T, TValue>> selector, params TValue[] values)
    {
        var containsMethod = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TValue));

        var valuesExpr = Expression.Constant(values, values.GetType());
        var body = Expression.Call(null, containsMethod, valuesExpr, selector.Body);
        return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }

    /// <summary>
    /// Checks if a value is not in a given list of values.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the value property/field.</typeparam>
    /// <typeparam name="TValue">The type of the value property/field.</typeparam>
    /// <param name="selector">Expression that selects the value property/field to check.</param>
    /// <param name="values">The list of values to check against.</param>
    /// <returns>An expression that represents the condition if the value property/field is not in the list of values.</returns>
    public static Expression<Func<T, bool>> NotIn<T, TValue>(this Expression<Func<T, TValue>> selector, params TValue[] values)
    {
        var inExpression = In(selector, values);
        var body = Expression.Not(inExpression.Body);
        return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }

    /// <summary>
    /// Checks if a nullable property/field has a value.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the nullable property/field.</typeparam>
    /// <typeparam name="TProp">The type of the nullable property/field.</typeparam>
    /// <param name="selector">Expression that selects the nullable property/field to check.</param>
    /// <returns>An expression that represents the condition if the nullable property/field has a value.</returns>
    public static Expression<Func<T, bool>> HasValue<T, TProp>(this Expression<Func<T, TProp?>> selector) where TProp : struct
    {
        var property = Expression.Property(selector.Body, "HasValue");
        return Expression.Lambda<Func<T, bool>>(property, selector.Parameters);
    }

    /// <summary>
    /// Checks if a nullable property/field does not have a value.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the nullable property/field.</typeparam>
    /// <typeparam name="TProp">The type of the nullable property/field.</typeparam>
    /// <param name="selector">Expression that selects the nullable property/field to check.</param>
    /// <returns>An expression that represents the condition if the nullable property/field does not have a value.</returns>
    public static Expression<Func<T, bool>> DoesNotHaveValue<T, TProp>(this Expression<Func<T, TProp?>> selector) where TProp : struct
    {
        var hasValueExpression = HasValue(selector);
        var body = Expression.Not(hasValueExpression.Body);
        return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }

    /// <summary>
    /// Checks if a string property/field has a specific length.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the string property/field.</typeparam>
    /// <param name="selector">Expression that selects the string property/field to check.</param>
    /// <param name="length">The length to compare the string property/field with.</param>
    /// <returns>An expression that represents the condition if the string property/field has the specified length.</returns>
    public static Expression<Func<T, bool>> IsLength<T>(this Expression<Func<T, string>> selector, int length)
    {
        var lengthProperty = Expression.Property(selector.Body, "Length");
        var body = Expression.Equal(lengthProperty, Expression.Constant(length));
        return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }

    /// <summary>
    /// Checks if a numeric property/field is greater than a specific value.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the numeric property/field.</typeparam>
    /// <param name="selector">Expression that selects the numeric property/field to check.</param>
    /// <param name="value">The value to compare the numeric property/field with.</param>
    /// <returns>An expression that represents the condition if the numeric property/field is greater than the specified value.</returns>
    public static Expression<Func<T, bool>> IsGreaterThan<T>(this Expression<Func<T, int>> selector, int value)
    {
        var body = Expression.GreaterThan(selector.Body, Expression.Constant(value));
        return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }

    /// <summary>
    /// Checks if a numeric property/field is less than a specific value.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the numeric property/field.</typeparam>
    /// <param name="selector">Expression that selects the numeric property/field to check.</param>
    /// <param name="value">The value to compare the numeric property/field with.</param>
    /// <returns>An expression that represents the condition if the numeric property/field is less than the specified value.</returns>
    public static Expression<Func<T, bool>> IsLessThan<T>(this Expression<Func<T, int>> selector, int value)
    {
        var body = Expression.LessThan(selector.Body, Expression.Constant(value));
        return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }

    /// <summary>
    /// Checks if a DateTime property/field is before a specific date.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the DateTime property/field.</typeparam>
    /// <param name="selector">Expression that selects the DateTime property/field to check.</param>
    /// <param name="date">The date to compare the DateTime property/field with.</param>
    /// <returns>An expression that represents the condition if the DateTime property/field is before the specified date.</returns>
    public static Expression<Func<T, bool>> IsDateBefore<T>(this Expression<Func<T, DateTime>> selector, DateTime date)
    {
        var body = Expression.LessThan(selector.Body, Expression.Constant(date));
        return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }

    /// <summary>
    /// Checks if a DateTime property/field is after a specific date.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the DateTime property/field.</typeparam>
    /// <param name="selector">Expression that selects the DateTime property/field to check.</param>
    /// <param name="date">The date to compare the DateTime property/field with.</param>
    /// <returns>An expression that represents the condition if the DateTime property/field is after the specified date.</returns>
    public static Expression<Func<T, bool>> IsDateAfter<T>(this Expression<Func<T, DateTime>> selector, DateTime date)
    {
        var body = Expression.GreaterThan(selector.Body, Expression.Constant(date));
        return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }

    /// <summary>
    /// Checks if a string property/field matches a regex pattern.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the property/field.</typeparam>
    /// <param name="selector">Expression that selects the string property/field to check.</param>
    /// <param name="pattern">The regex pattern to match against the string property/field.</param>
    /// <returns>An expression that represents the condition if the string property/field matches the regex pattern.</returns>
    public static Expression<Func<T, bool>> MatchesRegex<T>(this Expression<Func<T, string>> selector, string pattern)
    {
        var method = typeof(Regex).GetMethod("IsMatch", new[] { typeof(string), typeof(string) });
        var regexBody = Expression.Call(method ?? throw new InvalidOperationException(), selector.Body, Expression.Constant(pattern));
        return Expression.Lambda<Func<T, bool>>(regexBody, selector.Parameters);
    }

    /// <summary>
    /// Checks if a DateTime property/field is within a specified duration from the current date/time.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the property/field.</typeparam>
    /// <param name="selector">Expression that selects the DateTime property/field to check.</param>
    /// <param name="duration">The duration within which the DateTime property/field should fall.</param>
    /// <returns>An expression that represents the condition if the DateTime property/field is within the specified duration from the current date/time.</returns>
    public static Expression<Func<T, bool>> WithinDuration<T>(this Expression<Func<T, DateTime>> selector, TimeSpan duration)
    {
        var now = DateTime.Now;
        var fromTime = now - duration;
        var toTime = now + duration;

        var greaterThanFrom = Expression.GreaterThanOrEqual(selector.Body, Expression.Constant(fromTime));
        var lessThanTo = Expression.LessThanOrEqual(selector.Body, Expression.Constant(toTime));

        var body = Expression.AndAlso(greaterThanFrom, lessThanTo);
        return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }

    /// <summary>
    /// Checks if two properties/fields of the same object are equal.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the properties/fields.</typeparam>
    /// <typeparam name="TValue">The type of the properties/fields to compare.</typeparam>
    /// <param name="selector1">Expression that selects the first property/field.</param>
    /// <param name="selector2">Expression that selects the second property/field.</param>
    /// <returns>An expression that represents the condition if the properties/fields are equal.</returns>
    /// <remarks>This method generates an expression that checks if the selected properties/fields are equal.</remarks>
    public static Expression<Func<T, bool>> PropertyEquals<T, TValue>(this Expression<Func<T, TValue>> selector1, Expression<Func<T, TValue>> selector2)
    {
        var body = Expression.Equal(selector1.Body, selector2.Body);
        return Expression.Lambda<Func<T, bool>>(body, selector1.Parameters);
    }

    /// <summary>
    /// Checks if two properties/fields of the same object are not equal.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the properties/fields.</typeparam>
    /// <typeparam name="TValue">The type of the properties/fields to compare.</typeparam>
    /// <param name="selector1">Expression that selects the first property/field.</param>
    /// <param name="selector2">Expression that selects the second property/field.</param>
    /// <returns>An expression that represents the condition if the properties/fields are not equal.</returns>
    public static Expression<Func<T, bool>> PropertyNotEquals<T, TValue>(this Expression<Func<T, TValue>> selector1, Expression<Func<T, TValue>> selector2)
    {
        var body = Expression.NotEqual(selector1.Body, selector2.Body);
        return Expression.Lambda<Func<T, bool>>(body, selector1.Parameters);
    }

    public static string ToReadableString<T>(this Expression<Func<T, bool>> expression) =>
        new ExpressionStringBuilder().Visit(expression).ToString();
}
