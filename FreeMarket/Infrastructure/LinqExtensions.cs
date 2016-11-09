using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FreeMarket.Infrastructure
{
    public static class LinqExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static IQueryable<TSource> WhereLike<TSource>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, string>> valueSelector,
            string value,
            char wildcard)
        {
            return source.Where(BuildLikeExpression(valueSelector, value, wildcard));
        }

        public static Expression<Func<TElement, bool>> BuildLikeExpression<TElement>(
            Expression<Func<TElement, string>> valueSelector,
            string value,
            char wildcard)
        {
            if (valueSelector == null)
                throw new ArgumentNullException("valueSelector");

            var method = GetLikeMethod(value, wildcard);

            value = value.Trim(wildcard);
            var body = Expression.Call(valueSelector.Body, method, Expression.Constant(value));

            var parameter = valueSelector.Parameters.Single();
            return Expression.Lambda<Func<TElement, bool>>(body, parameter);
        }

        private static MethodInfo GetLikeMethod(string value, char wildcard)
        {
            var methodName = "Contains";

            var textLength = value.Length;
            value = value.TrimEnd(wildcard);
            if (textLength > value.Length)
            {
                methodName = "StartsWith";
                textLength = value.Length;
            }

            value = value.TrimStart(wildcard);
            if (textLength > value.Length)
            {
                methodName = (methodName == "StartsWith") ? "Contains" : "EndsWith";
                textLength = value.Length;
            }

            var stringType = typeof(string);
            return stringType.GetMethod(methodName, new Type[] { stringType });
        }
    }
}