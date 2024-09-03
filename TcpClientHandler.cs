using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace trivia_client
{
    public class TcpClientHandler
    {
        private TcpClient client;
        private NetworkStream stream;

        public async Task ConnectAsync(string host, int port)
        {
            client = new TcpClient();
            await client.ConnectAsync(host, port);
            stream = client.GetStream();
        }

        public async Task SendMessageAsync(byte actionCode, string jsonContent)
        {
            if (stream.CanWrite)
            {
                byte[] contentBytes = Encoding.ASCII.GetBytes(jsonContent);
                int contentLength = contentBytes.Length;

                byte[] headerBytes = new byte[5];
                headerBytes[0] = actionCode;
                BitConverter.GetBytes(contentLength).CopyTo(headerBytes, 1);
                Array.Reverse(headerBytes, 1, 4);

                await stream.WriteAsync(headerBytes, 0, 5);
                await stream.WriteAsync(contentBytes, 0, contentLength);
            }
        }

        public async Task<(byte ActionCode, string JsonContent)> ReceiveMessageAsync()
        {
            byte[] headerBytes = new byte[5];
            int bytesRead = 0;
            while (bytesRead < 5)
            {
                int read = await stream.ReadAsync(headerBytes, bytesRead, 5 - bytesRead);
                if (read == 0)
                    throw new Exception("Connection closed by server.");
                bytesRead += read;
            }

            byte actionCode = headerBytes[0];
            Array.Reverse(headerBytes, 1, 4);
            int contentLength = BitConverter.ToInt32(headerBytes, 1);

            byte[] contentBytes = new byte[contentLength];
            bytesRead = 0;
            while (bytesRead < contentLength)
            {
                int read = await stream.ReadAsync(contentBytes, bytesRead, contentLength - bytesRead);
                if (read == 0)
                    throw new Exception("Connection closed by server.");
                bytesRead += read;
            }

            string jsonContent = Encoding.UTF8.GetString(contentBytes);

            return (actionCode, jsonContent);
        }

        public void Close()
        {
            stream?.Close();
            client?.Close();
        }
    }
}
