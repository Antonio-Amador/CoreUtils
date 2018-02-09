using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Criar construtos que passee i ID_Correlatio ex. Pistolas, Excel epassar source como sendo o ambiente Testes PRD etc.

//Codelocation tem informação acerca


namespace Logging
{
    public class LogsInfoRepository
    {

        public enum LogLevel : int
        {
            None = 0,
            Error = 1,
            Info = 2,
            Warn = 3,
            Debug = 4
        }

        private enum LogDBType : int
        {
            Normal = 0,
            Error = 1
        }
        private static string KSP_INSERT = @"INSERT INTO dbo.Logging(IDCorrelation, CreatedDate, Message, CodeLocation
                    , ApplicationIdentity, Level, StackTrace, Source)
                    SELECT @IDCorrelation, GETDATE(), @Message, @CodeLocation
                    , @ApplicationIdentity, @Level, @StackTrace, @Source";

        private static string connectionString = ConfigurationManager.ConnectionStrings["ArquivoDigitalConnString"].ToString();
        public static void Warn(string Message)
        {
            if ((int)LogLevel.Warn <= Settings.LogLevel)
            {
                InsertNormal(Message, LogLevel.Warn, null);
            }
        }

        public static void Warn(string Message, string ID_Correlation)
        {
            if ((int)LogLevel.Warn <= Settings.LogLevel)
            {
                InsertNormal(Message, LogLevel.Warn, ID_Correlation);
            }
        }

        public static void Info(string Message)
        {
            if ((int)LogLevel.Info <= Settings.LogLevel)
            {
                InsertNormal(Message, LogLevel.Info, null);
            }
        }

        public static void Info(string Message, string ID_Correlation)
        {
            if ((int)LogLevel.Info <= Settings.LogLevel)
            {
                InsertNormal(Message, LogLevel.Info, ID_Correlation);
            }
        }

        public static void Debug(string Message)
        {
            if ((int)LogLevel.Debug <= Settings.LogLevel)
            {
                InsertNormal(Message, LogLevel.Debug, null);
            }
        }

        public static void Debug(string Message, string ID_Correlation)
        {
            if ((int)LogLevel.Debug <= Settings.LogLevel)
            {
                InsertNormal(Message, LogLevel.Debug, ID_Correlation);
            }
        }

        public static void Error(Exception ex)
        {
            if ((int)LogLevel.Error <= Settings.LogLevel)
            {
                InsertError(ex, LogLevel.Error, null);
                if (ex.InnerException != null)
                    Error(ex.InnerException);
            }
        }

        public static void Error(Exception ex, string ID_Correlation)
        {
            if ((int)LogLevel.Error <= Settings.LogLevel)
            {
                InsertError(ex, LogLevel.Error, ID_Correlation);
                if (ex.InnerException != null)
                    Error(ex.InnerException, ID_Correlation);
            }
        }

        private static void InsertError(Exception ex, LogLevel Level, string ID_Correlation)
        {
            try
            {
                var log = new LogsInfo();
                log.IDCorrelation = ID_Correlation;
                log.CreatedDate = DateTime.Now;
                log.Message = ex.Message;
                log.CodeLocation = GetCurrentStackTrace();
                log.ApplicationIdentity = string.Format("{0}.{1}.{2}", Settings.ApplicationIdentity, Process.GetCurrentProcess().ProcessName, System.Environment.MachineName);
                log.Level = (byte)Level;
                log.StackTrace = ex.StackTrace;
                log.Source = ex.Source;

                Insert(log);
                
            }
            catch (Exception exx)
            {
                Console.WriteLine(exx);
                //InsertErrorToFile(exx, Level, ID_Correlation);
                //InsertErrorToFile(ex, Level, ID_Correlation);
            }
        }

        private static void InsertNormal(string Message, LogLevel Level, string ID_Correlation)
        {
            try
            {
                var log = new LogsInfo();
                log.IDCorrelation = ID_Correlation;
                log.Message = Message;
                log.CodeLocation = GetCurrentStackTrace();
                log.ApplicationIdentity = string.Format("{0}.{1}.{2}", Settings.ApplicationIdentity
                    , Process.GetCurrentProcess().ProcessName, System.Environment.MachineName);
                log.Level = (byte)Level;
                Insert(log);
            }
            catch (Exception exx)
            {
                //InsertErrorToFile(exx, Level, ID_Correlation);
                //InsertNormalToFile(Message, Level, ID_Correlation);
            }
        }

        private static void Insert(LogsInfo entity)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(KSP_INSERT, connection))
                {
                    object[] parameters = {
                    //new SqlParameter("object_name", SqlHelper.GetDataValue(entity.object_name)),
                    new SqlParameter("@IDCorrelation", GetDataValue(entity.IDCorrelation))
                    , new SqlParameter("@Message", GetDataValue(Left(entity.Message)))
                    , new SqlParameter("@CodeLocation", GetDataValue(Left(entity.CodeLocation)))
                    , new SqlParameter("@ApplicationIdentity", GetDataValue(Left(entity.ApplicationIdentity)))
                    , new SqlParameter("@Level", GetDataValue(entity.Level))
                    , new SqlParameter("@StackTrace", GetDataValue(Left(entity.StackTrace)))
                    , new SqlParameter("@Source", GetDataValue(Left(entity.Source)))
                    };
                    command.Parameters.AddRange(parameters);
                    command.ExecuteNonQuery();
                }
            }
        }
        private static string Left(string value)
        {
            if (value == null)
                return null;

            return value.Substring(0, Math.Min(value.Length - 1, 8000));
        }
        private static string GetCurrentStackTrace()
        {
            StringBuilder sb = new StringBuilder();
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            try
            {
                foreach (System.Diagnostics.StackFrame sf in st.GetFrames())
                {
                    System.Reflection.AssemblyCompanyAttribute aca = (System.Reflection.AssemblyCompanyAttribute)sf.GetMethod().DeclaringType.Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyCompanyAttribute), false)[0];
                    if (!aca.Company.Equals("Sofrapa"))
                        break;

                    if (!sf.GetMethod().DeclaringType.FullName.Equals("Logging.LogsInfoRepository"))
                        sb.Append(string.Format("{0}.{1}.{2} « ", sf.GetMethod().DeclaringType.Namespace, sf.GetMethod().DeclaringType.Name, sf.GetMethod().Name));

                }
            }
            catch (Exception)
            {
            }
            return sb.ToString(0, sb.Length - 3);
        }
        internal static object GetDataValue(object value)
        {
            if (value == null)
            {
                return DBNull.Value;
            }

            return value;
        }
    }
}
