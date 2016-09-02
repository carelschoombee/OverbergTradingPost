using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;

namespace FreeMarket.Infrastructure
{
    public class MinValueAttribute : ValidationAttribute, IClientValidatable
    {
        private readonly decimal _minValue;

        public MinValueAttribute(string minValue)
        {
            _minValue = Decimal.Parse(minValue, CultureInfo.InvariantCulture);
            ErrorMessage = "Enter a value greater than or equal to " + _minValue;
        }

        public override bool IsValid(object value)
        {
            return Convert.ToDecimal(value) >= _minValue;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule();
            rule.ErrorMessage = ErrorMessage;
            rule.ValidationParameters.Add("min", _minValue);
            rule.ValidationParameters.Add("max", Decimal.MaxValue);
            rule.ValidationType = "range";
            yield return rule;
        }

    }
}