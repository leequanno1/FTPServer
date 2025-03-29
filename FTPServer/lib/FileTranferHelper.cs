using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace lib
{
    public class FileTranferHelper
    {
        private const int BufferSize = 4096; // Dung lượng buffer 4KB

        /// <summary>
        /// Send file to socket
        /// </summary>
        /// <param name="socket">Connected socket</param>
        /// <param name="filePath">Sended file path</param>
        /// <param name="statusHandler">Function get current byte readed and return current byte readed</param>
        public static void SendFileTo(Socket socket, string filePath, Func<int, int> statusHandler = null)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File không tồn tại!");
                return;
            }
            try
            {
                // Gửi dữ liệu file
                byte[] buffer = new byte[BufferSize];
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    int bytesRead;
                    int totalRead = 0;
                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        socket.Send(buffer, bytesRead, SocketFlags.None);
                        totalRead += bytesRead;
                        if (statusHandler != null) statusHandler(totalRead);
                    }
                }
                Console.WriteLine($"File '{filePath}' sended.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send error: {ex.Message}");
            }
        }

        /// <summary>
        /// Recieve from socket
        /// </summary>
        /// <param name="socket">Connected socket</param>
        /// <param name="savePath">Saved file path</param>
        /// <param name="fileName">Saved file name</param>
        /// <param name="statusHandler"> function get current byte readed and return current byte readed</param>
        public static void ReceiveFileFrom(Socket socket, string savePath, string fileName, Func<int, int> statusHandler = null)
        {
            try
            {
                // Nhận tên file
                byte[] buffer = new byte[BufferSize];
                int bytesRead = socket.Receive(buffer);
                string receivedText = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string filePath = Path.Combine(savePath, fileName);

                Console.WriteLine($"Recieving: {fileName}");
                int totalRead = 0;

                // Nhận dữ liệu file
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    while ((bytesRead = socket.Receive(buffer)) > 0)
                    {
                        fs.Write(buffer, 0, bytesRead);
                        totalRead += bytesRead;

                        if (statusHandler != null) statusHandler(totalRead);

                        // Kiểm tra nếu nhận hết dữ liệu
                        if (bytesRead < BufferSize)
                            break;
                    }
                }
                Console.WriteLine($"File saved at: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Recieve error: {ex.Message}");
            }
        }
    }
}
