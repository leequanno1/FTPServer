using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace lib
{
    public static class TcpProtocol
    {
        /// <summary>
        /// Gửi dữ liệu kiểu T qua socket
        /// </summary>
        public static void Send<T>(Socket socket, T data)
        {
            try
            {
                // Chuyển object thành JSON string
                string json = JsonSerializer.Serialize(data);
                byte[] jsonData = Encoding.UTF8.GetBytes(json);

                // Gửi độ dài của dữ liệu (4 byte đầu)
                byte[] lengthPrefix = BitConverter.GetBytes(jsonData.Length);
                socket.Send(lengthPrefix);

                // Gửi dữ liệu chính
                socket.Send(jsonData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send error: {ex.Message}");
            }
        }

        /// <summary>
        /// Nhận dữ liệu kiểu T từ socket
        /// </summary>
        public static bool Receive<T>(Socket socket, out T data)
        {
            data = default;

            //try
            //{
                // Nhận độ dài dữ liệu (4 byte đầu)
                byte[] lengthBuffer = new byte[4];
                int bytesRead = socket.Receive(lengthBuffer);
                if (bytesRead != 4) return false;

                int dataLength = BitConverter.ToInt32(lengthBuffer, 0);

                // Nhận dữ liệu chính
                byte[] dataBuffer = new byte[dataLength];
                int received = 0;
                while (received < dataLength)
                {
                    int read = socket.Receive(dataBuffer, received, dataLength - received, SocketFlags.None);
                    if (read == 0) return false; // Kết nối bị đóng
                    received += read;
                }

                // Chuyển JSON về object kiểu T
                string json = Encoding.UTF8.GetString(dataBuffer);
                data = JsonSerializer.Deserialize<T>(json);

                return true;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Lỗi khi nhận dữ liệu: {ex.Message}");
            //    return false;
            //}
        }
    }
}