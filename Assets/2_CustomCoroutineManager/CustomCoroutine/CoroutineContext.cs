namespace Extension.Coroutine
{
    using System;
    using System.Text;
    using UnityEngine;

    public enum ContextFlag
    {
        Default             = 0,

        Coroutine           ,
        Timer               ,
        Delegate            ,
        
        TimerAndDelegate    ,
        TimerOrDelegate     ,
    }

    public enum InterruptContext
    {
        Update              = 0,
        LateUpdate          ,
        FixedUpdate         ,
        EndOfFrame          ,
    }

    public enum TimerContext
    {
        Time                = 0,
        UnscaledTime        ,
        FixedTime           ,
        RealTime            ,
    }

    public enum DelegateContext
    {
        WaitWhile           = 0,
        WaitUntil           ,
    }

    public struct Context
    {
        public ContextFlag          flag;

        public InterruptContext     interruptContext;
        public TimerContext         timerContext;
        public float                delayTime;
        public float                startTime;
        public DelegateContext      delegateContext;
        public Func<bool>           func;

        public Tag                  tag;

        public CoroutineState       state;

        public static Context operator +(Context context, Tag tag)
        {
            context.tag = tag;
            return context;
        }

        public static implicit operator Context(CoroutineState state)
        {
            Context context = new Context{ flag = ContextFlag.Coroutine, state = state };
            return context;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(GetType().ToString()).Append('(').Append(flag).Append(", ");
            switch (flag)
            {
                case ContextFlag.Default:
                    builder.Append(interruptContext);
                    break;
                case ContextFlag.Timer:
                    builder.Append(interruptContext).Append(", ").Append(timerContext).Append(", ").Append(startTime);
                    break;
                case ContextFlag.Delegate:
                    builder.Append(interruptContext).Append(", ").Append(delegateContext).Append(", ").Append(func);
                    break;
            }
            builder.Append(")");

            if (!tag.empty)
                builder.Append(" + ").Append(tag.ToString());

            return builder.ToString();
        }

        public bool                 isCoroutineExpired
        {
            get
            {
                return state.isDone;
            }
        }
        public bool                 isTimeExpired
        {
            get
            {
                switch (timerContext)
                {
                    case TimerContext.Time:
                        return startTime + delayTime <= Time.time;
                    case TimerContext.UnscaledTime:
                        return startTime + delayTime <= Time.unscaledTime;
                    case TimerContext.FixedTime:
                        return startTime + delayTime <= Time.fixedTime;
                    case TimerContext.RealTime:
                        return startTime + delayTime <= Time.realtimeSinceStartup;
                    default:
                        return true;
                }
            }
        }
        public bool                 isDelegateExpired
        {
            get
            {
                bool check = func();
                return delegateContext == DelegateContext.WaitUntil ? check : !check;
            }
        }
        public bool                 isExpired
        {
            get
            {
                switch (flag)
                {
                    case ContextFlag.Coroutine:
                        return isCoroutineExpired;
                    case ContextFlag.Timer:
                        return isTimeExpired;
                    case ContextFlag.Delegate:
                        return isDelegateExpired;
                    case ContextFlag.TimerAndDelegate:
                        return isTimeExpired && isDelegateExpired;
                    case ContextFlag.TimerOrDelegate:
                        return isTimeExpired || isDelegateExpired;
                    case ContextFlag.Default:
                    // fall through
                    default:
                        return true;
                }
            }
        }

        public static Context CreateCoroutine(InterruptContext _interruptContext, CoroutineState state)
        {
            return new Context() { flag = ContextFlag.Coroutine, interruptContext = _interruptContext, state = state };
        }

        public static Context CreateCoroutine(CoroutineState state)
        {
            return new Context() { flag = ContextFlag.Coroutine, interruptContext = InterruptContext.Update, state = state };
        }

        #region Interrupt Variance

        public static Context CreateInterrupter(InterruptContext _interruptContext)
        {
            return new Context() { flag = ContextFlag.Default, interruptContext = _interruptContext };
        }

        public static Context CreateUpdate()
        {
            return new Context() { flag = ContextFlag.Default, interruptContext = InterruptContext.Update };
        }
        public static Context CreateFixedUpdate()
        {
            return new Context() { flag = ContextFlag.Default, interruptContext = InterruptContext.FixedUpdate };
        }
        public static Context CreateLateUpdate()
        {
            return new Context() { flag = ContextFlag.Default, interruptContext = InterruptContext.LateUpdate };
        }
        public static Context CreateEndOfFrame()
        {
            return new Context() { flag = ContextFlag.Default, interruptContext = InterruptContext.EndOfFrame };
        }

        #endregion

        #region Delegate Variance

        public static Context CreateDelegate(InterruptContext _interruptContext, DelegateContext _delegateContext, Func<bool> _func)
        {
            return new Context() { flag = ContextFlag.Delegate, interruptContext = _interruptContext, delegateContext = _delegateContext, func = _func };
        }
        public static Context CreateDelegate(DelegateContext _delegateContext, Func<bool> _func)
        {
            return new Context() { flag = ContextFlag.Delegate, delegateContext = _delegateContext, func = _func };
        }

        public static Context CreateWaitWhile(InterruptContext _interruptContext, Func<bool> _func)
        {
            return new Context() { flag = ContextFlag.Delegate, interruptContext = _interruptContext, delegateContext = DelegateContext.WaitWhile, func = _func };
        }
        public static Context CreateWaitWhile(Func<bool> _func)
        {
            return new Context() { flag = ContextFlag.Delegate, delegateContext = DelegateContext.WaitWhile, func = _func };
        }

        public static Context CreateWaitUntil(InterruptContext _interruptContext, Func<bool> _func)
        {
            return new Context() { flag = ContextFlag.Delegate, interruptContext = _interruptContext, delegateContext = DelegateContext.WaitUntil, func = _func };
        }
        public static Context CreateWaitUntil(Func<bool> _func)
        {
            return new Context() { flag = ContextFlag.Delegate, delegateContext = DelegateContext.WaitUntil, func = _func };
        }

        #endregion

        #region Timer Variance

        public static Context CreateTimer(InterruptContext _interruptContext, TimerContext _timerContext, float _startTime, float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, interruptContext = _interruptContext, timerContext = _timerContext, startTime = _startTime, delayTime = _delayTime };
        }
        public static Context CreateTimer(InterruptContext _interruptContext, TimerContext _timerContext, float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, interruptContext = _interruptContext, timerContext = _timerContext, startTime = _timerContext == TimerContext.Time ? Time.time : _timerContext == TimerContext.RealTime ? Time.realtimeSinceStartup : Time.unscaledTime, delayTime = _delayTime };
        }
        public static Context CreateTimer(TimerContext _timerContext, float _startTime, float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, startTime = _startTime, delayTime = _delayTime, timerContext = _timerContext };
        }
        public static Context CreateTimer(TimerContext _timerContext, float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, timerContext = _timerContext, startTime = _timerContext == TimerContext.Time ? Time.time : _timerContext == TimerContext.RealTime ? Time.realtimeSinceStartup : Time.unscaledTime, delayTime = _delayTime };
        }

        public static Context CreateNormalTimer(InterruptContext _interruptContext, float _startTime, float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, interruptContext = _interruptContext, timerContext = TimerContext.Time, startTime = _startTime, delayTime = _delayTime };
        }
        public static Context CreateNormalTimer(InterruptContext _interruptContext, float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, interruptContext = _interruptContext, timerContext = TimerContext.Time, startTime = Time.time, delayTime = _delayTime };
        }
        public static Context CreateNormalTimer(float _startTime, float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, timerContext = TimerContext.Time, startTime = _startTime, delayTime = _delayTime };
        }
        public static Context CreateNormalTimer(float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, timerContext = TimerContext.Time, startTime = Time.time, delayTime = _delayTime };
        }

        public static Context CreateUnscaledTimer(InterruptContext _interruptContext, float _startTime, float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, interruptContext = _interruptContext, timerContext = TimerContext.UnscaledTime, startTime = _startTime, delayTime = _delayTime };
        }
        public static Context CreateUnscaledTimer(InterruptContext _interruptContext, float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, interruptContext = _interruptContext, timerContext = TimerContext.UnscaledTime, startTime = Time.unscaledTime, delayTime = _delayTime };
        }
        public static Context CreateUnscaledTimer(float _startTime, float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, timerContext = TimerContext.UnscaledTime, startTime = _startTime, delayTime = _delayTime };
        }
        public static Context CreateUnscaledTimer(float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, timerContext = TimerContext.UnscaledTime, startTime = Time.unscaledTime, delayTime = _delayTime };
        }

        public static Context CreateRealTimer(InterruptContext _interruptContext, float _startTime, float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, interruptContext = _interruptContext, timerContext = TimerContext.RealTime, startTime = _startTime, delayTime = _delayTime };
        }
        public static Context CreateRealTimer(InterruptContext _interruptContext, float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, interruptContext = _interruptContext, timerContext = TimerContext.RealTime, startTime = Time.realtimeSinceStartup, delayTime = _delayTime };
        }
        public static Context CreateRealTimer(float _startTime, float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, timerContext = TimerContext.RealTime, startTime = _startTime, delayTime = _delayTime };
        }
        public static Context CreateRealTimer(float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, timerContext = TimerContext.RealTime, startTime = Time.realtimeSinceStartup, delayTime = _delayTime };
        }

        public static Context CreateFixedTimer(InterruptContext _interruptContext, float _startTime, float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, interruptContext = _interruptContext, timerContext = TimerContext.FixedTime, startTime = _startTime, delayTime = _delayTime };
        }
        public static Context CreateFixedTimer(InterruptContext _interruptContext, float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, interruptContext = _interruptContext, timerContext = TimerContext.FixedTime, startTime = Time.realtimeSinceStartup, delayTime = _delayTime };
        }
        public static Context CreateFixedTimer(float _startTime, float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, timerContext = TimerContext.FixedTime, startTime = _startTime, delayTime = _delayTime };
        }
        public static Context CreateFixedTimer(float _delayTime)
        {
            return new Context() { flag = ContextFlag.Timer, timerContext = TimerContext.FixedTime, startTime = Time.realtimeSinceStartup, delayTime = _delayTime };
        }

        #endregion

        #region TimerAndDelegate Variance

        public static Context CreateDelegateAndTimer(InterruptContext _interruptContext, DelegateContext _delegateContext, Func<bool> _func, TimerContext _timerContext, float _startTime, float _delayTime)
        {
            return new Context() { flag = ContextFlag.TimerAndDelegate, interruptContext = _interruptContext, timerContext = _timerContext, startTime = _startTime, delayTime = _delayTime };
        }
        public static Context CreateDelegateAndTimer(DelegateContext _delegateContext, Func<bool> _func, TimerContext _timerContext, float _startTime, float _delayTime)
        {
            return new Context() { flag = ContextFlag.TimerAndDelegate, timerContext = _timerContext, startTime = _startTime, delayTime = _delayTime };
        }
        public static Context CreateDelegateAndTimer(InterruptContext _interruptContext, DelegateContext _delegateContext, Func<bool> _func, TimerContext _timerContext, float _delayTime)
        {
            return new Context() { flag = ContextFlag.TimerAndDelegate, interruptContext = _interruptContext, timerContext = _timerContext, startTime = _timerContext == TimerContext.Time ? Time.time : _timerContext == TimerContext.RealTime ? Time.realtimeSinceStartup : Time.unscaledTime, delayTime = _delayTime };
        }
        public static Context CreateDelegateAndTimer(DelegateContext _delegateContext, Func<bool> _func, TimerContext _timerContext, float _delayTime)
        {
            return new Context() { flag = ContextFlag.TimerAndDelegate, timerContext = _timerContext, startTime = _timerContext == TimerContext.Time ? Time.time : _timerContext == TimerContext.RealTime ? Time.realtimeSinceStartup : Time.unscaledTime, delayTime = _delayTime };
        }

        #endregion

        #region TimerOrDelegate Variance

        public static Context CreateTimerOrDelegate(InterruptContext _interruptContext, DelegateContext _delegateContext, Func<bool> _func, TimerContext _timerContext, float _startTime, float _delayTime)
        {
            return new Context() { flag = ContextFlag.TimerOrDelegate, interruptContext = _interruptContext, timerContext = _timerContext, startTime = _startTime, delayTime = _delayTime };
        }
        public static Context CreateTimerOrDelegate(DelegateContext _delegateContext, Func<bool> _func, TimerContext _timerContext, float _startTime, float _delayTime)
        {
            return new Context() { flag = ContextFlag.TimerOrDelegate, timerContext = _timerContext, startTime = _startTime, delayTime = _delayTime };
        }
        public static Context CreateTimerOrDelegate(InterruptContext _interruptContext, DelegateContext _delegateContext, Func<bool> _func, TimerContext _timerContext, float _delayTime)
        {
            return new Context() { flag = ContextFlag.TimerOrDelegate, interruptContext = _interruptContext, timerContext = _timerContext, startTime = _timerContext == TimerContext.Time ? Time.time : _timerContext == TimerContext.RealTime ? Time.realtimeSinceStartup : Time.unscaledTime, delayTime = _delayTime };
        }
        public static Context CreateTimerOrDelegate(DelegateContext _delegateContext, Func<bool> _func, TimerContext _timerContext, float _delayTime)
        {
            return new Context() { flag = ContextFlag.TimerOrDelegate, timerContext = _timerContext, startTime = _timerContext == TimerContext.Time ? Time.time : _timerContext == TimerContext.RealTime ? Time.realtimeSinceStartup : Time.unscaledTime, delayTime = _delayTime };
        }

        #endregion
    }

    public struct Tag
    {
        public string str;

        public bool empty { get { return string.IsNullOrEmpty(str); } }

        public override string ToString()
        {
            return string.Format("{0}(\"{1}\")", GetType().ToString(), str);
        }
    }
}