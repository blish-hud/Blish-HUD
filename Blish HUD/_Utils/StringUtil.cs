using System;
using System.Diagnostics;
using System.Text;

namespace Blish_HUD {
    public static class StringUtil {
        private const char REPLACEMENT_CHARACTER = '\uFFFD';

        /// <summary>
        /// Returns a new string in which all occurrences of a specified string in the current instance are replaced with another 
        /// specified string according the type of search to use for the specified string.
        /// </summary>
        /// <param name="str">The string performing the replace method.</param>
        /// <param name="oldValue">The string to be replaced.</param>
        /// <param name="newValue">The string replace all occurrences of <paramref name="oldValue"/>. 
        /// If value is equal to <c>null</c>, than all occurrences of <paramref name="oldValue"/> will be removed from the <paramref name="str"/>.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
        /// <returns>A string that is equivalent to the current string except that all instances of <paramref name="oldValue"/> are replaced with <paramref name="newValue"/>. 
        /// If <paramref name="oldValue"/> is not found in the current instance, the method returns the current instance unchanged.</returns>
        /// <remarks>https://stackoverflow.com/a/45756981/595437</remarks>
        [DebuggerStepThrough]
        public static string ReplaceUsingStringComparison(string str, string oldValue, string newValue, StringComparison comparisonType) {
            // Check inputs.
            if (str == null) {
                // Same as original .NET C# string.Replace behavior.
                throw new ArgumentNullException(nameof(str));
            }
            if (str.Length == 0) {
                // Same as original .NET C# string.Replace behavior.
                return str;
            }
            if (oldValue == null) {
                // Same as original .NET C# string.Replace behavior.
                throw new ArgumentNullException(nameof(oldValue));
            }
            if (oldValue.Length == 0) {
                // Same as original .NET C# string.Replace behavior.
                throw new ArgumentException("String cannot be of zero length.");
            }

            // Prepare string builder for storing the processed string.
            // Note: StringBuilder has a better performance than String by 30-40%.
            StringBuilder resultStringBuilder = new StringBuilder(str.Length);

            // Analyze the replacement: replace or remove.
            bool isReplacementNullOrEmpty = string.IsNullOrEmpty(newValue);

            // Replace all values.
            const int valueNotFound = -1;
            int foundAt;
            int startSearchFromIndex = 0;
            while ((foundAt = str.IndexOf(oldValue, startSearchFromIndex, comparisonType)) != valueNotFound) {
                // Append all characters until the found replacement.
                int charsUntilReplacment = foundAt - startSearchFromIndex;
                bool isNothingToAppend = charsUntilReplacment == 0;
                if (!isNothingToAppend) {
                    resultStringBuilder.Append(str, startSearchFromIndex, charsUntilReplacment);
                }

                // Process the replacement.
                if (!isReplacementNullOrEmpty) {
                    resultStringBuilder.Append(newValue);
                }

                // Prepare start index for the next search.
                // This needed to prevent infinite loop, otherwise method always start search 
                // from the start of the string. For example: if an oldValue == "EXAMPLE", newValue == "example"
                // and comparisonType == "any ignore case" will conquer to replacing:
                // "EXAMPLE" to "example" to "example" to "example" … infinite loop.
                startSearchFromIndex = foundAt + oldValue.Length;
                if (startSearchFromIndex == str.Length) {
                    // It is end of the input string: no more space for the next search.
                    // The input string ends with a value that has already been replaced. 
                    // Therefore, the string builder with the result is complete and no further action is required.
                    return resultStringBuilder.ToString();
                }
            }

            // Append the last part to the result.
            int charsUntilStringEnd = str.Length - startSearchFromIndex;
            resultStringBuilder.Append(str, startSearchFromIndex, charsUntilStringEnd);

            return resultStringBuilder.ToString();
        }

        /// <summary>
        /// Checks that the string is well-formed.
        /// (Does not contain non-matching or truncated surrogate pairs)
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>A value indicating whether the string is well-formed.</returns>
        internal static bool IsWellFormed(this string input) {
            if (input == null) {
                throw new ArgumentNullException(nameof(input));
            }

            return IsWellFormed(input, 0, input.Length);
        }

        /// <summary>
        /// Checks that a part of the string is well-formed.
        /// (Does not contain non-matching or truncated surrogate pairs)
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="offset">The offset to start validating from.</param>
        /// <param name="length">The length of the input to validate.</param>
        /// <returns>A value indicating whether the string is well-formed.</returns>
        internal static bool IsWellFormed(this string input, int offset, int length) {
            if (input == null) {
                throw new ArgumentNullException(nameof(input));
            }

            if (offset > input.Length) {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            int end = offset + length;
            if (end > input.Length) {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            for (int i = offset; i < end; i++) {
                char current = input[i];

                if (char.IsHighSurrogate(current)) {
                    // A high surrogate must be followed by a low surrogate.
                    if (i + 1 >= end || !char.IsLowSurrogate(input[i + 1])) {
                        return false;
                    }

                    i++;
                } else if (char.IsLowSurrogate(current)) {
                    // A low surrogate without a preceding high surrogate is invalid.
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Replaces any invalid surrogate pairs within the string with the replacement character.
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>A string with any invalid surrogates replaced with the replacement character.
        /// If no invalid surrogates are found, returns the input string unaltered.</returns>
        internal static string ReplaceInvalidSurrogates(this string input) {
            if (input == null) {
                throw new ArgumentNullException(nameof(input));
            }

            return ReplaceInvalidSurrogates(input, 0, input.Length);
        }

        /// <summary>
        /// Checks that a part of the string is well-formed.
        /// (Does not contain non-matching or truncated surrogate pairs)
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="offset">The offset to start validating from.</param>
        /// <param name="length">The length of the input to validate.</param>
        /// <returns>A string with any invalid surrogates replaced with the replacement character.
        /// If no invalid surrogates are found, returns the input string unaltered.</returns>
        internal static string ReplaceInvalidSurrogates(this string input, int offset, int length) {
            if (input == null) {
                throw new ArgumentNullException(nameof(input));
            }

            if (offset > input.Length) {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            int end = offset + length;
            if (end > input.Length) {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            StringBuilder replacement = null;
            for (int i = offset; i < end; i++) {
                char current = input[i];

                if (char.IsHighSurrogate(current)) {
                    // A high surrogate must be followed by a low surrogate.
                    if (i + 1 >= end || !char.IsLowSurrogate(input[i + 1])) {
                        replacement ??= new StringBuilder(input, input.Length);
                        replacement[i] = REPLACEMENT_CHARACTER;
                    }

                    i++;
                } else if (char.IsLowSurrogate(current)) {
                    // A low surrogate without a preceding high surrogate is invalid.
                    replacement ??= new StringBuilder(input, input.Length);
                    replacement[i] = REPLACEMENT_CHARACTER;
                }
            }

            return replacement?.ToString() ?? input;
        }

        /// <summary>
        /// Returns the number of utf-16 characters needed
        /// to represent this utf-32 codepoint.
        /// </summary>
        /// <param name="utf32"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static int GetUtf16CharCountFromUtf32(int utf32) {
            if (utf32 < 0 || utf32 > 1114111 || (utf32 >= 55296 && utf32 <= 57343)) {
                throw new ArgumentOutOfRangeException("utf32", "InvalidUTF32");
            }

            return utf32 <= char.MaxValue ? 1 : 2;
        }
    }
}
