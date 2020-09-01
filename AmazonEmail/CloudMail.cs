using Amazon.SimpleEmail.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;


namespace CloudEmail
{
    public class CloudMail
    {
        //From E-mail address must be verified through Amazon
        /// <param name="AWSAccessKey">public key associated with our Amazon Account</param>
        /// <param name="AWSSecretKey">private key associated with our Amazon Account</param>
        /// <param name="ToEmail">Who do you want to send the E-mail to, seperate multiple addresses with a comma</param>
        /// <param name="FromEmail">Who is the e-mail from, this must be a verified e-mail address through Amazon</param>
        /// <param name="Subject">Subject of e-mail</param>
        /// <param name="Content">Text for e-mail</param>
        public void SendEmail(string AWSAccessKey, string AWSSecretKey, string ToEmail, string FromEmail, string Subject, string Content)
        {
            Amazon.SimpleEmail.AmazonSimpleEmailServiceClient client = new Amazon.SimpleEmail.AmazonSimpleEmailServiceClient(AWSAccessKey, AWSSecretKey, Amazon.RegionEndpoint.USEast1);

            SendEmailRequest em = new SendEmailRequest()
            {
                Destination = (new Destination() { BccAddresses = new List<String>() { ToEmail } }),
                Source = FromEmail,
                Message = new Message(new Content(Subject), new Body() { Text = new Content(Content) })
            };

            SendEmailResponse response = client.SendEmail(em);
        }

        //From E-mail address must be verified through Amazon
        /// <param name="AWSAccessKey">public key associated with our Amazon Account</param>
        /// <param name="AWSSecretKey">private key associated with our Amazon Account</param>
        /// <param name="ToEmail">Who do you want to send the E-mail to, seperate multiple addresses with a comma</param>
        /// <param name="FromEmail">Who is the e-mail from, this must be a verified e-mail address through Amazon</param>
        /// <param name="Subject">Subject of e-mail</param>
        /// <param name="Html">Html for e-mail</param>
        public void SendHTMLEmail(string AWSAccessKey, string AWSSecretKey, string ToEmail, string FromEmail, string Subject, String Html)
        {
            Amazon.SimpleEmail.AmazonSimpleEmailServiceClient client = new Amazon.SimpleEmail.AmazonSimpleEmailServiceClient(AWSAccessKey, AWSSecretKey, Amazon.RegionEndpoint.USEast1);

            SendEmailRequest em = new SendEmailRequest()
            {
                Destination = new Destination() { BccAddresses = new List<String>() { ToEmail } },
                Source = FromEmail,
                Message = new Message(new Content(Subject), new Body() { Html = new Content(Html) })
            };

            SendEmailResponse response = client.SendEmail(em);
        }
        
        //From E-mail address must be verified through Amazon
        /// <summary>
        /// Text Only for now, no attachments yet.  Amazon SES is still in Beta
        /// </summary>
        /// <param name="AWSAccessKey">public key associated with our Amazon Account</param>
        /// <param name="AWSSecretKey">private key associated with our Amazon Account</param>
        /// <param name="From">Who is the e-mail from, this must be a verified e-mail address through Amazon</param>
        /// <param name="To">Who do you want to send the E-mail to, seperate multiple addresses with a comma</param>
        /// <param name="Subject">Subject of e-mail</param>
        /// <param name="Attachment">File Location of attachment.  PDF's, text files and images supported, seperate multiple addresses with a comma</param>
        /// <param name="Text">Plain text for e-mail, can be null</param>
        /// <param name="Html">Html view for e-mail, can be null</param>
        /// <param name="ReplyTo ">Email address for replies, can be null</param>
        /// <param name="CCAddresses ">ArrayList of strings for CC'd addresses</param>
        /// <param name="BCCAddresses ">ArrayList of strings for BCC'd addresses</param>
        public bool SendEmailWithAttachments(string AWSAccessKey, string AWSSecretKey, String From, String To, String Subject, String Attachment, String Text = null, String Html = null, String ReplyTo = null, ArrayList CCAddresses = null, ArrayList BCCAddresses = null)
        {

            AlternateView plainView = null;

            if (Text != null)
                plainView = AlternateView.CreateAlternateViewFromString(Text, Encoding.UTF8, "text/plain");

            AlternateView htmlView = null;

            if (Html != null)
                htmlView = AlternateView.CreateAlternateViewFromString(Html, Encoding.UTF8, "text/html");

            MailMessage mailMessage = new MailMessage();

            mailMessage.From = new MailAddress(From);

            List<String> toAddresses = To.Replace(", ", ",").Split(',').ToList();

            foreach (String toAddress in toAddresses)
            {
                mailMessage.To.Add(new MailAddress(toAddress));

            }

            if (CCAddresses != null)
            {
                foreach (String ccAddress in CCAddresses)
                {
                    mailMessage.CC.Add(new MailAddress(ccAddress));
                }
            }

            if (BCCAddresses != null)
            {
                foreach (String bccAddress in BCCAddresses)
                {
                    mailMessage.Bcc.Add(new MailAddress(bccAddress));
                }
            }
            mailMessage.Subject = Subject;

            mailMessage.SubjectEncoding = Encoding.UTF8;

            if (ReplyTo != null)
            {
                mailMessage.ReplyTo = new MailAddress(ReplyTo);
            }

            if (Text != null)
            {
                mailMessage.AlternateViews.Add(plainView);
            }

            if (Html != null)
            {
                mailMessage.AlternateViews.Add(htmlView);
            }

            // Attachment Fix
            //System.Net.Mail.Attachment a = new System.Net.Mail.Attachment(Attachment);
            //mailMessage.Attachments.Add(a);
            foreach (String attachment in Attachment.Replace(", ", ",").Split(',').ToList())
            {
                System.Net.Mail.Attachment a = new System.Net.Mail.Attachment(attachment);
                mailMessage.Attachments.Add(a);
            }

            RawMessage rawMessage = new RawMessage();

            using (MemoryStream memoryStream = ConvertMailMessageToMemoryStream(mailMessage))
            {
                rawMessage.Data = memoryStream;
            }

            SendRawEmailRequest request = new SendRawEmailRequest();

            request.RawMessage = rawMessage;

            request.Destinations = toAddresses;
            request.Source = From;

            Amazon.SimpleEmail.AmazonSimpleEmailServiceClient ses = new Amazon.SimpleEmail.AmazonSimpleEmailServiceClient(AWSAccessKey, AWSSecretKey, Amazon.RegionEndpoint.USEast1);

            try
            {
                SendRawEmailResponse response = ses.SendRawEmail(request);

                return true;
            }

            catch
            {
                return false;
            }

        }

        public bool SendEmailWithAttachments(string AWSAccessKey, string AWSSecretKey, MailAddress From, MailAddress[] To, String Subject, System.Net.Mail.Attachment[] Attachments, String Text = null, String Html = null, MailAddress ReplyTo = null, MailAddress[] CCAddresses = null, MailAddress[] BCCAddresses = null)
        {
            AlternateView plainView = null;

            if (Text != null)
                plainView = AlternateView.CreateAlternateViewFromString(Text, Encoding.UTF8, "text/plain");

            AlternateView htmlView = null;

            if (Html != null)
                htmlView = AlternateView.CreateAlternateViewFromString(Html, Encoding.UTF8, "text/html");

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = From;

            foreach (MailAddress toAdd in To ?? Enumerable.Empty<MailAddress>())
                mailMessage.To.Add(toAdd);

            foreach (MailAddress toCC in CCAddresses ?? Enumerable.Empty<MailAddress>())
                mailMessage.CC.Add(toCC);

            foreach (MailAddress toBcc in BCCAddresses ?? Enumerable.Empty<MailAddress>())
                mailMessage.Bcc.Add(toBcc);

            mailMessage.Subject = Subject;

            mailMessage.SubjectEncoding = Encoding.UTF8;

            if (ReplyTo != null)
            {
                mailMessage.ReplyTo = ReplyTo;
            }

            if (Text != null)
            {
                mailMessage.AlternateViews.Add(plainView);
            }

            if (Html != null)
            {
                mailMessage.AlternateViews.Add(htmlView);
            }

            foreach (System.Net.Mail.Attachment a in Attachments ?? Enumerable.Empty<System.Net.Mail.Attachment>())
            {
                mailMessage.Attachments.Add(a);
            }

            RawMessage rawMessage = new RawMessage();

            using (MemoryStream memoryStream = ConvertMailMessageToMemoryStream(mailMessage))
            {
                rawMessage.Data = memoryStream;
            }

            SendRawEmailRequest request = new SendRawEmailRequest();

            request.RawMessage = rawMessage;

            request.Destinations = To.Select(a => a.Address).ToList();
            request.Source = From.Address;

            Amazon.SimpleEmail.AmazonSimpleEmailServiceClient ses = new Amazon.SimpleEmail.AmazonSimpleEmailServiceClient(AWSAccessKey, AWSSecretKey, Amazon.RegionEndpoint.USEast1);

            try
            {
                SendRawEmailResponse response = ses.SendRawEmail(request);

                return true;
            }

            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("There was an error sending the e-mail: {0}", ex.Message));
                return false;
            }
        }

        public MemoryStream ConvertMailMessageToMemoryStream(MailMessage message)
        {
	        Assembly assembly = typeof(SmtpClient).Assembly;

	        Type mailWriterType = assembly.GetType("System.Net.Mail.MailWriter");

	        MemoryStream fileStream = new MemoryStream();

	        ConstructorInfo mailWriterContructor = mailWriterType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(Stream) }, null);

	        object mailWriter = mailWriterContructor.Invoke(new object[] { fileStream });

	        MethodInfo sendMethod = typeof(MailMessage).GetMethod("Send", BindingFlags.Instance | BindingFlags.NonPublic);

            Assembly assemblyMailMessage = typeof(MailMessage).Assembly;

            // Rudimentary way to check for a .Net framework version number of the MailMessage that is being passed in by the code that hits this.
            // There was a new parameter added in 4.5. This way can't tell the difference between 4.0 and 4.5, but since we don't have anything
            // built for 4.0 and won't in the future I went with this route.
            if (int.Parse(assemblyMailMessage.ImageRuntimeVersion.Substring(1, 1)) < 4)
            {
                sendMethod.Invoke(message, BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { mailWriter, true }, null);
            }
            else
            {
                sendMethod.Invoke(message, BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { mailWriter, true, true }, null);
            }

	        MethodInfo closeMethod = mailWriter.GetType().GetMethod("Close", BindingFlags.Instance | BindingFlags.NonPublic);

	        closeMethod.Invoke(mailWriter, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { }, null);

	        return fileStream;
        }
    }
}
