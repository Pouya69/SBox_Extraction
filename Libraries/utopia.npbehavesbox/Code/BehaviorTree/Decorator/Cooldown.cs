using System;
using Sandbox;
using Sandbox.Diagnostics;
namespace NPBehave
{

    public class Cooldown : Decorator
    {
        private bool _startAfterDecoratee = false;
        private bool _resetOnFailiure = false;
	    private bool _failOnCooldown = false;
        private float _cooldownTime = 0.0f;
        private float _randomVariation = 0.05f;
        private bool _isReady = true;

        /// <summary>
        /// The Cooldown decorator ensures that the branch can not be started twice within the given cooldown time.
        /// 
        /// The decorator can start the cooldown timer right away or wait until the child stopps, you can control this behavior with the
        /// `startAfterDecoratee` parameter.
        /// 
        /// The default behavior in case the cooldown timer is active and this node is started again is, that the decorator waits until
        /// the cooldown is reached and then executes the underlying node.
        /// You can change this behavior with the `failOnCooldown` parameter to make the decorator immediately fail instead.
        /// 
        /// </summary>
        /// <param name="cooldownTime">time until next execution</param>
        /// <param name="randomVariation">random variation added to the cooldown time</param>
        /// <param name="startAfterDecoratee">If set to <c>true</c> the cooldown timer is started from the point after the decoratee has been started, else it will be started right away.</param>
        /// <param name="resetOnFailiure">If set to <c>true</c> the timer will be reset in case the underlying node fails.</param>
        /// <param name="failOnCooldown">If currently on cooldown and this parameter is set to <c>true</c>, the decorator will immmediately fail instead of waiting for the cooldown.</param>
        /// <param name="decoratee">Decoratee node.</param>
        public Cooldown(float cooldownTime, float randomVariation, bool startAfterDecoratee, bool resetOnFailiure, bool failOnCooldown, Node decoratee) : base("TimeCooldown", decoratee)
        {
        	_startAfterDecoratee = startAfterDecoratee;
        	_cooldownTime = cooldownTime;
        	_resetOnFailiure = resetOnFailiure;
        	_randomVariation = randomVariation;
        	_failOnCooldown = failOnCooldown;
        	Assert.True(cooldownTime > 0f, "cooldownTime has to be set");
        }

        public Cooldown(float cooldownTime, bool startAfterDecoratee, bool resetOnFailiure, bool failOnCooldown, Node decoratee) : base("TimeCooldown", decoratee)
        {
        	_startAfterDecoratee = startAfterDecoratee;
        	_cooldownTime = cooldownTime;
        	_randomVariation = cooldownTime * 0.1f; 
        	_resetOnFailiure = resetOnFailiure;
        	_failOnCooldown = failOnCooldown;
        	Assert.True(cooldownTime > 0f, "cooldownTime has to be set");
        }

        public Cooldown(float cooldownTime, float randomVariation, bool startAfterDecoratee, bool resetOnFailiure, Node decoratee) : base("TimeCooldown", decoratee)
        {
        	_startAfterDecoratee = startAfterDecoratee;
        	_cooldownTime = cooldownTime;
        	_resetOnFailiure = resetOnFailiure;
        	_randomVariation = randomVariation;
        	Assert.True(cooldownTime > 0f, "cooldownTime has to be set");
        }

        public Cooldown(float cooldownTime, bool startAfterDecoratee, bool resetOnFailiure, Node decoratee) : base("TimeCooldown", decoratee)
        {
            _startAfterDecoratee = startAfterDecoratee;
            _cooldownTime = cooldownTime;
            _randomVariation = cooldownTime * 0.1f;
            _resetOnFailiure = resetOnFailiure;
            Assert.True(cooldownTime > 0f, "cooldownTime has to be set");
        }

        public Cooldown(float cooldownTime, float randomVariation, Node decoratee) : base("TimeCooldown", decoratee)
        {
            _startAfterDecoratee = false;
            _cooldownTime = cooldownTime;
            _resetOnFailiure = false;
            _randomVariation = randomVariation;
        	Assert.True(cooldownTime > 0f, "cooldownTime has to be set");
        }

        public Cooldown(float cooldownTime, Node decoratee) : base("TimeCooldown", decoratee)
        {
            _startAfterDecoratee = false;
            _cooldownTime = cooldownTime;
            _resetOnFailiure = false;
            _randomVariation = cooldownTime * 0.1f;
        	Assert.True(cooldownTime > 0f, "cooldownTime has to be set");
        }

        protected override void DoStart()
        {
            if (_isReady)
            {
                _isReady = false;
                if (!_startAfterDecoratee)
                {
                    Clock.AddTimer(_cooldownTime, _randomVariation, 0, TimeoutReached);
                    _started = true;
                    _timeSinceStart = 0;
                }
                Decoratee.Start();
            }
            else
            {
                if (_failOnCooldown)
                {
                    Stopped(false);
                }
            }
        }

        protected override void DoStop()
        {
            if (Decoratee.IsActive)
            {
                _isReady = true;
                Clock.RemoveTimer(TimeoutReached);
                Decoratee.Stop();
            }
            else
            {
                _isReady = true;
                Clock.RemoveTimer(TimeoutReached);
                Stopped(false);
            }
        }

        protected override void DoChildStopped(Node child, bool result)
        {
            if (_resetOnFailiure && !result)
            {
                _isReady = true;
                Clock.RemoveTimer(TimeoutReached);
            }
            else if (_startAfterDecoratee)
            {
                Clock.AddTimer(_cooldownTime, _randomVariation, 0, TimeoutReached);
                _started = true;
                _timeSinceStart = 0;
            }
            Stopped(result);
        }

        private void TimeoutReached()
        {
            if (IsActive && !Decoratee.IsActive)
            {
                Clock.AddTimer(_cooldownTime, _randomVariation, 0, TimeoutReached);
                Decoratee.Start();
            }
            else
            {
                _isReady = true;
                _started = false;
            }
        }

#if DEBUG

	    private TimeSince _timeSinceStart = 0;
	    private bool _started = false;
	    public override string DebugIcon => "timer";
	    public override string ComputedLabel
	    {
		    get
		    {
			    return _started ? $"{_cooldownTime - _timeSinceStart:F1}s (Total: {_cooldownTime:F1})" : $"{_cooldownTime:F1}s";
		    }
	    }
#endif
    }
}
