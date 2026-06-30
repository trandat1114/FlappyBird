using System;

namespace FlappyBird.Game.Modes.TwoPlayer
{
    /// <summary>
    /// Xử lý countdown logic cho TwoPlayerGameMode
    /// </summary>
    public class TwoPlayerCountdown
    {
        private bool isCountingDown = false;
        private int countdownValue = 3;
        private DateTime countdownStartTime;

        public bool IsCountingDown => isCountingDown;
        public int CountdownValue => countdownValue;

        /// <summary>
        /// Bắt đầu countdown
        /// </summary>
        public void StartCountdown()
        {
            isCountingDown = true;
            countdownValue = 3;
            countdownStartTime = DateTime.Now;
        }

        /// <summary>
        /// Reset countdown state
        /// </summary>
        public void ResetCountdown()
        {
            isCountingDown = false;
            countdownValue = 3;
            countdownStartTime = DateTime.MinValue;
        }

        /// <summary>
        /// Update countdown logic
        /// </summary>
        /// <returns>True nếu countdown đã kết thúc</returns>
        public bool UpdateCountdown()
        {
            if (!isCountingDown) return false;

            int elapsed = (int)(DateTime.Now - countdownStartTime).TotalSeconds;
            int newCountdown = 3 - elapsed;
            if (newCountdown != countdownValue)
            {
                countdownValue = newCountdown;
            }
            if (countdownValue <= 0)
            {
                isCountingDown = false;
                return true; // Countdown finished
            }
            return false; // Still counting down
        }
    }
}
