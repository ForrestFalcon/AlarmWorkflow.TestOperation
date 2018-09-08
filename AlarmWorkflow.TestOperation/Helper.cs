using AlarmWorkflow.Shared.Core;
using Mustache;
using Newtonsoft.Json;
using PdfSharp;
using PdfSharp.Pdf;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace AlarmWorkflow.TestOperation
{
    static class Helper
    {
        public const int DEFAULT_PORT = 8112;

        public static void GeneratePDF(Operation operation, String file)
        {
            //Paths
            var source = "alarmfax.htm";

            //Create PDF
            string text = File.ReadAllText(source);
            FormatCompiler compiler = new FormatCompiler();
            Generator generator = compiler.Compile(text);
            string result = generator.Render(new { Operation = operation });

            using (PdfDocument pdf = PdfGenerator.GeneratePdf(result, PageSize.A4))
            {
                pdf.Save(file);
            }
        }

        public static void SendOperation(String server, Operation operation)
        {
            string message = JsonConvert.SerializeObject(operation, Formatting.None, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
            });

            IPEndPoint endPoint = ParseHost(server, DEFAULT_PORT);
            using (TcpClient client = new TcpClient())
            {
                client.Connect(endPoint);
                using (NetworkStream stream = client.GetStream())
                {
                    Byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }
            }
        }

        #region Get IPEndPoint
        public static IPEndPoint ParseHost(string endpointstring)
        {
            return ParseHost(endpointstring, -1);
        }

        public static IPEndPoint ParseHost(string endpointstring, int defaultport)
        {
            if (string.IsNullOrEmpty(endpointstring)
                || endpointstring.Trim().Length == 0)
            {
                throw new ArgumentException("Endpoint descriptor may not be empty.");
            }

            if (defaultport != -1 &&
                (defaultport < IPEndPoint.MinPort
                || defaultport > IPEndPoint.MaxPort))
            {
                throw new ArgumentException(string.Format("Invalid default port '{0}'", defaultport));
            }

            string[] values = endpointstring.Split(new char[] { ':' });
            IPAddress ipaddy;
            int port = -1;

            //check if we have an IPv6 or ports
            if (values.Length <= 2) // ipv4 or hostname
            {
                if (values.Length == 1)
                    //no port is specified, default
                    port = defaultport;
                else
                    port = getPort(values[1]);

                //try to use the address as IPv4, otherwise get hostname
                if (!IPAddress.TryParse(values[0], out ipaddy))
                    ipaddy = getIPfromHost(values[0]);
            }
            else if (values.Length > 2) //ipv6
            {
                //could [a:b:c]:d
                if (values[0].StartsWith("[") && values[values.Length - 2].EndsWith("]"))
                {
                    string ipaddressstring = string.Join(":", values.Take(values.Length - 1).ToArray());
                    ipaddy = IPAddress.Parse(ipaddressstring);
                    port = getPort(values[values.Length - 1]);
                }
                else //[a:b:c] or a:b:c
                {
                    ipaddy = IPAddress.Parse(endpointstring);
                    port = defaultport;
                }
            }
            else
            {
                throw new FormatException(string.Format("Invalid endpoint ipaddress '{0}'", endpointstring));
            }

            if (port == -1)
                throw new ArgumentException(string.Format("No port specified: '{0}'", endpointstring));

            return new IPEndPoint(ipaddy, port);
        }

        private static int getPort(string p)
        {
            int port;

            if (!int.TryParse(p, out port)
             || port < IPEndPoint.MinPort
             || port > IPEndPoint.MaxPort)
            {
                throw new FormatException(string.Format("Invalid end point port '{0}'", p));
            }

            return port;
        }

        private static IPAddress getIPfromHost(string p)
        {
            var hosts = Dns.GetHostAddresses(p);

            if (hosts == null || hosts.Length == 0)
                throw new ArgumentException(string.Format("Host not found: {0}", p));

            IPAddress adress = hosts.Where(_ => _.AddressFamily == AddressFamily.InterNetwork).SingleOrDefault();
            return adress == null ? hosts[0] : adress;
        }
        #endregion
    }
}
