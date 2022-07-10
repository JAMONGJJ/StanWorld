using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace stanworld
{
    using System.Diagnostics;
    public static class LogFileWriter // attribute searching
    {
        public enum LogType
        {
            DEFAULT = 0,
            EXCEPTION,
            PACKET_SEND_TO_ROOM,
            PACKET_SEND_TO_ROOM_WAITING_RESPONSE,
            PACKET_SEND_TO_ZONE,
            PACKET_SEND_TO_ZONE_WAITING_RESPONSE,
            PACKET_RECEIVE,
            PACKET_RECEIVE_TIMEOUT,
            UI_OPEN,
            UI_CLOSE,
            BUTTON_PRESSED,
            ROOM_TRANSITION_START,
            ROOM_TRANSITION_BEFORE_LEAVE_ROOM,
            ROOM_TRANSITION_JOIN_ROOM_REQUEST,
            ROOM_TRANSITION_LEAVE_ROOM_REQUEST,
            ROOM_TRANSITION_SUCCESS,
            ROOM_TRANSITION_JOIN_ROOM_ERROR,
            ROOM_TRANSITION_CREATE_ROOM_ERROR,
            ROOM_TRANSITION_PACKET_REQUEST_START,
            ROOM_TRANSITION_PACKET_RESPONSE_END,
            ROOM_TRANSITION_END,
            SCENE_LOADING_START,
            SCENE_LOADING_FINISH,
            STATE_MACHINE_ON_BEGIN,
            STATE_MACHINE_ON_BEFORE_END,
            STATE_MACHINE_ON_END,
            CONTROLLER_INIT,
            CONTROLLER_RELEASE,
            STATE_CHANGE,
            STREAMING_END,
            STREAMING_PLAY,
            STREAMING_PAUSE,
            STREAMING_RESUME,
            STREAMING_RESTART,
            USER_VARIABLE_SEND,
            USER_VARIABLE_RECEIVE,
            ROOM_VARIABLE_SEND,
            ROOM_VARIABLE_RECEIVE,
            SFS_EVENT_LISTENER,
            PARSE_DATA,
            IMAGE_UPLOAD,
            SCREEN_TYPE_CHANGE,
            TEST,

        }

        private static class LogStrings
        {
            public static Dictionary<LogType, string> LogTable = new Dictionary<LogType, string>()
            {
                { LogType.DEFAULT, "{0}" },
                { LogType.EXCEPTION, "EXCEPTION!\n{0}" },
                { LogType.PACKET_SEND_TO_ROOM, "[To the room] {0} sent!" },
                { LogType.PACKET_SEND_TO_ROOM_WAITING_RESPONSE, "[To the room] {0} sent! <client is waiting for response>" },
                { LogType.PACKET_SEND_TO_ZONE, "[To the zone] {0} sent!" },
                { LogType.PACKET_SEND_TO_ZONE_WAITING_RESPONSE, "[To the zone] {0} sent! <client is waiting for response>" },
                { LogType.PACKET_RECEIVE, "{0} received!" },
                { LogType.PACKET_RECEIVE_TIMEOUT, "TIMEOUT occurred while waiting for server response! <Timeout second : 10s>" },
                { LogType.UI_OPEN, "{0} UI opened!" },
                { LogType.UI_CLOSE, "{0} UI closed!" },
                { LogType.BUTTON_PRESSED, "{0} button pressed!" },
                { LogType.ROOM_TRANSITION_START, "-------------------ROOM TRANSITION Started-------------------" }, 
                { LogType.ROOM_TRANSITION_BEFORE_LEAVE_ROOM, "ROOM TRANSITION BEFORE LEAVE ROOM!\t\t<leave {0}>" }, 
                { LogType.ROOM_TRANSITION_JOIN_ROOM_REQUEST, "ROOM TRANSITION JOIN ROOM requested!\t\t<join {0}>" },
                { LogType.ROOM_TRANSITION_LEAVE_ROOM_REQUEST, "ROOM TRANSITION LEAVE ROOM requested!\t\t<leave {0}>" },
                { LogType.ROOM_TRANSITION_SUCCESS, "ROOM TRANSITION succeed!\t\t<join {0}>" },
                { LogType.ROOM_TRANSITION_JOIN_ROOM_ERROR, "ROOM TRANSITION JOIN ROOM failed!" },
                { LogType.ROOM_TRANSITION_CREATE_ROOM_ERROR, "ROOM TRANSITION CREATE ROOM failed!" },
                { LogType.ROOM_TRANSITION_PACKET_REQUEST_START, "ROOM TRANSITION request packets!\t\t<join {0}>" },
                { LogType.ROOM_TRANSITION_PACKET_RESPONSE_END, "ROOM TRANSITION all packets received!\t\t<join {0}>" },
                { LogType.ROOM_TRANSITION_END, "-------------------ROOM TRANSITION Ended!-------------------" },
                { LogType.SCENE_LOADING_START, "{0} SCENE LOAD is started!" },
                { LogType.SCENE_LOADING_FINISH, "{0} SCENE LOAD is finished!" },
                { LogType.STATE_MACHINE_ON_BEGIN, "{0} state is ON BEGIN!" },
                { LogType.STATE_MACHINE_ON_BEFORE_END, "{0} state is ON BEFORE END!" },
                { LogType.STATE_MACHINE_ON_END, "{0} state is ON END!" },
                { LogType.CONTROLLER_INIT, "{0} is INITIALIZED!" },
                { LogType.CONTROLLER_RELEASE, "{0} is RELEASED!"},
                { LogType.STATE_CHANGE, "{0} STATE CHANGED!\t\t<{1} -> {2}>" },
                { LogType.STREAMING_END, "STREAMING ended!\t\t<video title : {0}>"},
                { LogType.STREAMING_PLAY, "STREAMING started!\t\t<video title : {0}>, <is paused : {1}>, <passed time : {2}>"},
                { LogType.STREAMING_PAUSE, "STREAMING paused!\t\t<video title : {0}>"},
                { LogType.STREAMING_RESUME, "STREAMING resumed!\t\t<video title : {0}>"},
                { LogType.STREAMING_RESTART, "STREAMING restarted!\t\t<video title : {0}>"},
                { LogType.SFS_EVENT_LISTENER, "SFS EVENT LISTENER {0} triggered!"},
                { LogType.PARSE_DATA, "PARSE DATA with key {0}!"},
                { LogType.IMAGE_UPLOAD, "{0} IMAGE UPLOAD is completed!"},
                { LogType.SCREEN_TYPE_CHANGE, "SCREEN TYPE CHANGED into {0}!"},
                { LogType.TEST, "*************************** TEST LOGS! *************************** {0}"},

            };

            public static void CheckLogTableValidity()
            {
                try
                {
                    string forTest = string.Empty;
                    if (LogTable.ContainsKey(LogType.TEST))
                    {
                        UnityEngine.Debug.Log("LogFileWriter's LogTable validity check completed!");
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e.ToString());
                }
            }
        }

        private class FileWriter
        {
            private string logFilePath = string.Empty;
            private string logFileName = string.Empty;
            private string FilePath = string.Empty;

            public FileWriter()
            {
                LogStrings.CheckLogTableValidity();
#if UNITY_EDITOR
                logFilePath = $"{Application.persistentDataPath}\\UnityEditorLogFiles";
                System.IO.Directory.CreateDirectory(logFilePath);
                logFileName = $"{DateTime.Now.Year:D4}{DateTime.Now.Month:D2}{DateTime.Now.Day:D2}_Log";
                FilePath = $"{logFilePath}\\{logFileName}.txt";
                WriteInitialText();
#elif TEST_BUILD
                logFilePath = $"{Application.persistentDataPath}/UnityEditorLogFiles";
                System.IO.Directory.CreateDirectory(logFilePath);
                logFileName = $"{DateTime.Now.Year:D4}{DateTime.Now.Month:D2}{DateTime.Now.Day:D2}_Log";
                FilePath = $"{logFilePath}/{logFileName}.txt";
                WriteInitialText();
#endif
            }

            internal void WriteText(string _message)
            {
                using (FileStream fs = new FileStream(FilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    string outputMessage = $"{DateTime.Now.ToLongTimeString()}.{DateTime.Now.Millisecond:D6}   {_message}\n";
                    for (int i = 0; i < outputMessage.Length; i++)
                    {
                        fs.WriteByte((byte)outputMessage[i]);
                    }
                }
            }

            private void WriteInitialText()
            {
                using (FileStream fs = new FileStream(FilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    string outputMessage = "\n\n-------------------------------<SYSTEM INITIATED>-------------------------------\n";
                    for (int i = 0; i < outputMessage.Length; i++)
                    {
                        fs.WriteByte((byte)outputMessage[i]);
                    }
                }
            }
        }

        private class LogInfo
        {
            private string LogFormat = "USERNAME : <{0}>   MIDX : <{1}>     {2}";
            public string LogMessage { get; set; }
            public LogType LogType { get; set; }

            /// <summary>
            /// _message의 포맷은 _type에 따라 달라짐.
            /// LogType과 대응되는 로그 메시지가 LogStrings.LogTable에 저장되어있음. 
            /// </summary>
            /// <param name="_message"></param>
            /// <param name="_type"></param>
            /// <param name="_level"></param>
            public void SetLog(string _message, LogType _type)
            {
                LogType = _type;
                SetLogMessage(_message);
            }
            public void SetLog(string[] _messages, LogType _type)
            {
                LogType = _type;
                SetLogMessage(_messages);
            }
            public void SetLog(List<string> _messages, LogType _type)
            {
                LogType = _type;
                SetLogMessage(_messages);
            }

            private void SetLogMessage(string _message)
            {
                string result = "";
                try
                {
                    if (TryGetLogFormat(out result))
                    {
                        string logMessage = string.Format($"{result}", _message);
                        LogMessage = string.Format(LogFormat, GameInstance.Instance.UserName, GameInstance.Instance.MIDX, logMessage);
                    }
                    else
                    {
                        LogFileWriter.LogError("Log format not found!", LogType.DEFAULT);
                    }
                }
                catch (Exception e)
                {
                    LogFileWriter.Log(e.ToString(), LogType.EXCEPTION);
                }
            }

            private void SetLogMessage(string[] _messages)
            {
                string result = "";
                try
                {
                    if (TryGetLogFormat(out result))
                    {
                        string logMessage = string.Format($"{result}", ConvertIntoList(_messages));
                        LogMessage = string.Format(LogFormat, GameInstance.Instance.UserName, GameInstance.Instance.MIDX, logMessage);

                    }
                    else
                    {
                        LogFileWriter.LogError("Log format not found!", LogType.DEFAULT);
                    }
                }
                catch (Exception e)
                {
                    LogFileWriter.Log(e.ToString(), LogType.EXCEPTION);
                }
            }

            private void SetLogMessage(List<string> _messages)
            {
                string result = "";
                try
                {
                    if (TryGetLogFormat(out result))
                    {
                        string logMessage = string.Format($"{result}", ConvertIntoList(_messages));
                        LogMessage = string.Format(LogFormat, GameInstance.Instance.UserName, GameInstance.Instance.MIDX, logMessage);
                    }
                    else
                    {
                        LogFileWriter.LogError("Log format not found!", LogType.DEFAULT);
                    }
                }
                catch (Exception e)
                {
                    LogFileWriter.Log(e.ToString(), LogType.EXCEPTION);
                }
            }

            private bool TryGetLogFormat(out string _format)
            {
                if (LogStrings.LogTable.TryGetValue(LogType, out _format))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private T[] ConvertIntoList<T>(T[] _list)
            {
                T[] objectlist = new T[_list.Length];
                _list.CopyTo(objectlist, 0);
                return objectlist;
            }

            private T[] ConvertIntoList<T>(List<T> _list)
            {
                T[] objectlist = new T[_list.Count];
                _list.CopyTo(objectlist, 0);
                return objectlist;
            }
        }

        private static LogInfo logInfo = new LogInfo();
        private static FileWriter logFileWriter = new FileWriter();

        /// <summary>
        /// 추후 로그를 찍으면 안 되는 상황이 생길 경우를 핸들링하기 위해 만들어놓은 메소드
        /// </summary>
        /// <returns></returns>
        private static bool CanPrintLog()
        {
            return true;
        }

        #region Print Log Message
        [Conditional("UNITY_EDITOR")]
        [Conditional("TEST_BUILD")]
        public static void Log(string _message, LogType _type = LogType.DEFAULT)
        {
            logInfo.SetLog(_message, _type);
            Print();
            PrintEditorLog();
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("TEST_BUILD")]
        public static void Log(string[] _messages, LogType _type = LogType.DEFAULT)
        {
            logInfo.SetLog(_messages, _type);
            Print();
            PrintEditorLog();
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("TEST_BUILD")]
        public static void Log(List<string> _messages, LogType _type = LogType.DEFAULT)
        {
            logInfo.SetLog(_messages, _type);
            Print();
            PrintEditorLog();
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("TEST_BUILD")]
        public static void LogWarning(string _message, LogType _type = LogType.DEFAULT)
        {
            logInfo.SetLog(_message, _type);
            Print();
            PrintEditorLogWarning();
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("TEST_BUILD")]
        public static void LogWarning(string[] _messages, LogType _type = LogType.DEFAULT)
        {
            logInfo.SetLog(_messages, _type);
            Print();
            PrintEditorLogWarning();
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("TEST_BUILD")]
        public static void LogWarning(List<string> _messages, LogType _type = LogType.DEFAULT)
        {
            logInfo.SetLog(_messages, _type);
            Print();
            PrintEditorLogWarning();
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("TEST_BUILD")]
        public static void LogError(string _message, LogType _type = LogType.DEFAULT)
        {
            logInfo.SetLog(_message, _type);
            Print();
            PrintEditorLogError();
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("TEST_BUILD")]
        public static void LogError(string[] _messages, LogType _type = LogType.DEFAULT)
        {
            logInfo.SetLog(_messages, _type);
            Print();
            PrintEditorLogError();
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("TEST_BUILD")]
        public static void LogError(List<string> _messages, LogType _type = LogType.DEFAULT)
        {
            logInfo.SetLog(_messages, _type);
            Print();
            PrintEditorLogError();
        }

        private static void Print()
        {
            if (!CanPrintLog())
            {
                return;
            }
            logFileWriter.WriteText(logInfo.LogMessage);    // export as txt file!
        }

        [Conditional("EDITOR_LOG_ENABLED")]
        private static void PrintEditorLog()
        {
            UnityEngine.Debug.Log(logInfo.LogMessage);         // for editor console log!
        }

        [Conditional("EDITOR_LOG_ENABLED")]
        private static void PrintEditorLogWarning()
        {
            UnityEngine.Debug.LogWarning(logInfo.LogMessage);         // for editor console log!
        }

        [Conditional("EDITOR_LOG_ENABLED")]
        private static void PrintEditorLogError()
        {
            UnityEngine.Debug.LogError(logInfo.LogMessage);         // for editor console log!
        }
        #endregion
    }
}
