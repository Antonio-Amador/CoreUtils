﻿using CLFCore.AuxModels;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace CLFCore.Utilis
{
    public class Utils
    {

        #region Validation_Passwords
        public Boolean CheckNIF(string nif)
        {

            string firstDigit = nif.Substring(0, 1);

            if (nif.Length != 9 || (firstDigit == "0") || (firstDigit == "3") || (firstDigit == "4") || (firstDigit == "7") || (firstDigit == "8"))
            {
                return false;
            }

            return AlgoritmoValidacao(nif);
        }

        public Boolean CheckBI(string nbi, string lastDigit)
        {
            string BIformated;

            if ((nbi.Length < 7) || (nbi.Length > 8))
            {
                return false;
            }

            if (lastDigit.Length != 1)
            {
                return false;
            }

            if (nbi.Length == 8)
            {
                BIformated = nbi + lastDigit;
            }
            else
            {
                BIformated = '0' + nbi + lastDigit;
            }

            return AlgoritmoValidacao(BIformated);
        }

        private Boolean AlgoritmoValidacao(string numFormated)
        {

            int ctl = 0;
            int val = 0;

            for (int i = 0; i < 8; i++)
            {
                val += Convert.ToInt32(numFormated.Substring(i, 1)) * (9 - i);
            }

            if ((val % 11) != 0)
            {
                ctl = (11 - val % 11) % 10;
            }

            if (ctl == Convert.ToInt32(numFormated.Substring(8, 1)))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public string GerarSenhas()
        {
            int Tamanho = 8;  // Numero de digitos da senha
            string senha = string.Empty;
            for (int i = 0; i < Tamanho; i++)
            {
                Random random = new Random();
                int codigo = Convert.ToInt32(random.Next(48, 122).ToString());

                if ((codigo >= 48 && codigo <= 57) || (codigo >= 97 && codigo <= 122))
                {
                    string _char = ((char)codigo).ToString();
                    if (!senha.Contains(_char))
                    {
                        senha += _char;
                    }
                    else
                    {
                        i--;
                    }
                }
                else
                {
                    i--;
                }

            }
            senha = senha.Substring(4) + senha.Substring(0, 4);
            return senha;
        }

        public string GerarLogin(string app, string id)
        {
            string temp = "0";

            while ((temp.Length + app.Length + id.Length) < 6)
            {
                temp = temp + "0";
            }

            return app + temp + id;
        }

        #endregion

        #region String_Format
        public DateTime GetDateFromString(string date)
        {
            DateTime d = new DateTime();

            try
            {
                d = Convert.ToDateTime(date);
            }
            catch (Exception)
            {
                //ignore
            }

            return d;
        }

        public string GetStringToday(string format)
        {
            return DateTime.Now.ToString(format);
        }

        public string FormatDate(string date, string format)
        {
            DateTime dt = GetDateFromString(date);
            //yyyy-MM-dd HH:mm:ss
            return dt.ToString(format);
        }

        public string FormatDateWithCulture(string date, string format)
        {

            IFormatProvider culture = new System.Globalization.CultureInfo("pt-PT", true);
            DateTime dt2 = DateTime.Parse(date, culture, System.Globalization.DateTimeStyles.AssumeLocal);
            return dt2.ToString();
        }

        #endregion

        #region Mails
        private SmtpClient CreateSmtpClient(string SmtpServer)
        {

            SmtpClient client = new SmtpClient(SmtpServer);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = true;
            //client.UseDefaultCredentials = false;
            //client.Credentials = new NetworkCredential("diodev", "Clf*123");               
            //client.EnableSsl = true;
            return client;

        }

        private MailMessage CreateMailMessage(string EmailAddressFrom, string EmailAddressTo)
        {
            MailMessage message = new MailMessage(EmailAddressFrom, EmailAddressTo);
            message.IsBodyHtml = true;
            return message;
        }

        public void SendEmail(List<EmailModel> emailModelArray)
        {
            string t = string.Empty;
            foreach (EmailModel m in emailModelArray)
            {
                try
                {
                    MemoryStream ms = null;
                    Attachment attach = null;

                    if (m.attachByteArray != null)
                    {
                        ms = new MemoryStream(m.attachByteArray);
                        ms.Seek(0, SeekOrigin.Begin);
                        attach = new Attachment(ms, m.documentName);
                    }

                    SmtpClient client = null;
                    try
                    {
                        client = CreateSmtpClient(m.smtpServer);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("1 - " + e.Message);
                    }

                    MailMessage message = null;
                    try
                    {
                        message = CreateMailMessage(m.emailAddressFrom, m.emailAddressTo);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("2 - " + e.Message);
                    }

                    message.Subject = m.emailSubject;
                    message.Body = m.emailBody;

                    if (m.attachByteArray != null)
                    {
                        message.Attachments.Add(attach);
                    }
                    client.Send(message);
                    CLFCore.Logs.LogsInfoRepository.Info("To: " + m.emailAddressFrom + " Subject: " + m.emailSubject + " Message: " + m.emailBody, "Util");
                }
                catch (Exception e)
                {
                    CLFCore.Logs.LogsInfoRepository.Error(e, "Util");
                    throw new Exception("3 - " + e.InnerException);
                }
            }
        }
#endregion

#region Files

 

        public byte[] GetByteArrayFromFile(string fileName)
        {
            byte[] buff = null;
            try
            {
                FileStream fs = new FileStream(fileName,
                                               FileMode.Open,
                                               FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                long numBytes = new FileInfo(fileName).Length;
                buff = br.ReadBytes((int)numBytes);
            }
            catch (Exception e)
            {
                CLFCore.Logs.LogsInfoRepository.Error(e, "Util");
            }
            return buff;
        }

        private string createFile(string sTempDir, string extension, byte[] content)
        {

            TempFileCollection oTempFiles = new TempFileCollection(sTempDir, true);
            string sFilePathName = oTempFiles.AddExtension(extension, true);
            string sFileName = Path.GetFileName(sFilePathName);
            string sFileUrl = string.Format("{0}{1}", sTempDir, sFileName);

            FileStream fs = new FileStream(sFilePathName, FileMode.Create, FileAccess.ReadWrite);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(content);
            bw.Close();

            return sFileName;

        }

#endregion

#region Acessos

        public string GetGroups(string UserName, string LDAPQueryString)
        {

            //1.	GDL_AQD_PCL - Portal do Cliente – Funcionalidades para gestão do portal:
            //2.	GDL_AQD_GED - Gestão Documental – Substituto do Documentum Webtop:
            //3.	GDL_AQD_COF - Configuração – Funcionalidades para configuração da plataforma (gestão de acessos, etc.):
            //4.    GDL_AQD_ADM - Administração – Os utilizadores pertencentes a este grupo terão acesso a todos os menus (este grupo não será gerido pela plataforma

            DirectoryEntry de = new DirectoryEntry();
            List<string> userInfo = new List<string>();

            string groupNames = "";

            de.Path = LDAPQueryString;

            de.AuthenticationType = AuthenticationTypes.Secure;

            DirectorySearcher deSearch = new DirectorySearcher();

            deSearch.SearchRoot = de;
            deSearch.Filter = "(&(objectClass=user) (samAccountName=" + UserName + "))";
            //deSearch.Filter = "(&(objectClass=user) (displayName=" + UserName + "))";

            SearchResult dresult = deSearch.FindOne();

            if (dresult != null)
            {
                DirectoryEntry deUser = new DirectoryEntry(dresult.Path);

                string dn = null;
                int equalsIndex = 0, commaIndex = 0;
                var memberOf = deUser.Properties["memberOf"];

                System.DirectoryServices.PropertyCollection pc = deUser.Properties;
                foreach (PropertyValueCollection col in pc)
                {
                    userInfo.Add(col.PropertyName + " : " + col.Value);

                }

                foreach (var result2 in memberOf)
                {

                    dn = result2.ToString();
                    equalsIndex = dn.IndexOf("=", 1);
                    commaIndex = dn.IndexOf(",", 1);
                    if (-1 == equalsIndex)
                    {
                        return null;
                    }

                    if (dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1) != null && dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1).StartsWith("GDL_AQD_"))
                    {
                        groupNames = groupNames + dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1) + "_";
                    }
                }

                deUser.Close();
            }
            return groupNames;
        }

        public List<string> GetArrayGroups(string UserName, string LDAPQueryString)
        {

            //1.	GDL_AQD_PCL - Portal do Cliente – Funcionalidades para gestão do portal:
            //2.	GDL_AQD_GED - Gestão Documental – Substituto do Documentum Webtop:
            //3.	GDL_AQD_COF - Configuração – Funcionalidades para configuração da plataforma (gestão de acessos, etc.):
            //4.    GDL_AQD_ADM - Administração – Os utilizadores pertencentes a este grupo terão acesso a todos os menus (este grupo não será gerido pela plataforma

            DirectoryEntry de = new DirectoryEntry();
            List<string> userInfo = new List<string>();

            List<string> groupNames = new List<string>();

            de.Path = LDAPQueryString;

            de.AuthenticationType = AuthenticationTypes.Secure;

            DirectorySearcher deSearch = new DirectorySearcher();

            deSearch.SearchRoot = de;
            //deSearch.Filter = "(&(objectClass=user) (samAccountName=" + UserLogin + "))";
            deSearch.Filter = "(&(objectClass=user) (displayName=" + UserName + "))";

            SearchResult dresult = deSearch.FindOne();

            if (dresult != null)
            {
                DirectoryEntry deUser = new DirectoryEntry(dresult.Path);

                string dn = null;
                int equalsIndex = 0, commaIndex = 0;
                var memberOf = deUser.Properties["memberOf"];

                System.DirectoryServices.PropertyCollection pc = deUser.Properties;
                foreach (PropertyValueCollection col in pc)
                {
                    userInfo.Add(col.PropertyName + " : " + col.Value);

                }

                foreach (var result2 in memberOf)
                {

                    dn = result2.ToString();
                    equalsIndex = dn.IndexOf("=", 1);
                    commaIndex = dn.IndexOf(",", 1);
                    if (-1 == equalsIndex)
                    {
                        return null;
                    }

                    if (dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1) != null && dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1).StartsWith("GDL_AQD_"))
                    {
                        groupNames.Add(dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1));
                    }
                }

                deUser.Close();
            }
            return groupNames;
        }
#endregion
    }
}
