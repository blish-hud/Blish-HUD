namespace Glide {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class Tweener : Tween.TweenerImpl { };

    public partial class Tween {
        private interface IRemoveTweens //	lol get it
        {
            void Remove(Tween t);
        }

        public class TweenerImpl : IRemoveTweens {
            static TweenerImpl() {
                registeredLerpers = new Dictionary<Type, ConstructorInfo>();
                var numericTypes = new Type[] {
                    typeof(Int16),
                    typeof(Int32),
                    typeof(Int64),
                    typeof(UInt16),
                    typeof(UInt32),
                    typeof(UInt64),
                    typeof(Single),
                    typeof(Double),
                    typeof(byte)
                };

                for (int i = 0; i < numericTypes.Length; i++) {
                    SetLerper<NumericLerper>(numericTypes[i]);
                }
            }

            /// <summary>
            /// Associate a Lerper class with a property type.
            /// </summary>
            /// <typeparam name="TLerper">The Lerper class to use for properties of the given type.</typeparam>
            /// <param name="propertyType">The type of the property to associate the given Lerper with.</param>
            public static void SetLerper<TLerper>(Type propertyType) where TLerper : MemberLerper, new() {
                SetLerper(typeof(TLerper), propertyType);
            }

            /// <summary>
            /// Associate a Lerper type with a property type.
            /// </summary>
            /// <param name="lerperType">The type of the Lerper to use for properties of the given type.</param>
            /// <param name="propertyType">The type of the property to associate the given Lerper with.</param>
            public static void SetLerper(Type lerperType, Type propertyType) {
                registeredLerpers[propertyType] = lerperType.GetConstructor(Type.EmptyTypes);
            }

            protected TweenerImpl() {
                tweens = new ConcurrentDictionary<object, List<Tween>>();
                toRemove = new ConcurrentQueue<Tween>();
                toAdd = new ConcurrentQueue<Tween>();
            }


            private static Dictionary<Type, ConstructorInfo> registeredLerpers;

            private ConcurrentDictionary<object, List<Tween>> tweens;
            private ConcurrentQueue<Tween> toRemove;
            private ConcurrentQueue<Tween> toAdd;

            /// <summary>
            /// <para>Tweens a set of properties on an object.</para>
            /// <para>To tween instance properties/fields, pass the object.</para>
            /// <para>To tween static properties/fields, pass the type of the object, using typeof(ObjectType) or object.GetType().</para>
            /// </summary>
            /// <param name="target">The object or type to tween.</param>
            /// <param name="values">The values to tween to, in an anonymous type ( new { prop1 = 100, prop2 = 0} ).</param>
            /// <param name="duration">Duration of the tween in seconds.</param>
            /// <param name="delay">Delay before the tween starts, in seconds.</param>
            /// <param name="overwrite">Whether pre-existing tweens should be overwritten if this tween involves the same properties.</param>
            /// <returns>The tween created, for setting properties on.</returns>
            public Tween Tween<T>(T target, object values, float duration, float delay = 0, bool overwrite = true) where T : class {
                if (target == null) {
                    throw new ArgumentNullException("target");
                }

                //	Prevent tweening on structs if you cheat by casting target as Object
                var targetType = typeof(T);
                if (targetType.IsValueType) {
                    throw new Exception("Target of tween cannot be a struct!");
                }

                var tween = new Tween(target, duration, delay, this);
                toAdd.Enqueue(tween);

                if (values == null) // valid in case of manual timer
                    return tween;

                var props = values.GetType().GetProperties();
                for (int i = 0; i < props.Length; ++i) {
                    if (overwrite) {
                        ForAllTweens(target, tw => tw.Cancel(props[i].Name));
                    }

                    var property = props[i];
                    var info = new MemberAccessor(target, property.Name);
                    var to = new MemberAccessor(values, property.Name, false);
                    var lerper = CreateLerper(info.MemberType);

                    tween.AddLerp(lerper, info, info.GetValue(target), to.GetValue(values));
                }

                AddAndRemove();
                return tween;
            }

            /// <summary>
            /// Starts a simple timer for setting up callback scheduling.
            /// </summary>
            /// <param name="duration">How long the timer will run for, in seconds.</param>
            /// <param name="delay">How long to wait before starting the timer, in seconds.</param>
            /// <returns>The tween created, for setting properties.</returns>
            public Tween Timer(float duration, float delay = 0) {
                var tween = new Tween(this, duration, delay, this);
                toAdd.Enqueue(tween);
                AddAndRemove();
                return tween;
            }

            /// <summary>
            /// Remove tweens from the tweener without calling their complete functions.
            /// </summary>
            public void Cancel() {
                ForAllTweens(toRemove.Enqueue);
            }

            /// <summary>
            /// Assign tweens their final value and remove them from the tweener.
            /// </summary>
            public void CancelAndComplete() {
                ForAllTweens(tw => tw.CancelAndComplete());
            }

            /// <summary>
            /// Set tweens to pause. They won't update and their delays won't tick down.
            /// </summary>
            public void Pause() {
                ForAllTweens(tw => tw.Pause());
            }

            /// <summary>
            /// Toggle tweens' paused value.
            /// </summary>
            public void PauseToggle() {
                ForAllTweens(tw => tw.PauseToggle());
            }

            /// <summary>
            /// Resumes tweens from a paused state.
            /// </summary>
            public void Resume() {
                ForAllTweens(tw => tw.Resume());
            }

            /// <summary>
            /// Updates the tweener and all objects it contains.
            /// </summary>
            /// <param name="secondsElapsed">Seconds elapsed since last update.</param>
            public void Update(float secondsElapsed) {
                ForAllTweens(tw => tw.Update(secondsElapsed));

                AddAndRemove();
            }

            private void ForAllTweens(Action<Tween> action) {
                foreach (object target in this.tweens.Keys) {
                    ForAllTweens(target, action);
                }
            }

            private void ForAllTweens(object target, Action<Tween> action) {
                if (tweens.TryGetValue(target, out var list)) {
                    lock (list) {
                        foreach (Tween tween in list) {
                            action(tween);
                        }
                    }
                }
            }

            private MemberLerper CreateLerper(Type propertyType) {
                if (!registeredLerpers.TryGetValue(propertyType, out ConstructorInfo lerper)) {
                    throw new Exception(string.Format("No Lerper found for type {0}.", propertyType.FullName));
                }

                return (MemberLerper)lerper.Invoke(null);
            }

            void IRemoveTweens.Remove(Tween tween) {
                toRemove.Enqueue(tween);
            }

            private void AddAndRemove() {
                while (toAdd.TryDequeue(out Tween tween)) {
                    List<Tween> list = tweens.GetOrAdd(tween.Target, _ => new List<Tween>(4));
                    lock (list) {
                        list.Add(tween);
                    }
                }

                while (toRemove.TryDequeue(out Tween tween)) {
                    if (tweens.TryGetValue(tween.Target, out List<Tween> list)) {
                        lock (list) {
                            list.Remove(tween);
                        }

                        if (!list.Any()) {
                            tweens.TryRemove(tween.Target, out _);
                        }
                    }
                }
            }

            #region Target control
            /// <summary>
            /// Cancel all tweens with the given target.
            /// </summary>
            /// <param name="target">The object being tweened that you want to cancel.</param>
            public void TargetCancel(object target) {
                ForAllTweens(target, tw => tw.Cancel());
            }

            /// <summary>
            /// Cancel tweening named properties on the given target.
            /// </summary>
            /// <param name="target">The object being tweened that you want to cancel properties on.</param>
            /// <param name="properties">The properties to cancel.</param>
            public void TargetCancel(object target, params string[] properties) {
                ForAllTweens(target, tw => tw.Cancel(properties));
            }

            /// <summary>
            /// Cancel, complete, and call complete callbacks for all tweens with the given target..
            /// </summary>
            /// <param name="target">The object being tweened that you want to cancel and complete.</param>
            public void TargetCancelAndComplete(object target) {
                ForAllTweens(target, tw => tw.CancelAndComplete());
            }


            /// <summary>
            /// Pause all tweens with the given target.
            /// </summary>
            /// <param name="target">The object being tweened that you want to pause.</param>
            public void TargetPause(object target) {
                ForAllTweens(target, tw => tw.Pause());
            }

            /// <summary>
            /// Toggle the pause state of all tweens with the given target.
            /// </summary>
            /// <param name="target">The object being tweened that you want to toggle pause.</param>
            public void TargetPauseToggle(object target) {
                ForAllTweens(target, tw => tw.PauseToggle());
            }

            /// <summary>
            /// Resume all tweens with the given target.
            /// </summary>
            /// <param name="target">The object being tweened that you want to resume.</param>
            public void TargetResume(object target) {
                ForAllTweens(target, tw => tw.Resume());
            }
            #endregion

            private class NumericLerper : MemberLerper {
                float from, to, range;

                public override void Initialize(object fromValue, object toValue, Behavior behavior) {
                    from = Convert.ToSingle(fromValue);
                    to = Convert.ToSingle(toValue);
                    range = to - from;

                    if ((behavior & Behavior.Rotation) == Behavior.Rotation) {
                        float angle = from;
                        if ((behavior & Behavior.RotationRadians) == Behavior.RotationRadians)
                            angle *= DEG;

                        if (angle < 0)
                            angle = 360 + angle;

                        float r = angle + range;
                        float d = r - angle;
                        float a = (float)Math.Abs(d);

                        if (a >= 180) range = (360 - a) * (d > 0 ? -1 : 1);
                        else range = d;
                    }
                }

                public override object Interpolate(float t, object current, Behavior behavior) {
                    var value = from + range * t;
                    if ((behavior & Behavior.Rotation) == Behavior.Rotation) {
                        if ((behavior & Behavior.RotationRadians) == Behavior.RotationRadians)
                            value *= DEG;

                        value %= 360.0f;

                        if (value < 0)
                            value += 360.0f;

                        if ((behavior & Behavior.RotationRadians) == Behavior.RotationRadians)
                            value *= RAD;
                    }

                    if ((behavior & Behavior.Round) == Behavior.Round)
                        value = (float)Math.Round(value);

                    var type = current.GetType();
                    return Convert.ChangeType(value, type);
                }
            }
        }
    }
}