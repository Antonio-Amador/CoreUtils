using System;

namespace Logging
{
    public class LogsInfo
    {
        public int Id { get; set; }
        public string IDCorrelation { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Message { get; set; }

        public string CodeLocation { get; set; }

        public string ApplicationIdentity { get; set; }
        public short Level { get; set; }
        public string StackTrace { get; set; }
        public string Source { get; set; }

        public LogsInfo()
        {
            this.CreatedDate = DateTime.Now;
            var d = string.Empty;
        }
    }

    public class Settings
    {
        private static int _LogLevel = 2;

        public static int LogLevel
        {
            get
            {

                try
                {
                    if (_LogLevel == -1)
                    {

                        //_LogLevel = Convert.ToInt32(GetValue(string.Format("{0}.LogLevel", ApplicationIdentity)));
                    }
                }
                catch (Exception)
                {
                    _LogLevel = 4; //Valor pré-defenido
                }
                return _LogLevel;

            }
        }

        public static string ApplicationIdentity
        {
            get
            {
                return Environment.MachineName;
            }
        }


    }
}
