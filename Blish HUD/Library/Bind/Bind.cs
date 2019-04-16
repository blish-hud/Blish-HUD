////
////  Copyright 2013-2014 Frank A. Krueger
////
////    Licensed under the Apache License, Version 2.0 (the "License");
////    you may not use this file except in compliance with the License.
////    You may obtain a copy of the License at
////
////        http://www.apache.org/licenses/LICENSE-2.0
////
////    Unless required by applicable law or agreed to in writing, software
////    distributed under the License is distributed on an "AS IS" BASIS,
////    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
////    See the License for the specific language governing permissions and
////    limitations under the License.
////
//using System;
//using System.Linq.Expressions;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Diagnostics;
//using System.ComponentModel;

//namespace Praeclarum.Bind {
//    /// <summary>
//    /// Abstract class that represents bindings between values in an applications.
//    /// Binding are created using Create and removed by calling Unbind.
//    /// </summary>
//    public abstract class Binding {
//        /// <summary>
//        /// Unbind this instance. This cannot be undone.
//        /// </summary>
//        public virtual void Unbind() {
//        }

//        /// <summary>
//        /// Uses the lambda expression to create data bindings.
//        /// Equality expression (==) become data bindings.
//        /// And expressions (&&) can be used to group the data bindings.
//        /// </summary>
//        /// <param name="specifications">The binding specifications.</param>
//        public static Binding Create<T>(Expression<Func<T>> specifications) {
//            return BindExpression(specifications.Body);
//        }

//        static Binding BindExpression(Expression expr) {
//            //
//            // Is this a group of bindings
//            //
//            if (expr.NodeType == ExpressionType.AndAlso) {

//                var b = (BinaryExpression)expr;

//                var parts = new List<Expression>();

//                while (b != null) {
//                    var l = b.Left;
//                    parts.Add(b.Right);
//                    if (l.NodeType == ExpressionType.AndAlso) {
//                        b = (BinaryExpression)l;
//                    } else {
//                        parts.Add(l);
//                        b = null;
//                    }
//                }

//                parts.Reverse();

//                return new MultipleBindings(parts.Select(BindExpression));
//            }

//            //
//            // Are we binding two values?
//            //
//            if (expr.NodeType == ExpressionType.Equal) {
//                var b = (BinaryExpression)expr;
//                return new EqualityBinding(b.Left, b.Right);
//            }

//            //
//            // This must be a new object binding (a template)
//            //
//            throw new NotSupportedException("Only equality bindings are supported.");
//        }

//        protected static bool SetValue(Expression expr, object value, int changeId) {
//            if (expr.NodeType == ExpressionType.MemberAccess) {
//                var m = (MemberExpression)expr;
//                var mem = m.Member;

//                var target = Evaluator.EvalExpression(m.Expression);

//                var f = mem as FieldInfo;
//                var p = mem as PropertyInfo;

//                if (f != null) {
//                    f.SetValue(target, value);
//                } else if (p != null) {
//                    p.SetValue(target, value, null);
//                } else {
//                    ReportError("Trying to SetValue on " + mem.GetType() + " member");
//                    return false;
//                }

//                InvalidateMember(target, mem, changeId);
//                return true;
//            }

//            ReportError("Trying to SetValue on " + expr.NodeType + " expression");
//            return false;
//        }

//        public static event Action<string> Error = delegate { };

//        static void ReportError(string message) {
//            //Debug.WriteLine(message);
//            Error(message);
//        }

//        static void ReportError(object errorObject) {
//            ReportError(errorObject.ToString());
//        }

//        #region Change Notification

//        class MemberActions {
//            readonly object target;
//            readonly MemberInfo member;

//            EventInfo eventInfo;
//            Delegate eventHandler;

//            public MemberActions(object target, MemberInfo mem) {
//                this.target = target;
//                member = mem;
//            }

//            void AddChangeNotificationEventHandler() {
//                if (target != null) {
//                    if (target is INotifyPropertyChanged npc && (member is PropertyInfo)) {
//                        npc.PropertyChanged += HandleNotifyPropertyChanged;
//                    } else {
//                        /* Not utilized by Blish HUD (all applicable elements implement INotifyPropertyChanged */

//                        //var added = AddHandlerForFirstExistingEvent(member.Name + "Changed", "EditingDidEnd", "ValueChanged", "Changed");
//                        //if (!added) {
//                        //    Debug.WriteLine("Failed to bind to change event for " + target);
//                        //}
//                    }
//                }
//            }

//            bool AddHandlerForFirstExistingEvent(params string[] names) {
//                var type = target.GetType();
//                foreach (var name in names) {
//                    var ev = GetEvent(type, name);

//                    if (ev != null) {
//                        eventInfo = ev;
//                        var isClassicHandler = typeof(EventHandler).GetTypeInfo().IsAssignableFrom(ev.EventHandlerType.GetTypeInfo());

//                        eventHandler = isClassicHandler ?
//                            (EventHandler)HandleAnyEvent :
//                            CreateGenericEventHandler(ev, () => HandleAnyEvent(null, EventArgs.Empty));

//                        ev.AddEventHandler(target, eventHandler);
//                        Debug.WriteLine("BIND: Added handler for {0} on {1}", eventInfo.Name, target);
//                        return true;
//                    }
//                }
//                return false;
//            }

//            static EventInfo GetEvent(Type type, string eventName) {
//                var t = type;
//                while (t != null && t != typeof(object)) {
//                    var ti = t.GetTypeInfo();
//                    var ev = t.GetTypeInfo().GetDeclaredEvent(eventName);
//                    if (ev != null)
//                        return ev;
//                    t = ti.BaseType;
//                }
//                return null;
//            }

//            static Delegate CreateGenericEventHandler(EventInfo evt, Action d) {
//                var handlerType = evt.EventHandlerType;
//                var handlerTypeInfo = handlerType.GetTypeInfo();
//                var handlerInvokeInfo = handlerTypeInfo.GetDeclaredMethod("Invoke");
//                var eventParams = handlerInvokeInfo.GetParameters();

//                //lambda: (object x0, EventArgs x1) => d()
//                var parameters = eventParams.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
//                var body = Expression.Call(Expression.Constant(d), d.GetType().GetTypeInfo().GetDeclaredMethod("Invoke"));
//                var lambda = Expression.Lambda(body, parameters);

//                return lambda.Compile(); // From PR https://github.com/praeclarum/Bind/pull/3/commits/da275e8d09a59a4b909b49f0f53c64b3a009286a

//                //var delegateInvokeInfo = lambda.Compile().GetMethodInfo();
//                //return delegateInvokeInfo.CreateDelegate(handlerType, null);
//            }

//            void UnsubscribeFromChangeNotificationEvent() {
//                var npc = target as INotifyPropertyChanged;
//                if (npc != null && (member is PropertyInfo)) {
//                    npc.PropertyChanged -= HandleNotifyPropertyChanged;
//                    return;
//                }

//                if (eventInfo == null)
//                    return;

//                eventInfo.RemoveEventHandler(target, eventHandler);

//                Debug.WriteLine("BIND: Removed handler for {0} on {1}", eventInfo.Name, target);

//                eventInfo = null;
//                eventHandler = null;
//            }

//            void HandleNotifyPropertyChanged(object sender, PropertyChangedEventArgs e) {
//                if (e.PropertyName == member.Name)
//                    Binding.InvalidateMember(target, member);
//            }

//            void HandleAnyEvent(object sender, EventArgs e) {
//                Binding.InvalidateMember(target, member);
//            }

//            readonly List<MemberChangeAction> actions = new List<MemberChangeAction>();

//            /// <summary>
//            /// Add the specified action to be executed when Notify() is called.
//            /// </summary>
//            /// <param name="action">Action.</param>
//            public void AddAction(MemberChangeAction action) {
//                if (actions.Count == 0) {
//                    AddChangeNotificationEventHandler();
//                }

//                actions.Add(action);
//            }

//            public void RemoveAction(MemberChangeAction action) {
//                actions.Remove(action);

//                if (actions.Count == 0) {
//                    UnsubscribeFromChangeNotificationEvent();
//                }
//            }

//            /// <summary>
//            /// Execute all the actions.
//            /// </summary>
//            /// <param name="changeId">Change identifier.</param>
//            public void Notify(int changeId) {
//                foreach (var s in actions) {
//                    s.Notify(changeId);
//                }
//            }
//        }

//        static readonly Dictionary<Tuple<Object, MemberInfo>, MemberActions> objectSubs = new Dictionary<Tuple<Object, MemberInfo>, MemberActions>();

//        internal static MemberChangeAction AddMemberChangeAction(object target, MemberInfo member, Action<int> k) {
//            var key = Tuple.Create(target, member);
//            MemberActions subs;
//            if (!objectSubs.TryGetValue(key, out subs)) {
//                subs = new MemberActions(target, member);
//                objectSubs.Add(key, subs);
//            }

//            //			Debug.WriteLine ("ADD CHANGE ACTION " + target + " " + member);
//            var sub = new MemberChangeAction(target, member, k);
//            subs.AddAction(sub);
//            return sub;
//        }

//        internal static void RemoveMemberChangeAction(MemberChangeAction sub) {
//            var key = Tuple.Create(sub.Target, sub.Member);
//            MemberActions subs;
//            if (objectSubs.TryGetValue(key, out subs)) {
//                //				Debug.WriteLine ("REMOVE CHANGE ACTION " + sub.Target + " " + sub.Member);
//                subs.RemoveAction(sub);
//                objectSubs.Remove(key); // From PR https://github.com/praeclarum/Bind/pull/14/commits/68f373e2abd6bf9d8a42c7b98d39c40a7330b40c
//            }
//        }

//        /// <summary>
//        /// Invalidate the specified object member. This will cause all actions
//        /// associated with that member to be executed.
//        /// This is the main mechanism by which binding values are distributed.
//        /// </summary>
//        /// <param name="target">Target object</param>
//        /// <param name="member">Member of the object that changed</param>
//        /// <param name="changeId">Change identifier</param>
//        public static void InvalidateMember(object target, MemberInfo member, int changeId = 0) {
//            var key = Tuple.Create(target, member);
//            MemberActions subs;
//            if (objectSubs.TryGetValue(key, out subs)) {
//                //				Debug.WriteLine ("INVALIDATE {0} {1}", target, member.Name);
//                subs.Notify(changeId);
//            }
//        }

//        /// <summary>
//        /// A nice expression based way to invalidate the specified object member. 
//        /// This will cause all actions associated with that member to be executed.
//        /// This is the main mechanism by which binding values are distributed.
//        /// </summary>
//        /// <param name="lambdaExpr">Lambda expr of the Member of the object that changed</param>
//        /// <typeparam name="T">The 1st type parameter.</typeparam>
//        public static void InvalidateMember<T>(Expression<Func<T>> lambdaExpr) {
//            var body = lambdaExpr.Body;
//            if (body.NodeType == ExpressionType.MemberAccess) {
//                var m = (MemberExpression)body;
//                var obj = Evaluator.EvalExpression(m.Expression);
//                InvalidateMember(obj, m.Member, 0);
//            }
//        }

//        #endregion
//    }


//    /// <summary>
//    /// An action tied to a particular member of an object.
//    /// When Notify is called, the action is executed.
//    /// </summary>
//    class MemberChangeAction {
//        readonly Action<int> action;

//        public object Target { get; private set; }
//        public MemberInfo Member { get; private set; }

//        public MemberChangeAction(object target, MemberInfo member, Action<int> action) {
//            Target = target;
//            if (member == null)
//                throw new ArgumentNullException("member");
//            Member = member;
//            if (action == null)
//                throw new ArgumentNullException("action");
//            this.action = action;
//        }

//        public void Notify(int changeId) {
//            action(changeId);
//        }
//    }


//    /// <summary>
//    /// Methods that can evaluate Linq expressions.
//    /// </summary>
//    static class Evaluator {
//        /// <summary>
//        /// Gets the value of a Linq expression.
//        /// </summary>
//        /// <param name="expr">The expresssion.</param>
//        public static object EvalExpression(Expression expr) {
//            //
//            // Easy case
//            //
//            if (expr.NodeType == ExpressionType.Constant) {
//                return ((ConstantExpression)expr).Value;
//            }

//            //
//            // General case
//            //
//            			//Debug.WriteLine ("WARNING EVAL COMPILED {0}", expr);
//            var lambda = Expression.Lambda(expr, Enumerable.Empty<ParameterExpression>());
//            return lambda.Compile().DynamicInvoke();
//        }
//    }

//    /// <summary>
//    /// Binding between two values. When one changes, the other
//    /// is set.
//    /// </summary>
//    class EqualityBinding:Binding {
//        object Value;

//        class Trigger {
//            public Expression Expression;
//            public MemberInfo Member;
//            public MemberChangeAction ChangeAction;
//        }

//        readonly List<Trigger> leftTriggers = new List<Trigger>();
//        readonly List<Trigger> rightTriggers = new List<Trigger>();

//        public EqualityBinding(Expression left, Expression right) {
//            // Try evaling the right and assigning left
//            Value = Evaluator.EvalExpression(right);
//            var leftSet = SetValue(left, Value, nextChangeId);

//            // If that didn't work, then try the other direction
//            if (!leftSet) {
//                Value = Evaluator.EvalExpression(left);
//                SetValue(right, Value, nextChangeId);
//            }

//            nextChangeId++;

//            CollectTriggers(left, leftTriggers);
//            CollectTriggers(right, rightTriggers);

//            Resubscribe(leftTriggers, left, right);
//            Resubscribe(rightTriggers, right, left);
//        }

//        public override void Unbind() {
//            Unsubscribe(leftTriggers);
//            Unsubscribe(rightTriggers);
//            base.Unbind();
//        }

//        void Resubscribe(List<Trigger> triggers, Expression expr, Expression dependentExpr) {
//            Unsubscribe(triggers);
//            Subscribe(triggers, changeId => OnSideChanged(expr, dependentExpr, changeId));
//        }

//        int nextChangeId = 1;
//        readonly HashSet<int> activeChangeIds = new HashSet<int>();

//        void OnSideChanged(Expression expr, Expression dependentExpr, int causeChangeId) {
//            if (activeChangeIds.Contains(causeChangeId))
//                return;

//            var v = Evaluator.EvalExpression(expr);

//            if (v == null && Value == null)
//                return;

//            if ((v == null && Value != null) ||
//                (v != null && Value == null) ||
//                (v != Value)) { // https://github.com/praeclarum/Bind/issues/13
//                //((v is IComparable) && ((IComparable)v).CompareTo(Value) != 0)) {

//                Value = v;

//                var changeId = nextChangeId++;
//                activeChangeIds.Add(changeId);
//                SetValue(dependentExpr, v, changeId);
//                activeChangeIds.Remove(changeId);
//            }
//            //			else {
//            //				Debug.WriteLine ("Prevented needless update");
//            //			}
//        }

//        static void Unsubscribe(List<Trigger> triggers) {
//            foreach (var t in triggers) {
//                if (t.ChangeAction != null) {
//                    RemoveMemberChangeAction(t.ChangeAction);
//                }
//            }
//        }

//        static void Subscribe(List<Trigger> triggers, Action<int> action) {
//            foreach (var t in triggers) {
//                t.ChangeAction = AddMemberChangeAction(Evaluator.EvalExpression(t.Expression), t.Member, action);
//            }
//        }

//        void CollectTriggers(Expression s, List<Trigger> triggers) {
//            if (s.NodeType == ExpressionType.MemberAccess) {

//                var m = (MemberExpression)s;
//                CollectTriggers(m.Expression, triggers);
//                var t = new Trigger { Expression = m.Expression, Member = m.Member };
//                triggers.Add(t);

//            } else {
//                var b = s as BinaryExpression;
//                if (b != null) {
//                    CollectTriggers(b.Left, triggers);
//                    CollectTriggers(b.Right, triggers);
//                }
//            }
//        }
//    }


//    /// <summary>
//    /// Multiple bindings grouped under a single binding to make adding and removing easier.
//    /// </summary>
//    class MultipleBindings:Binding {
//        readonly List<Binding> bindings;

//        public MultipleBindings(IEnumerable<Binding> bindings) {
//            this.bindings = bindings.Where(x => x != null).ToList();
//        }

//        public override void Unbind() {
//            base.Unbind();
//            foreach (var b in bindings) {
//                b.Unbind();
//            }
//            bindings.Clear();
//        }
//    }
//}
