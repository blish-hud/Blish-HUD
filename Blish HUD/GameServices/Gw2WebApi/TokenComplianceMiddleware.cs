using System;
using System.Threading;
using System.Threading.Tasks;
using Gw2Sharp.WebApi.Http;
using Gw2Sharp.WebApi.Middleware;

namespace Blish_HUD.Gw2WebApi {
    public class TokenComplianceMiddleware : IWebApiMiddleware {

        private readonly TokenBucket _bucket;

        public TokenComplianceMiddleware(TokenBucket tokenBucket) {
            _bucket = tokenBucket;
        }

        public async Task<IWebApiResponse> OnRequestAsync(MiddlewareContext context, Func<MiddlewareContext, CancellationToken, Task<IWebApiResponse>> callNext, CancellationToken cancellationToken = new CancellationToken()) {
            return await _bucket.ConsumeCompliant(() => callNext(context, cancellationToken)).ConfigureAwait(false);
        }

    }
}
