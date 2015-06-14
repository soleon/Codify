using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using Expression = System.Linq.Expressions.Expression;

namespace Codify.Windows.Converters
{
    public class ConditionalBooleanConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var @operator = Operator;
            if (@operator == null)
                throw new InvalidOperationException("Operator cannot be null.");
            @operator = @operator.Trim();
            if (@operator == string.Empty)
                throw new InvalidOperationException("Operator cannot be empty.");
            @operator = @operator.ToLower();

            Func<Expression, Expression, BinaryExpression> binaryExpressionFunc;
            switch (@operator)
            {
                case "==":
                    binaryExpressionFunc = Expression.Equal;
                    break;
                case "<":
                    binaryExpressionFunc = Expression.LessThan;
                    break;
                case ">":
                    binaryExpressionFunc = Expression.GreaterThan;
                    break;
                case ">=":
                    binaryExpressionFunc = Expression.GreaterThanOrEqual;
                    break;
                case "<=":
                    binaryExpressionFunc = Expression.LessThanOrEqual;
                    break;
                case "!=":
                    binaryExpressionFunc = Expression.NotEqual;
                    break;
                case "&&":
                    binaryExpressionFunc = Expression.AndAlso;
                    break;
                case "||":
                    binaryExpressionFunc = Expression.OrElse;
                    break;
                default:
                    throw new InvalidOperationException("Invalid operator value: " + Operator + ". Valid operators are: ==, !=, <, >, <=, >=, && and ||.");
            }
            return Expression.Lambda<Func<bool>>(binaryExpressionFunc(Expression.Constant(value), Expression.Constant(ComparisonTarget)), null).Compile()();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public object ComparisonTarget
        {
            get { return GetValue(ComparisonTargetProperty); }
            set { SetValue(ComparisonTargetProperty, value); }
        }

        public static readonly DependencyProperty ComparisonTargetProperty = DependencyProperty.Register("ComparisonTarget", typeof (object), typeof (ConditionalBooleanConverter));

        public string Operator
        {
            get { return (string) GetValue(OperatorProperty); }
            set { SetValue(OperatorProperty, value); }
        }

        public static readonly DependencyProperty OperatorProperty = DependencyProperty.Register("Operator", typeof (string), typeof (ConditionalBooleanConverter));
    }
}