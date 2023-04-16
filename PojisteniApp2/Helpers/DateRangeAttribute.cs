using System.ComponentModel.DataAnnotations;

namespace PojisteniApp2.Helpers
{
    public class DateRangeAttribute : ValidationAttribute
    {
        private readonly string _dateFromPropertyName;
        private readonly string _dateToPropertyName;

        public DateRangeAttribute(string dateFromPropertyName, string dateToPropertyName)
        {
            _dateFromPropertyName = dateFromPropertyName;
            _dateToPropertyName = dateToPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dateFromProperty = validationContext.ObjectType.GetProperty(_dateFromPropertyName);
            var dateToProperty = validationContext.ObjectType.GetProperty(_dateToPropertyName);

            if (dateFromProperty != null && dateToProperty != null)
            {
                var dateFrom = (DateTime?)dateFromProperty.GetValue(validationContext.ObjectInstance);
                var dateTo = (DateTime?)dateToProperty.GetValue(validationContext.ObjectInstance);

                if (dateFrom.HasValue && dateTo.HasValue && dateFrom > dateTo)
                {
                    return new ValidationResult(ErrorMessage ?? /*$"{_dateFromPropertyName} musí být menší nebo rovno {_dateToPropertyName}"*/ "Neplatný rozsah datumů");
                }
            }

            return ValidationResult.Success;
        }
    }
}
