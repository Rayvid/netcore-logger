using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;

namespace Planet.Library.Logging
{
    public class LogMessage
    {
        [DataContract]
        public class Dto
        {
            [DataMember]
            public string Category;

            [DataMember]
            public DateTime Timestamp;

            [DataMember]
            public int LogLevel;

            [DataMember]
            public string LogLevelName;

            [DataMember]
            public string Message;

            [DataMember]
            public string Exception;

            [DataMember]
            public string Stacktrace;

            [DataMember]
            public Dictionary<string, string> Scopes;
        }

        public string Category { get; private set; }
        public DateTime Timestamp { get; private set; }
        public LogLevel LogLevel { get; private set; }
        public string Message { get; private set; }
        public Exception Exception { get; private set; }
        public IEnumerable<object> Scopes { get; private set; }

        public LogMessage(
            string category,
            LogLevel logLevel,
            string message,
            Exception exception,
            IEnumerable<object> scopes)
        {
            Category = category;
            Timestamp = DateTime.UtcNow;
            LogLevel = logLevel;
            Message = message;
            Exception = exception;
            Scopes = scopes;
        }

        public Dto GetDto()
        {
            var scopes = new Dictionary<string, string>();
            var scopeNesting = "_";
            foreach (var scope in Scopes)
            {
                /* From GELF provider documentation:
                     ...Log scopes can also be used to attach fields to a group of related logs.
                     Create a log scope with a ValueTuple<string, string>, ValueTuple<string, int/byte/double> (or any other numeric value) or Dictionary<string, object> to do so.
                     Note that any other types passed to BeginScope() will be ignored, including Dictionary<string, string> and ValueTuple<string, object>...
                   So i am narrowing this to Dictionary<string, object> and Tuple<string, string> and Tuple<string, int>.
                     Anything else will not be ignored as they do it in GELF provider, but just converted to string */
                if (scope as Dictionary<string, object> != null)
                {
                    foreach (var ss in scope as Dictionary<string, object>)
                    {
                        AddScope(scopes, ref scopeNesting, ss.Key, ss.Value.ToString());
                    }
                }
                else if (scope is ValueTuple<string, string>)
                {
                    var ss = (ValueTuple<string, string>)scope;
                    AddScope(scopes, ref scopeNesting, ss.Item1, ss.Item2);
                }
                else if (scope is ValueTuple<string, int>)
                {
                    var ss = (ValueTuple<string, int>)scope;
                    AddScope(scopes, ref scopeNesting, ss.Item1, ss.Item2.ToString());
                }
                else
                {
                    scopes.Add(scopeNesting, scope.ToString());
                    scopeNesting += "_";
                }
            }

            return new Dto
            {
                Category = Category,
                Timestamp = Timestamp,
                LogLevel = (int)LogLevel,
                LogLevelName = LogLevel.ToString(),
                Message = Message,
                Exception = Exception?.GetBaseException().Message,
                Stacktrace = Exception?.StackTrace,
                Scopes = scopes
            };
        }

        private static void AddScope(Dictionary<string, string> scopes, ref string scopeNesting, string key, string value)
        {
            try
            {
                scopes.Add(key, value);
            }
            catch
            {
                scopes.Add(scopeNesting + key, value);
                scopeNesting += "_";
            }
        }
    }
}
