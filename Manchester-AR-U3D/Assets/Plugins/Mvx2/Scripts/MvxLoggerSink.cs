using UnityEngine;
using System.Text;

namespace MVXUnity
{
    [AddComponentMenu("Mvx2/Mvx Logger Sink")]
    public class MvxLoggerSink : MonoBehaviour
    {
        [SerializeField] public bool outputTime = false;

        public class UnityLoggerSink : MVCommon.NetLoggerSink
        {
            private MvxLoggerSink m_mvxLoggerSink;

            public UnityLoggerSink(MvxLoggerSink mvxLoggerSink)
            {
                m_mvxLoggerSink = mvxLoggerSink;
            }

            protected override void HandleLogEntry(MVCommon.LogEntry logEntry)
            {
                switch (logEntry.Level)
                {
                    case MVCommon.LogLevel.LL_CRITICAL:
                    case MVCommon.LogLevel.LL_ERROR:
                        Debug.LogErrorFormat("Mvx2: {0}", FormatLogEntry(logEntry));
                        break;
                    case MVCommon.LogLevel.LL_WARNING:
                        Debug.LogWarningFormat("Mvx2: {0}", FormatLogEntry(logEntry));
                        break;
                    case MVCommon.LogLevel.LL_DEBUG:
                    case MVCommon.LogLevel.LL_INFO:
                    case MVCommon.LogLevel.LL_VERBOSE:
                        Debug.LogFormat("Mvx2: {0}", FormatLogEntry(logEntry));
                        break;
                    default:
                        Debug.LogFormat("Mvx2: Unknown log level '{0}'", logEntry.Level.ToString());
                        break;
                }
            }

            private string FormatLogEntry(MVCommon.LogEntry logEntry)
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (m_mvxLoggerSink.outputTime)
                    stringBuilder.Append(TimestampToString(logEntry.Timestamp, true).NetString).Append("|");
                stringBuilder.Append(logEntry.ThreadID).Append("|");
                stringBuilder.Append(LogLevelToString(logEntry.Level, true)).Append("|");
                stringBuilder.Append(logEntry.Tag).Append("|");
                stringBuilder.Append(logEntry.Message);
                return stringBuilder.ToString();
            }
        }

        private static MvxLoggerSink s_instance = null;
        
        private UnityLoggerSink m_loggerSink = null;
        private MVCommon.Logger m_logger = null;

        public void Awake()
        {
            if (s_instance != null)
            {
              Debug.LogWarning("Mvx2: Only single instance of logger sink allowed");
              Destroy(this);
            }
            
            s_instance = this;
        }
        
        public void OnDestroy()
        {
          if (s_instance == this)
            s_instance = null;
        }

        public void OnEnable()
        {
            m_logger = new MVCommon.Logger();
            m_loggerSink = new UnityLoggerSink(this);
            m_logger.AddLoggerSink(m_loggerSink);
            Mvx2API.Utils.MVXLoggerInstance = m_logger;
        }

        public void OnDisable()
        {
            Mvx2API.Utils.MVXLoggerInstance = null;
            m_logger = null;
            m_loggerSink = null;
        }
    }
}
