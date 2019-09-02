using Blish_HUD.Properties;

namespace Blish_HUD.Contexts {

    /// <summary>
    /// The result when querying a <see cref="Context"/>.
    /// </summary>
    /// <typeparam name="T">The type the call returns.</typeparam>
    public struct ContextResult<T> {

        /// <summary>
        /// The value requested from the <see cref="Context"/>.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// A message indicating the result of the context request.
        /// This string is suitable to display in the UI.
        /// </summary>
        public string Status { get; }

        /// <summary>
        /// Creates an instance of <see cref="ContextResult{T}"/>
        /// which contains the return <c>T</c> <see cref="Value"/>
        /// of a context's call and a <c>string</c> <see cref="Status"/>
        /// describing the result of getting the call.
        /// </summary>
        /// <param name="value">The result to send back to the caller.</param>
        /// <param name="status">The summary status of the result.  This value should be UI ready.</param>
        public ContextResult(T value, string status) {
            this.Value  = value;
            this.Status = status;
        }

        /// <summary>
        /// Creates an instance of <see cref="ContextResult{T}"/>
        /// which contains the return <c>T</c> <see cref="Value"/>
        /// of a context's call and defines the <see cref="Status"/>
        /// as "Succeeded".  This call is a shortcut for a successfull
        /// calls.
        /// </summary>
        /// <param name="value">The result to send back to the caller.</param>
        public ContextResult(T value) {
            this.Value  = value;
            this.Status = Strings.Context_ResultSuccess;
        }

        public static implicit operator T(ContextResult<T> contextResult) {
            return contextResult.Value;
        }

    }

}
