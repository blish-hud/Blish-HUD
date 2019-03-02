using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Blish_HUD {

    public class EaseAnimation : IDisposable {

        public event EventHandler<EventArgs> AnimationCompleted;

        private AnimationService.EasingFunctionDelegate AnimationFunction;

        private double StartValue;
        private double ChangeInValue;
        private double Duration;

        private double StartTime;

        public bool Active { get; private set; } = false;

        public double CurrentValue { get; private set; }
        public int CurrentValueInt { get { return (int)Math.Round(this.CurrentValue); } }

        public bool Done { get; private set; } = false;

        private bool Repeat = false;

        public void Start(bool repeat = false) {
            Repeat = repeat;
            StartTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            this.CurrentValue = StartValue;
            this.Done = false;
            this.Active = true;
        }

        public void Stop() {
            this.Active = false;
        }

        public void Reverse() {
            double tempStartVal = StartValue;
            Duration = Duration * this.CurrentValue / ChangeInValue;
            StartValue = this.CurrentValue;
            ChangeInValue = -this.CurrentValue;

            Start();
        }

        public EaseAnimation(AnimationService.EasingFunctionDelegate animFunc, double startValue, double changeInValue, double duration) {
            StartTime = DateTime.Now.TimeOfDay.TotalMilliseconds;

            this.CurrentValue = startValue;
            AnimationFunction = animFunc;
            StartValue = startValue;
            ChangeInValue = changeInValue;
            Duration = duration;
        }

        public void Update(GameTime gameTime) {
            // Ensure everything is able to get the last value from the tween before it stops updating
            if (this.Done) this.Active = false;

            double currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds - StartTime;

            if (currentTime > Duration) {
                currentTime = Duration;
                this.Done = true;

                this.AnimationCompleted?.Invoke(this, null);
            }

            this.CurrentValue = AnimationFunction.Invoke(currentTime, StartValue, ChangeInValue, Duration);

            if (this.Done && Repeat) Start(Repeat);
        }

        public void Dispose() {
            GameServices.GetService<AnimationService>().RemoveAnim(this);
        }
    }

    public class AnimationService:GameService {

        public enum EasingMethod {
            Linear,
            EaseInQuad,
            EaseOutQuad,
            EaseInOutQuad,
            EaseInCubic,
            EaseOutCubic,
            EaseInOutCubic,
            EaseInQuart,
            EaseOutQuart,
            EaseInOutQuart,
            EaseInQuint,
            EaseOutQuint,
            EaseInOutQuint,
            EaseInSine,
            EaseOutSine,
            EaseInOutSine,
            EaseInExpo,
            EaseOutExpo,
            EaseInOutExpo,
            EaseInCirc,
            EaseOutCirc,
            EaseInOutCirc
        }

        public delegate double EasingFunctionDelegate(double currentTime, double startValue, double changeInValue, double duration);

        private Dictionary<EasingMethod, EasingFunctionDelegate> EaseFuncs;

        private List<EaseAnimation> CurrentAnimations;

        private Glide.Tweener _tweener;
        public Glide.Tweener Tweener { get { return _tweener; } }

        protected override void Initialize() {
            CurrentAnimations = new List<EaseAnimation>();
            EaseFuncs = new Dictionary<EasingMethod, EasingFunctionDelegate>();

            _tweener = new Glide.Tweener();

            EaseFuncs.Add(EasingMethod.Linear, CalcLinear);
            EaseFuncs.Add(EasingMethod.EaseInExpo, CalcExponentialEasingIn);
            EaseFuncs.Add(EasingMethod.EaseInOutQuad, CalcQuadraticEasingInOut);
        }

        public EaseAnimation Tween(double startValue, double changeInValue, double duration, EasingMethod method) {
            var nanim = new EaseAnimation(EaseFuncs[method], startValue, changeInValue, duration);
            CurrentAnimations.Add(nanim);
            return nanim;
        }

        public void RemoveAnim(EaseAnimation anim) {
            CurrentAnimations.Remove(anim);
        }

        protected override void Update(GameTime gameTime) {
            CurrentAnimations.Where(a => a.Active).ToList().ForEach(a => a.Update(gameTime));

            this.Tweener.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        // Easing functions:

        private double CalcLinear(double currentTime, double startValue, double changeInValue, double duration) {
            return changeInValue * currentTime / duration + startValue;
        }

        private double CalcExponentialEasingIn(double currentTime, double startValue, double changeInValue, double duration) {
            return changeInValue * Math.Pow(2, 10 * (currentTime / duration - 1)) + startValue;
        }

        private double CalcQuadraticEasingInOut(double currentTime, double startValue, double changeInValue, double duration) {
            currentTime /= duration / 2;
            if (currentTime < 1) return changeInValue / 2 * currentTime * currentTime + startValue;
            currentTime--;
            return -changeInValue / 2 * (currentTime * (currentTime - 2) - 1) + startValue;
        }

        protected override void Load() {
            // TODO: Set up animation service
        }

        protected override void Unload() {
            // TODO: Clean up animation service
        }
    }
}
