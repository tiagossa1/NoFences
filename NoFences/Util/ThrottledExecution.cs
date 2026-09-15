using System;
using System.Threading.Tasks;

namespace NoFences.Util
{
    /// <summary>
    /// Throttles repeated invocations of an <see cref="Action"/> so that it runs
    /// at most once per configured <see cref="TimeSpan"/>, deferring excess calls
    /// until the delay has elapsed.
    /// </summary>
    public class ThrottledExecution
    {
        private readonly TimeSpan delay;

        private DateTime lastExecution = DateTime.Now;

        private TimeSpan TimeSinceLastExecution => DateTime.Now - lastExecution;

        private volatile bool isAwaiting;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottledExecution"/> class.
        /// </summary>
        /// <param name="delay">The minimum interval that must elapse between executions.</param>
        public ThrottledExecution(TimeSpan delay)
        {
            this.delay = delay;
        }

        /// <summary>
        /// Executes <paramref name="action"/> immediately if enough time has passed since
        /// the last execution, otherwise schedules it to run once the throttle delay elapses.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is <see langword="null"/>.</exception>
        public async void Run(Action action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                if (TimeSinceLastExecution > delay)
                {
                    action.Invoke();
                }
                else if (!isAwaiting)
                {
                    isAwaiting = true;
                    while (TimeSinceLastExecution < delay)
                    {
                        await Task.Delay((int)(delay.TotalMilliseconds - TimeSinceLastExecution.TotalMilliseconds));
                        action.Invoke();
                    }
                    isAwaiting = false;
                }
                lastExecution = DateTime.Now;
            }
            catch (Exception ex)
            {
                // Prevent an unhandled exception in this fire-and-forget async void
                // method from crashing the process.
                isAwaiting = false;
                System.Diagnostics.Debug.WriteLine($"ThrottledExecution.Run failed: {ex}");
            }
        }
    }
}
