
using System.Net.Mail;
using System.Net;
using System;
using System.Text;
using System.Net.Mime;

/**
 *  Unity3d发送邮件
 *  需要注意的是在playersetting中需要设置api level 为.net2.0 (.net2.0 subset不支持)
 */
public class Email
{
    static SmtpClient smtpClient = null;// 设置smtp协议
    static MailMessage mailMessage_mai = null; //设置邮件信息,要发送的内容
    public static string OutLook = "smtp-mail.outlook.com";
    
    /// <summary>
    /// 发邮件
    /// </summary>
    /// <param name="smtp">邮箱服务器名称</param>
    /// <param name="affix">附件路径</param>
    /// <param name="from">发件箱地址</param>
    /// <param name="pwd">发件箱密码</param>
    /// <param name="to">收件箱地址</param>
    /// <param name="title">邮件标题</param>
    /// <param name="body">邮件正文</param>
    /// <returns></returns>
    static bool SendMail(string smtp, string affix,
        string from, string pwd, string to, string title, string body)
    {
        smtpClient = new SmtpClient();
        smtpClient.Host = smtp;
        smtpClient.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback = (a,b,c,d) => { return true; };
      //指定服务器认证
       NetworkCredential network = new NetworkCredential(from, pwd);
        network = network.GetCredential(from, 25,"TLS");
        //指定发件人信息,包括邮箱地址和密码
        //NetworkCredential nc = new NetworkCredential(from, pwd);
        smtpClient.Credentials =network; //这个在手机平台不成功
        //指定如何发送邮件
        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        smtpClient.UseDefaultCredentials = false;
        //创建mailMessage对象
        mailMessage_mai = new MailMessage(from, to);
        mailMessage_mai.Subject = title;

        //设置正文默认格式为html
        mailMessage_mai.Body = body;
        mailMessage_mai.IsBodyHtml = true;
        mailMessage_mai.BodyEncoding = Encoding.UTF8;

        //添加附件
        if (!string.IsNullOrEmpty(affix))
        {
            Attachment data = new Attachment(affix, MediaTypeNames.Application.Octet);
            mailMessage_mai.Attachments.Add(data);
        }

        try
        {
            // smtpClient.Send(mailMessage_mai);
            //发送
            smtpClient.Send(mailMessage_mai);
            return true;//返回true则发送成功
        }
        catch (Exception ex)
        {
            return false;//返回false则发送失败
        }
    }
    public static void StartSend()
    {
        string smtp = "smtp.qq.com";
        //(如果发送不成功, 那么改成: smtp.exmail.qq.com 这里改成foxmail的服务器)
        string from = "huqiang1990@outlook.com";
        string pwd = "test";
        string to = "651726047@qq.com";
        string title = "哈哈";
        string body = "嘿嘿";
       if(SendMail(OutLook, null, from, pwd, to, title, body))
        UnityEngine.Debug.Log("send");
       else UnityEngine.Debug.Log("failed");
    }
}
