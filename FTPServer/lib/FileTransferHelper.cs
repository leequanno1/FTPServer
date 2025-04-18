using System;
using System.IO;
using System.Net.Sockets;

namespace lib
{
    public class FileTransferHelper
    {
        private const int BufferSize = 4096; // 4KB buffer

        /// <summary>
        /// Gửi file qua socket. Trước tiên gửi 8 byte độ dài file, sau đó là nội dung file.
        /// </summary>
        public static void SendFileTo(Socket socket, string filePath, Func<long, long> statusHandler = null)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File không tồn tại.");
                return;
            }

            try
            {
                long fileSize = new FileInfo(filePath).Length;
                byte[] fileSizeBytes = BitConverter.GetBytes(fileSize);
                socket.Send(fileSizeBytes); // Gửi kích thước file trước

                Console.WriteLine($"Đang gửi file: {filePath} ({fileSize} bytes)");

                byte[] buffer = new byte[BufferSize];
                long totalSent = 0;

                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    int bytesRead;
                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        socket.Send(buffer, 0, bytesRead, SocketFlags.None);
                        totalSent += bytesRead;
                        statusHandler?.Invoke(totalSent);
                    }
                }

                Console.WriteLine($"Đã gửi xong file: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gửi file: {ex.Message}");
            }
        }

        /// <summary>
        /// Nhận file từ socket. Đọc trước 8 byte độ dài file, sau đó nhận đúng số byte tương ứng.
        /// </summary>
        public static void ReceiveFileFrom(Socket socket, string savePath, string fileName, Func<long, long> statusHandler = null)
        {
            try
            {
                // Nhận kích thước file (8 byte đầu tiên)
                byte[] sizeBuffer = new byte[8];
                int received = 0;
                while (received < 8)
                {
                    int read = socket.Receive(sizeBuffer, received, 8 - received, SocketFlags.None);
                    if (read <= 0) throw new Exception("Kết nối bị đóng khi đang nhận kích thước file.");
                    received += read;
                }

                long fileSize = BitConverter.ToInt64(sizeBuffer, 0);
                Console.WriteLine($"Đang nhận file '{fileName}' ({fileSize} bytes)");

                string filePath = Path.Combine(savePath, fileName);
                byte[] buffer = new byte[BufferSize];
                long totalRead = 0;

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    while (totalRead < fileSize)
                    {
                        int bytesToRead = (int)Math.Min(BufferSize, fileSize - totalRead);
                        int bytesRead = socket.Receive(buffer, 0, bytesToRead, SocketFlags.None);
                        if (bytesRead == 0) break;

                        fs.Write(buffer, 0, bytesRead);
                        totalRead += bytesRead;
                        statusHandler?.Invoke(totalRead);
                    }
                }

                Console.WriteLine($"Đã lưu file tại: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi nhận file: {ex.Message}");
            }
        }
    }
}
