using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;

using System.Security.Authentication;
using System.IO;

using Apple.Apns.Notifications;
using Apple.Apns.Notifications.SendMessage;


class PushNotification
{

    private bool IsProduct = false;
    private string P12File;
    private string P12FilePsw;
    private string apsHost;
    private string Product;
    private string Develop;
    private string NotificationAlert;
    private string NotificationSound;
    private string NotificationBage;

    public PushNotification()
    {
        IsProduct = Convert.ToBoolean(ConfigurationManager.AppSettings["IsProduct"]);

        P12File = ConfigurationManager.AppSettings["P12File"];
        P12FilePsw = ConfigurationManager.AppSettings["P12FilePsw"];

        Product = ConfigurationManager.AppSettings["Product"];
        Develop = ConfigurationManager.AppSettings["Develop"];

        apsHost = (IsProduct == true) ? Product : Develop;

        NotificationAlert = ConfigurationManager.AppSettings["NotificationAlert"];
        NotificationSound = ConfigurationManager.AppSettings["NotificationSound"];
        NotificationBage = ConfigurationManager.AppSettings["NotificationBage"];
    }


    private X509Certificate getServerCert()
    {
        X509Certificate certificate;
        string p12File = System.IO.Path.Combine(Environment.CurrentDirectory, P12File);

        certificate = new X509Certificate2(System.IO.File.ReadAllBytes(p12File), P12FilePsw);

        return certificate;
    }

    private string[] GetAllTokens()
    {
        using (var dbContext = new DataClasses1DataContext())
        {
            var tokens = (from d in dbContext.tb_DeviceTokens
                          select d.DeviceToken).Distinct().ToArray();

            return tokens;
        }
    }

    public void SendMessage()
    {
        try
        {
            X509Certificate2Collection certs = new X509Certificate2Collection();

            certs.Add(getServerCert());

            TcpClient tcpClient = new TcpClient();

            tcpClient.Connect(apsHost, 2195);

            SslStream sslStream = new SslStream(tcpClient.GetStream());

            sslStream.AuthenticateAsClient(apsHost, certs, SslProtocols.Default, false);

            string[] tokens = GetAllTokens();

            for (int i = 0; i < tokens.Length; i++)
            {
                string testDeviceToken = tokens[i];
                Notification alertNotification = new Notification(testDeviceToken);

                alertNotification.Payload.Alert.Body = NotificationAlert;
                alertNotification.Payload.Sound = NotificationSound;
                alertNotification.Payload.Badge = int.Parse(NotificationBage);


                sslStream.Write(alertNotification.ToBytes());
                Console.WriteLine(i + " Success!");
            }

            Console.ReadLine();
        }

        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.ReadLine();
        }
    }
}


class Program
{
    static void Main(string[] args)
    {
        PushNotification push = new PushNotification();
        push.SendMessage();
    }

    //private static X509Certificate getServerCert()
    //{
    //    string p12File = System.IO.Path.Combine(Environment.CurrentDirectory, "ck.p12");
    //    string p12FilePassword = "iphone";

    //    X509Certificate certificate;

    //    certificate = new X509Certificate2(System.IO.File.ReadAllBytes(p12File), p12FilePassword);

    //    return certificate;
    //}

    //private static string[] GetAllTokens()
    //{
    //    using (var dbContext = new DataClasses1DataContext())
    //    {
    //        var tokens = (from d in dbContext.tb_DeviceTokens
    //                      select d.DeviceToken).Distinct().ToArray();

    //        return tokens;
    //    }
    //}

    //public static void SendMessage()
    //{
    //    try
    //    {
    //        X509Certificate2Collection certs = new X509Certificate2Collection();

    //        certs.Add(getServerCert());

    //        string apsHost;

    //        apsHost = "gateway.sandbox.push.apple.com";

    //        TcpClient tcpClient = new TcpClient();

    //        tcpClient.Connect(apsHost, 2195);

    //        SslStream sslStream = new SslStream(tcpClient.GetStream());

    //        sslStream.AuthenticateAsClient(apsHost, certs, SslProtocols.Default, false);

    //        string[] tokens = GetAllTokens();

    //        for (int i = 0; i < tokens.Length; i++)
    //        {
    //            string testDeviceToken = tokens[i];
    //            Notification alertNotification = new Notification(testDeviceToken);

    //            alertNotification.Payload.Alert.Body = "2011-10-01  Libiya no kazhafei!!";
    //            alertNotification.Payload.Sound = "default";
    //            alertNotification.Payload.Badge = 2;


    //            sslStream.Write(alertNotification.ToBytes());
    //            Console.WriteLine("Success!");
    //        }

    //        Console.ReadLine();
    //    }

    //    catch (Exception ex)
    //    {
    //        Console.WriteLine(ex.Message);
    //        Console.ReadLine();
    //    }
    //}
}
 
