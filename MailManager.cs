using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Runtime.InteropServices;

namespace ceqalib
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class MailManager
    {
        public string Address { get; set; }
        public string Subject { get; set; }
        public List<string> Cc { get; set; }
        public List<string> Bcc { get; set; }
        public string MessageBody { get; set; }

        public List<string> AttechmentList;

        public bool IsOutlookInstalled()
        {
            const string appName = "Outlook.Application";
            Type officeType = Type.GetTypeFromProgID(appName);
            return officeType != null;
        }

        public MailManager()
        {
            AttechmentList = new List<string>();
            Cc = new List<string>();
            Bcc = new List<string>();
        }

        public void AddAttachmentFile(string fileName)
        {
            AttechmentList.Add(fileName);
        }

        public void AttachFolder(string folderPath)
        {
            AttechmentList.AddRange(Directory.GetFiles(folderPath));
        }

        public void AddCc(string cc)
        {
            if (!Cc.Contains(cc))
                Cc.Add(cc);
        }

        public void AddBcc(string bcc)
        {
            if (!Bcc.Contains(bcc))
                Bcc.Add(bcc);
        }

        public bool SendMail(bool? outLook = null)
        {
            bool result = false;
            if (outLook == null)
            {
                result = SendMailViaOutLook();
                if (!result)
                    result = SendMailViaSmtpServer();
            }
            else if (outLook == true)
            {
                try
                {
                    result = SendMailViaOutLook();
                }
                catch
                {
                    result = false;
                }
            }
            else
            {
                try
                {
                    result = SendMailViaSmtpServer();
                }
                catch
                {
                    result = false;
                }

            }
            return result;
        }

        public bool SendMailViaOutLook(bool visible=false)
        {
            if (!IsOutlookInstalled()) return false;

            Application app = new Application();
            MailItem mailItem = app.CreateItem(OlItemType.olMailItem);
            mailItem.Subject = Subject;
            mailItem.To = Address;
            mailItem.Body = MessageBody;
            string cc = Cc.Aggregate("", (current, c) => current + (c + ";"));
            if (!string.IsNullOrEmpty(cc))
                mailItem.CC = cc.Remove(cc.Length - 1);
            string bcc = Bcc.Aggregate("", (cureent, c) => cureent + (c + ";"));
            if (!string.IsNullOrEmpty(bcc))
                mailItem.BCC = bcc.Remove(bcc.Length - 1);
            foreach (string file in AttechmentList)
            {
                mailItem.Attachments.Add(file);
            }
            mailItem.Display(visible);
            mailItem.Send();

            return true;
        }

        public string AddressFrom { get; set; }
        public const string DefaultSmtpHost = "mailhub-vip.ny.ssmb.com";

        public bool SendMailViaSmtpServer(string host = DefaultSmtpHost)
        {
            SmtpClient client = new SmtpClient(host);
            MailAddress from = new MailAddress(AddressFrom);

            MailAddress to = new MailAddress(Address);

            MailMessage message = new MailMessage(from, to);
            message.Body = MessageBody;
            // Include some non-ASCII characters in body and subject.       
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.Subject = Subject;
            message.SubjectEncoding = System.Text.Encoding.UTF8;
            foreach (var cc in Cc)
            {
                message.CC.Add(cc);
            }
            foreach (var bcc in Bcc)
            {
                message.Bcc.Add(bcc);
            }

            foreach (string file in AttechmentList)
            {
                System.Net.Mail.Attachment ac = new System.Net.Mail.Attachment(file, MediaTypeNames.Application.Octet);
                message.Attachments.Add(ac);
            }

            client.Send(message);
            // Clean up.
            message.Dispose();

            return true;
        }

        public bool SendInstantMail()
        {
            if (!IsOutlookInstalled()) return false;

            Application app = new Application();
            MailItem mailItem = app.CreateItem(OlItemType.olMailItem);
            mailItem.Subject = Subject;
            mailItem.To = Address;
            string cc = Cc.Aggregate("", (current, c) => current + (c + ";"));
            if (!string.IsNullOrEmpty(cc))
                mailItem.CC = cc.Remove(cc.Length - 1);
            mailItem.Body = MessageBody;
            foreach (string file in AttechmentList)
            {
                mailItem.Attachments.Add(file);
            }
            mailItem.Display(true);
            return true;
        }
    }
}
