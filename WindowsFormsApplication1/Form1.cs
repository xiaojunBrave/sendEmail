using System;
using System.Text;
using System.Windows.Forms;
using System.Net.Mail;
using LumiSoft.Net.AUTH;
using LumiSoft.Net.Mail;
using LumiSoft.Net.MIME;
using LumiSoft.Net.SMTP.Client;
using System.IO;
using System.Text.RegularExpressions;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendEmail();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            eMail = ((TextBox)sender).Text.Trim();
        }
        string eMail = "";
        public bool SendEmail()
        {
            string mail = eMail;
            bool send = SendMailCG.SendMail("<p>您好!</p><p>感谢你使用绘学霸App</p><p>点击以下链接更改手机号为" + 55555 + "</p><a href='" + "http://116.62.37.16:8888/VerifyMailbox?keyCode=530140&email=13539890642@163.com" + "'>点击此处进行验证</a>", "这是一封测试邮件,请勿回复!", mail);
            if (send)
            {
                Console.WriteLine("发送邮件给:" + eMail + "成功！");
            }
            else {
                Console.WriteLine("发送邮件给:" + eMail + "失败！");

            }
            return send;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Regex reg = new Regex(@"^[1][0-9]{10}$");
            if (reg.Match(mobileNo).Success)
            {
                MessageBox.Show(mobileNo + ":是合法电话号码！");
            }
            else {
                MessageBox.Show(mobileNo + ":是不合法电话号码！");
            }
        }
        string mobileNo = "";
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            mobileNo = ((TextBox)sender).Text.Trim();
        }
    }

    class SendMailCG
    {
        public static string smtp = "smtp.163.com";
        public static int port = 25;
        public static string sendEmail = "cgwangwkh@163.com";
        public static string sendPassword = "cgwang163";
        public static string sendUser = "cgwangwkh";
        public static bool SendMail(string Body, string Title, string reciveEmail)
        {
            bool sended = false;
            using (SMTP_Client client = new SMTP_Client())
            {
                try
                {
                    //与Pop3服务器建立连接
                    client.Connect(smtp, port, false);
                    client.EhloHelo(smtp);
                    //验证身份
                    var authhh = new AUTH_SASL_Client_Plain(sendEmail, sendPassword);
                    client.Auth(authhh);
                    client.MailFrom(sendEmail, -1);
                    //收件人列表
                    client.RcptTo(reciveEmail);
                    //采用Mail_Message类型的Stream
                    Mail_Message m = Create_PlainText_Html_Attachment_Image(reciveEmail, sendEmail, sendEmail, Title, Body);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        m.ToStream(stream, new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.Q, Encoding.UTF8), Encoding.UTF8);
                        stream.Position = 0;
                        client.SendMessage(stream);
                        sended = true;
                    }
                    if (m != null)
                    {
                        m.Dispose();
                    }
                    client.Disconnect();
                    client.Dispose();
                }
                catch {
                    return false;
                }
            }
            return sended;
        }
        private static Mail_Message Create_PlainText_Html_Attachment_Image(string tomail,
          string mailFrom, string mailFromDisplay,
          string subject, string body)
        {
            Mail_Message msg = new Mail_Message();
            msg.MimeVersion = "1.0";
            msg.MessageID = MIME_Utils.CreateMessageID();
            msg.Date = DateTime.Now;
            msg.Subject = subject;
            msg.From = new Mail_t_MailboxList();
            msg.From.Add(new Mail_t_Mailbox("绘学霸", mailFrom));
            msg.To = new Mail_t_AddressList();
            msg.To.Add(new Mail_t_Mailbox(tomail, tomail));

            //设置回执通知
            string notifyEmail = "13539890642@163.com";
            if (!string.IsNullOrEmpty(notifyEmail))
            {
                msg.DispositionNotificationTo = new Mail_t_MailboxList();
                msg.DispositionNotificationTo.Add(new Mail_t_Mailbox(notifyEmail, notifyEmail));
            }

            #region MyRegion

            //--- multipart/mixed -----------------------------------
            MIME_h_ContentType contentType_multipartMixed = new MIME_h_ContentType(MIME_MediaTypes.Multipart.mixed);
            contentType_multipartMixed.Param_Boundary = Guid.NewGuid().ToString().Replace('-', '.');
            MIME_b_MultipartMixed multipartMixed = new MIME_b_MultipartMixed(contentType_multipartMixed);
            msg.Body = multipartMixed;

            //--- multipart/alternative -----------------------------
            MIME_Entity entity_multipartAlternative = new MIME_Entity();
            MIME_h_ContentType contentType_multipartAlternative = new MIME_h_ContentType(MIME_MediaTypes.Multipart.alternative);
            contentType_multipartAlternative.Param_Boundary = Guid.NewGuid().ToString().Replace('-', '.');
            MIME_b_MultipartAlternative multipartAlternative = new MIME_b_MultipartAlternative(contentType_multipartAlternative);
            entity_multipartAlternative.Body = multipartAlternative;
            multipartMixed.BodyParts.Add(entity_multipartAlternative);

            //--- text/plain ----------------------------------------
            MIME_Entity entity_text_plain = new MIME_Entity();
            MIME_b_Text text_plain = new MIME_b_Text(MIME_MediaTypes.Text.plain);
            entity_text_plain.Body = text_plain;

            //普通文本邮件内容，如果对方的收件客户端不支持HTML，这是必需的
            string plainTextBody = "如果你邮件客户端不支持HTML格式，或者你切换到“普通文本”视图，将看到此内容";
            /*
            if (!string.IsNullOrEmpty(plaintTextTips))
            {
                plainTextBody = "回执信息";
            }
            */
            text_plain.SetText(MIME_TransferEncodings.QuotedPrintable, Encoding.UTF8, plainTextBody);
            multipartAlternative.BodyParts.Add(entity_text_plain);

            //--- text/html -----------------------------------------
            string htmlText = body;
            MIME_Entity entity_text_html = new MIME_Entity();
            MIME_b_Text text_html = new MIME_b_Text(MIME_MediaTypes.Text.html);
            entity_text_html.Body = text_html;
            text_html.SetText(MIME_TransferEncodings.QuotedPrintable, Encoding.UTF8, htmlText);
            multipartAlternative.BodyParts.Add(entity_text_html);
            #endregion
            return msg;
        }
    }

}
