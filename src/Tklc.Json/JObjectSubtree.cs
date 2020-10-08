using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Tklc.Json {
    public static class JObjectSubtree {
        /// <summary>
        /// Create a subtree of a JObject given a navigation path.
        /// </summary>
        /// <typeparam name="T">Type of root element.</typeparam>
        /// <param name="source">Full JObject tree.</param>
        /// <param name="navigation">Navigation lambda.</param>
        /// <param name="resolver">The contract resolver used to create the full tree.</param>
        /// <returns>Created tree.</returns>
        public static JObject Create<T>(JObject source, Expression<Func<T, object>> navigation, DefaultContractResolver resolver = null) {
            return new JObject().AddSubtree(source, navigation, resolver);
        }

        /// <summary>
        /// Add a subtree of a JObject given a navigation path to an existing subtree.
        /// </summary>
        /// <typeparam name="T">Type of root element.</typeparam>
        /// <param name="destination">Existing subtree.</param>
        /// <param name="source">Full JObject tree.</param>
        /// <param name="navigation">Navigation lambda.</param>
        /// <param name="resolver">The contract resolver used to create the full tree.</param>
        /// <returns>Existing subtree, with new elements added.</returns>
        public static JObject AddSubtree<T>(this JObject destination, JObject source, Expression<Func<T, object>> navigation, DefaultContractResolver resolver = null) {
            var stack = BuildNavigation(navigation, resolver ?? new DefaultContractResolver());

            JToken src = source;
            JToken dst = destination;
            while (stack.Count > 0) {
                var node = stack.Pop();

                if (src.Type != dst.Type) {
                    throw new Exception($"at {node.Name}, source is {src.Type} but destination is {dst.Type}");
                }

                if (src is JObject srcObject && dst is JObject dstObject) {
                    var key = node.Name;
                    if (!srcObject.ContainsKey(key)) {
                        throw new Exception($"key {key} is not found in source");
                    }

                    // Continue navigation if key exists in dst
                    if (dstObject.ContainsKey(key)) {
                        src = srcObject[key];
                        dst = dstObject[key];
                        continue;
                    }

                    // Create object in dst
                    // Two cases: if node is leaf, copy whole object; otherwise, create empty object and continue navigation
                    var srcChild = srcObject[key];
                    if (stack.Count == 0) {
                        dst[key] = srcChild.DeepClone();
                    }
                    else {
                        dst[key] = (JToken)Activator.CreateInstance(srcChild.GetType());

                        src = srcChild;
                        dst = dst[key];
                    }
                }
                else if (src is JArray srcArray && dst is JArray dstArray) {
                    var key = Int32.Parse(node.Name);
                    if (key < 0 || key >= srcArray.Count) {
                        throw new Exception("array index out of range");
                    }

                    // Create object in dst
                    // Same two cases as above
                    var srcChild = srcArray[key];
                    if (stack.Count == 0) {
                        dstArray.Add(srcChild.DeepClone());
                    }
                    else {
                        dstArray.Add((JToken)Activator.CreateInstance(srcChild.GetType()));

                        src = srcChild;
                        dst = dstArray.Last;
                    }
                }
                else {
                    throw new Exception($"unexpected src token type {src.GetType().Name} and dst token type {dst.GetType().Name}");
                }
            }

            return destination;
        }

        private static Stack<NavigationNode> BuildNavigation(Expression expr, DefaultContractResolver resolver) {
            var stack = new Stack<NavigationNode>();
            var current = expr;
            var done = false;

            while (!done) {
                switch (current) {
                    case LambdaExpression lambda:
                        current = lambda.Body;
                        break;
                    case ParameterExpression param:
                        done = true;
                        break;
                    case UnaryExpression unary: {
                            if (unary.Method == null) {
                                current = unary.Operand;
                            }
                            else {
                                throw new Exception($"Unexpected unary method {unary.Method.Name}");
                            }
                        }
                        break;
                    case MethodCallExpression call: {
                            if (call.Method.Name != "get_Item") {
                                throw new Exception($"unexpected method call to {call.Method.Name}");
                            }

                            if (call.Arguments.Count != 1) {
                                throw new Exception($"unexpected argument count {call.Arguments.Count}");
                            }

                            var arg = call.Arguments[0];
                            var method = typeof(JObjectSubtree).GetMethod("BuildAndRunExpression",
                                    BindingFlags.NonPublic | BindingFlags.Static)
                                .MakeGenericMethod(arg.Type);

                            stack.Push(new NavigationNode((string)method.Invoke(null, new object[] { arg })));

                            current = call.Object;
                        }
                        break;
                    case MemberExpression member: {
                            stack.Push(new NavigationNode(GetJsonPropertyName(member.Member, resolver)));
                            current = member.Expression;
                        }
                        break;
                    default:
                        throw new Exception($"unexpected expression {current?.GetType().Name}");
                }
            }

            return stack;
        }

        private class NavigationNode {
            public string Name { get; }

            public NavigationNode(string name) {
                Name = name;
            }
        }

        private static string BuildAndRunExpression<T>(Expression expr) {
            if (expr.Type != typeof(T)) {
                throw new Exception($"type of expression does not match type of T");
            }

            var lambda = Expression.Lambda<Func<T>>(expr);
            return lambda.Compile()().ToString();
        }

        private static string GetJsonPropertyName(MemberInfo info, DefaultContractResolver resolver) {
            string memberName = null;
            foreach (var attribute in info.CustomAttributes) {
                if (attribute.AttributeType != typeof(JsonPropertyAttribute)) {
                    continue;
                }

                if (attribute.ConstructorArguments.Count == 1) {
                    memberName = attribute.ConstructorArguments[0].Value.ToString();
                    break;
                }

                foreach (var arg in attribute.NamedArguments) {
                    if (arg.MemberInfo.Name == "PropertyName") {
                        memberName = arg.TypedValue.Value.ToString();
                        break;
                    }
                }
            }

            if (memberName == null) {
                memberName = resolver.GetResolvedPropertyName(info.Name);
            }

            return memberName;
        }
    }
}
