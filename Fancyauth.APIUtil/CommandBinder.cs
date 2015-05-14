using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fancyauth.API;
using Fancyauth.API.Commands;

namespace Fancyauth.APIUtil
{
    internal static class CommandBinder
    {
        private static readonly Expression<Func<IEnumerable<string>, IEnumerable<string>>> Enumerable_Skip1 = x => x.Skip(1);
        private static readonly Expression<Func<IEnumerable<string>, string>> Enumerable_FirstOrDefault = x => x.FirstOrDefault();
        private static readonly Expression<Func<IEnumerable<string>, bool>> Enumerable_NotAny = x => !x.Any();

        internal static CommandFunc CompileBinding(MethodInfo method, string name, IEnumerable<ParameterInfo> mparams, object self)
        {
            // magic happens here
            var userParam = Expression.Parameter(typeof(IUser));
            var paramsParam = Expression.Parameter(typeof(IEnumerable<string>));

            var allParams = new List<Expression>();
            if (mparams.Select(x => x.ParameterType).FirstOrDefault() == typeof(IUser)) {
                allParams.Add(userParam);
                mparams = mparams.Skip(1);
            }

            var usageBuilder = new StringBuilder("Usage: @fancy-ng ");
            usageBuilder.Append(name);
            foreach (var param in mparams) {
                usageBuilder.Append(' ');
                if (param.HasDefaultValue)
                    usageBuilder.Append('[');
                usageBuilder.Append(param.ParameterType == typeof(IEnumerable<string>) ? "..." : param.Name);
                if (param.HasDefaultValue)
                    usageBuilder.Append(']');
            }
            var usage = usageBuilder.ToString();
            Expression<Func<IUserShim, Task>> usageExpr = u => u.SendMessage(usage);

            var paramVars = mparams.Select(x => Tuple.Create(x, x.ParameterType == typeof(IEnumerable<string>) ? null : Expression.Variable(x.ParameterType))).ToArray();
            allParams.AddRange(paramVars.Select(x => x.Item2).Where(x => x != null));

            var retval = Expression.Variable(typeof(Task));
            var paramsVar = Expression.Variable(typeof(IEnumerable<string>));
            var finalFunc = Expression.Block(new ParameterExpression[] { paramsVar, retval }.Concat(paramVars.Select(x => x.Item2).Where(x => x != null)),
                Expression.Assign(retval, Expression.Constant(null, typeof(Task))),
                Expression.Assign(paramsVar, paramsParam),

                RecursivelyBuildMatcher(paramsVar, retval, paramVars, method, self, allParams),

                Expression.IfThen(Expression.Equal(retval, Expression.Constant(null)),
                    Expression.Assign(retval, Expression.Invoke(usageExpr, userParam))
                ),

                retval
            );


            return Expression.Lambda<CommandFunc>(finalFunc, userParam, paramsParam).Compile();
        }

        private static Expression RecursivelyBuildMatcher(ParameterExpression paramsVar, ParameterExpression retval,
            IEnumerable<Tuple<ParameterInfo, ParameterExpression>> parameters, MethodInfo meth, object self, IEnumerable<Expression> allParams)
        {
            var cur = parameters.FirstOrDefault();
            if (cur == null) {
                // suddenly all params are gone, so there's no rest
                // let's validate that we ate all params
                return Expression.IfThen(Expression.Invoke(Enumerable_NotAny, paramsVar),
                    Expression.Assign(retval, Expression.Call(Expression.Constant(self), meth, allParams))
                );
            } else if (cur.Item2 != null) {
                // paramvar, so this still isn't the rest
                var tmpString = Expression.Variable(typeof(string));
                var getString = Expression.Assign(tmpString, Expression.Invoke(Enumerable_FirstOrDefault, paramsVar));
                Expression conversionSuccess;
                switch (Type.GetTypeCode(cur.Item1.ParameterType)) {
                case TypeCode.Int32:
                    conversionSuccess = Expression.Call(typeof(int).GetMethod("TryParse", new Type[] {
                        typeof(string),
                        typeof(int).MakeByRefType()
                    }), tmpString, cur.Item2);
                    break;
                case TypeCode.Int64:
                    conversionSuccess = Expression.Call(typeof(long).GetMethod("TryParse", new Type[] {
                        typeof(string),
                        typeof(long).MakeByRefType()
                    }), tmpString, cur.Item2);
                    break;
                case TypeCode.Boolean:
                    conversionSuccess = Expression.Call(typeof(bool).GetMethod("TryParse", new Type[] {
                        typeof(string),
                        typeof(bool).MakeByRefType()
                    }), tmpString, cur.Item2);
                    break;
                case TypeCode.Double:
                    conversionSuccess = Expression.Call(typeof(double).GetMethod("TryParse", new Type[] {
                        typeof(string),
                        typeof(double).MakeByRefType()
                    }), tmpString, cur.Item2);
                    break;
                case TypeCode.String:
                    conversionSuccess = Expression.Block(
                        Expression.Assign(cur.Item2, tmpString),
                        Expression.Constant(true)
                    );
                    break;
                default:
                    throw new InvalidOperationException("Unsupported param type");
                }

                if (cur.Item1.HasDefaultValue) {
                    return Expression.Block(new ParameterExpression[] { tmpString },
                        Expression.Assign(cur.Item2, Expression.Constant(cur.Item1.DefaultValue)),
                        getString,
                        Expression.Assign(paramsVar, Expression.Invoke(Enumerable_Skip1, paramsVar)),
                        Expression.IfThen(Expression.Condition(Expression.Equal(tmpString, Expression.Constant(null)), Expression.Constant(true), conversionSuccess),
                            RecursivelyBuildMatcher(paramsVar, retval, parameters.Skip(1), meth, self, allParams)
                        )
                    );
                } else {
                    return Expression.Block(new ParameterExpression[] { tmpString },
                        getString,
                        Expression.Assign(paramsVar, Expression.Invoke(Enumerable_Skip1, paramsVar)),
                        Expression.IfThen(Expression.NotEqual(tmpString, Expression.Constant(null)),
                            Expression.IfThen(conversionSuccess,
                                RecursivelyBuildMatcher(paramsVar, retval, parameters.Skip(1), meth, self, allParams)
                            )
                        )
                    );
                }
            } else {
                // this gotta be the rest
                if (parameters.Skip(1).Any())
                    throw new InvalidOperationException("rest must be the last parameter");

                return Expression.Assign(retval, Expression.Call(Expression.Constant(self), meth, allParams.Concat(Enumerable.Repeat(paramsVar, 1))));
            }
        }
    }
}

