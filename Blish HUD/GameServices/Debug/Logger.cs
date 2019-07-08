using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Blish_HUD {
    public class Logger {

        public static Logger GetLogger(Type type) {
            return new Logger(type);
        }

        private readonly NLog.Logger _internalLogger;

        private Logger(Type type) {
            _internalLogger = LogManager.GetLogger(type.AssemblyQualifiedName);
        }

        #region Trace

        public void Trace(string message) {
            _internalLogger.Trace(message);
        }

        public void Trace(string message, params object[] args) {
            _internalLogger.Trace(message, args);
        }

        public void Trace(Exception exception, string message) {
            _internalLogger.Trace(exception, message);
        }

        public void Trace(Exception exception, string message, params object[] args) {
            _internalLogger.Trace(exception, message, args);
        }

        #endregion

        #region Debug

        public void Debug(string message) {
            _internalLogger.Debug(message);
        }

        public void Debug(string message, params object[] args) {
            _internalLogger.Debug(message, args);
        }

        public void Debug(Exception exception, string message) {
            _internalLogger.Debug(exception, message);
        }

        public void Debug(Exception exception, string message, params object[] args) {
            _internalLogger.Debug(exception, message, args);
        }

        #endregion

        #region Info

        public void Info(string message) {
            _internalLogger.Info(message);
        }

        public void Info(string message, params object[] args) {
            _internalLogger.Info(message, args);
        }

        public void Info(Exception exception, string message) {
            _internalLogger.Info(exception, message);
        }

        public void Info(Exception exception, string message, params object[] args) {
            _internalLogger.Info(exception, message, args);
        }

        #endregion

        #region Warn

        public void Warn(string message) {
            _internalLogger.Warn(message);
        }

        public void Warn(string message, params object[] args) {
            _internalLogger.Warn(message, args);
        }

        public void Warn(Exception exception, string message) {
            _internalLogger.Warn(exception, message);
        }

        public void Warn(Exception exception, string message, params object[] args) {
            _internalLogger.Warn(exception, message, args);
        }

        #endregion

        #region Error

        public void Error(string message) {
            _internalLogger.Error(message);
        }

        public void Error(string message, params object[] args) {
            _internalLogger.Error(message, args);
        }

        public void Error(Exception exception, string message) {
            _internalLogger.Error(exception, message);
        }

        public void Error(Exception exception, string message, params object[] args) {
            _internalLogger.Error(exception, message, args);
        }

        #endregion

        #region Fatal

        public void Fatal(string message) {
            _internalLogger.Fatal(message);
        }

        public void Fatal(string message, params object[] args) {
            _internalLogger.Fatal(message, args);
        }

        public void Fatal(Exception exception, string message) {
            _internalLogger.Fatal(exception, message);
        }

        public void Fatal(Exception exception, string message, params object[] args) {
            _internalLogger.Fatal(exception, message, args);
        }

        #endregion

    }
}
