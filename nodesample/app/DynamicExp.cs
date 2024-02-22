using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    record User(string firstName, int age);
    internal class DynamicExp
    {
        public static void SampleExample()
        {
            var list = new List<User>
            {
               new User("jasim", 23),
               new User("arif", 18),
               new User("talha", 12)
            };
            var tokens = "(    first_name =    \"talha\"   ) or ( age <= 20 and first_name = arif)";
            foreach (var item in list.AsQueryable().Where(GetExpression<User>(tokens)))
            {
                Console.WriteLine(item.firstName);
            }
        }
        public static Expression<Func<T, bool>> GetExpression<T>(string str)
        {
            var paramExpression = Expression.Parameter(typeof(T), "u");

            try
            {
                var tokens = ConvertToPostfix(GetTokens(str));

                var stack = new Stack<Expression>();


                int i = 0, len = tokens.Count;
                foreach (var item in tokens)
                {
                    switch (item)
                    {
                        case "and":
                            var right = stack.Pop();
                            var left = stack.Pop();
                            var exp = Expression.And(left, right);
                            stack.Push(exp);
                            break;
                        case "or":
                            var right2 = stack.Pop();
                            var left2 = stack.Pop();
                            var exp2 = Expression.Or(left2, right2);
                            stack.Push(exp2);
                            break;
                        case "=":
                        case "==":
                        case "<":
                        case ">":
                        case "<=":
                        case ">=":
                            var exp22 = GetExpression<T>(tokens[i - 2], item, tokens[i - 1], paramExpression);
                            stack.Push(exp22);
                            break;
                    }
                    i++;
                }

                var binaryExpression = stack.Pop();
                var res = Expression.Lambda<Func<T, bool>>(binaryExpression, paramExpression);
                return res;
            }
            catch (Exception)
            {
                return Expression.Lambda<Func<T, bool>>(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), paramExpression);
            }
        }
        private static Expression GetExpression<T>(string colName, string action, string constraint, ParameterExpression paramExpression)
        {
            var info = typeof(T).GetProperties().FirstOrDefault(it => it.Name.ToLower() == colName);
            if (info == null)
            {
                return Expression.Lambda<Func<T, bool>>(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), paramExpression);
            }

            var constantExpression = info.PropertyType.Name switch
            {
                "Int32" => Expression.Constant(Int32.Parse(constraint)),
                _ => Expression.Constant(constraint, typeof(string))
            };


            var param = Expression.Property(paramExpression, info.Name);

            var binaryExpression = action switch
            {
                "=" => Expression.Equal(param, constantExpression),
                "==" => Expression.Equal(param, constantExpression),
                "<" => Expression.LessThan(param, constantExpression),
                "<=" => Expression.LessThanOrEqual(param, constantExpression),
                ">" => Expression.GreaterThan(param, constantExpression),
                ">=" => Expression.GreaterThanOrEqual(param, constantExpression),
                _ => Expression.Equal(Expression.Constant(1), Expression.Constant(1)),
            };

            return binaryExpression;
        }


        private static int CheckOr(string str, int curIndex, char ch, StringBuilder sb, List<string> tokens)
        {
            if (sb.Length > 0)
            {
                tokens.Add(sb.ToString());
                sb.Clear();
            }
            if (str[curIndex + 1] == '=')
            {
                tokens.Add(ch.ToString() + "=");
                curIndex++;
            }
            else tokens.Add(ch.ToString());
            return curIndex;
        }
        private static List<string> GetTokens(string str)
        {
            int len = str.Length, i = 0;
            var tokens = new List<string>();
            StringBuilder sb = new StringBuilder();

            while (i < len)
            {
                char ch = str[i];
                if (!(ch == '_' || ch == '\'' || ch == '"'))
                {
                    switch (ch)
                    {
                        case '(':
                        case ')':
                            if (sb.Length > 0)
                            {
                                tokens.Add(sb.ToString());
                                sb.Clear();
                            }
                            else tokens.Add(ch.ToString());
                            break;
                        case '=':
                        case '<':
                        case '>':
                            i = CheckOr(str, i, ch, sb, tokens);
                            break;

                        case ' ':
                            if (sb.Length > 0)
                            {
                                tokens.Add(sb.ToString());
                                sb.Clear();
                            }
                            while (i + 1 < len && str[i + 1] == ' ')
                            {
                                ch = str[i];
                                i++;
                            }
                            if (ch != ' ')
                            {
                                sb.Append(ch);
                            }
                            break;
                        default:
                            sb.Append(ch);
                            break;
                    }


                }
                i++;
            }
            if (sb.Length > 0)
            {
                tokens.Add(sb.ToString());
            }
            return tokens;
        }
        private static List<string> ConvertToPostfix(List<string> tokens)
        {

            var res = new List<string>();
            var stack = new Stack<string>();
            foreach (var item in tokens)
            {
                switch (item)
                {
                    case "(":
                        stack.Push(item);
                        break;
                    case "and":
                    case "or":
                        if (stack.Count > 0)
                            res.Add(stack.Pop());
                        stack.Push(item);
                        break;
                    case "=":
                    case "==":
                    case "<":
                    case ">":
                    case "<=":
                    case ">=":
                        stack.Push(item);
                        break;
                    case ")":
                        while (stack.Count > 0)
                        {
                            var operand = stack.Pop();
                            if (operand == "(") break;
                            res.Add(operand);
                        }
                        break;
                    default:
                        res.Add(item);
                        break;

                }
            }
            while (stack.Count > 0)
            {
                res.Add(stack.Pop());
            }
            return res;
        }
    }
}
