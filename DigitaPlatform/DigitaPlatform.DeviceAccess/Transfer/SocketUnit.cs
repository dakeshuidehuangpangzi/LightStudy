using DigitaPlatform.DeviceAccess.Base;
using DigitaPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DigitaPlatform.DeviceAccess.Transfer
{
    internal class SocketUnit:TransferObject
    {
        Socket socket;

        public SocketUnit()

        {
            socket =new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TUnit = socket;
        }

        string ip;
        int port=0;
        internal override Result Config(List<DevicePropItemEntity> props)
        {
            try
            {
                ip = props.FirstOrDefault(x => x.PropName == "Ip")?.PropValue;
                int.TryParse(props.FirstOrDefault(x=>x.PropName == "Port")?.PropValue, out port);
                return new Result();
            }
            catch (Exception ex )
            {
                return new Result(false, ex.Message);
            }
        }

        ManualResetEvent TimeoutObject = new ManualResetEvent(false);
        readonly object socketLock = new object();
        internal override Result Connect(int trycount = 30)
        {
            lock (socketLock)
            {
                Result result = new Result();
                try
                {
                    if (socket == null)
                        // ProtocolType 可支持配置
                        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    int count = 0;
                    bool connectState = false;
                    TimeoutObject.Reset();
                    while (count < trycount)
                    {
                        if (!(!socket.Connected || (socket.Poll(200, SelectMode.SelectRead) && (socket.Available == 0))))
                        {
                            return result;
                        }
                        try
                        {
                            socket?.Close();
                            socket.Dispose();
                            socket = null;
                            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            socket.BeginConnect(ip, port, callback =>
                            {
                                connectState = false;
                                var cbSocket = callback.AsyncState as Socket;
                                if (cbSocket != null)
                                {
                                    connectState = cbSocket.Connected;

                                    if (cbSocket.Connected)
                                        cbSocket.EndConnect(callback);
                                }
                                TimeoutObject.Set();
                            }, socket);
                            TimeoutObject.WaitOne(2000, false);
                            if (!connectState) throw new Exception();
                            else break;
                        }
                        catch (SocketException ex)
                        {
                            if (ex.SocketErrorCode == SocketError.TimedOut)// 连接超时
                                throw new Exception(ex.Message);
                        }
                        catch (Exception) { }
                    }
                    if (socket == null || !socket.Connected || ((socket.Poll(200, SelectMode.SelectRead) && (socket.Available == 0))))
                    {
                        throw new Exception("网络连接失败");
                    }
                }
                catch (Exception ex)
                {
                    result = new Result(false, ex.Message);
                }

                return result;
            }
        }

        // 通用
        internal override Result<List<byte>> SendAndReceived(List<byte> req, int header_len, int timeout, Func<byte[], int> calcLen)
        {
            lock (socketLock)
            {
                Result<List<byte>> result = new Result<List<byte>>();
                try
                {
                    socket.ReceiveTimeout = timeout;

                    if (req != null)
                        socket.Send(req.ToArray(), 0, req.Count, SocketFlags.None);

                    // 获取报文头字节
                    byte[] data = new byte[header_len];
                    socket.Receive(data, 0, header_len, SocketFlags.None);
                    result.Data = new List<byte>(data);

                    int dataLen = 0;
                    if (calcLen != null)
                        dataLen = calcLen(data);
                    if (dataLen == 0)
                        throw new Exception("获取数据长度失败");

                    // 剩余的报文字节
                    data = new byte[dataLen];
                    socket.Receive(data, 0, dataLen, SocketFlags.None);
                    result.Data.AddRange(data);
                }
                catch (SocketException se)
                {
                    result.Status = false;
                    if (se.SocketErrorCode == SocketError.TimedOut)
                    {
                        result.Message = "未获取到响应数据，接收超时";
                    }
                }
                catch (Exception ex)
                {
                    result.Status = false;
                    result.Message = ex.Message;
                }
                return result;
            }
        }

        internal override Result<List<byte>> SendAndReceived(List<byte> req,int timeout, Func<byte[], int> calcLen)
        {
            lock (socketLock)
            {
                Result<List<byte>> result = new Result<List<byte>>();
                try
                {
                    socket.ReceiveTimeout = timeout;

                    if (req != null)
                        socket.Send(req.ToArray(), 0, req.Count, SocketFlags.None);

                    // 获取报文头字节
                    byte[] data = new byte[10];
                    socket.Receive(data, 0, 10, SocketFlags.None);
                    result.Data = new List<byte>(data);

                    int dataLen = 0;
                    if (calcLen != null)
                        dataLen = calcLen(data);
                    if (dataLen == 0)
                        throw new Exception("获取数据长度失败");

                    // 剩余的报文字节
                    data = new byte[dataLen];
                    socket.Receive(data, 0, dataLen, SocketFlags.None);
                    result.Data.AddRange(data);
                }
                catch (SocketException se)
                {
                    result.Status = false;
                    if (se.SocketErrorCode == SocketError.TimedOut)
                    {
                        result.Message = "未获取到响应数据，接收超时";
                    }
                }
                catch (Exception ex)
                {
                    result.Status = false;
                    result.Message = ex.Message;
                }
                return result;
            }
        }

        internal override Result Close()
        {
            try
            {
                if (socket?.Connected ?? false)
                    socket?.Shutdown(SocketShutdown.Both);//正常关闭连接
                base.Close();
            }
            catch { }

            try
            {
                socket?.Close();
                return new Result();
            }
            catch (Exception ex) { return new Result(false, ex.Message); }
        }
    }
}
