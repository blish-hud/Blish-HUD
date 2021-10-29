﻿using System;
using System.Threading.Tasks;
using Gw2Sharp.WebApi.Exceptions;

namespace Blish_HUD.Gw2WebApi {
    public class TokenBucket {

        private const int REFILL_INTERVAL        = 1000;
        private const int FAILED_CONSUME_RETRIES = 3;

        /// <summary>
        /// The maximum number of tokens in the bucket.
        /// </summary>
        /// <remarks>
        /// Also represents the maximum burst.
        /// </remarks>
        public double MaxTokens { get; set; }

        private double _tokens;

        /// <summary>
        /// How many tokens are currently available in the <see cref="TokenBucket"/>.
        /// </summary>
        /// <remarks>
        /// Tokens can be negative if there are multiple calls attempting to be made after all tokens have been exhausted and represents a token debt.
        /// </remarks>
        public double Tokens {
            get => GetLatestTokenBalance();
            set {
                lock (_tokenLock) {
                    _tokens = Math.Min(value, this.MaxTokens);
                }
            }
        }

        /// <summary>
        /// The number of tokens added to the bucket each second.
        /// </summary>
        public double RefillAmount { get; set; }

        private readonly object _tokenLock = new object();

        private DateTime _lastUpdate;

        public TokenBucket(double maxBurst, double refillAmountPerSecond) {
            this.MaxTokens    = maxBurst;
            this.RefillAmount = refillAmountPerSecond;
            this.Tokens       = maxBurst;

            _lastUpdate = DateTime.Now;
        }

        private double GetLatestTokenBalance() {
            lock (_tokenLock) {
                double elapsedTime = (DateTime.Now - _lastUpdate).TotalMilliseconds / REFILL_INTERVAL;
                _lastUpdate = DateTime.Now;

                return _tokens = Math.Min(_tokens + elapsedTime * this.RefillAmount, this.MaxTokens);
            }
        }

        public async Task<T> ConsumeCompliant<T>(Func<Task<T>> updateFunc, int remainingAttempts = FAILED_CONSUME_RETRIES) {
            GetLatestTokenBalance();

            double remainingTokens = --this.Tokens;

            if (remainingTokens < 0) {
                await Task.Delay((int)Math.Ceiling(-remainingTokens * (REFILL_INTERVAL / this.RefillAmount)));
            }

            try {
                return await updateFunc().ConfigureAwait(false);
            } catch (TooManyRequestsException) {
                // Penalize our token count - we've hit the rate limited
                this.Tokens = Math.Min(this.Tokens - this.RefillAmount, -1);

                if (remainingAttempts > 0) {
                    return await ConsumeCompliant<T>(updateFunc, remainingAttempts - 1);
                }

                // Too many attempts
                throw;
            }
        }

    }
}
