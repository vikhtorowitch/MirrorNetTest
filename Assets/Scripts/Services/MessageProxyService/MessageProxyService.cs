using System;
using System.Collections.Generic;
using System.Reflection;
using Mirror;
using UnityEngine;

namespace MirrorNetTest.Services.MessageProxyService
{
    /// <summary>
    /// MessageProxyService - фильтр входящих сообщений Mirror на уровне транспорта.
    /// Перехватывает данные до их обработки в NetworkClient, отсеивает сообщения,
    /// для которых на клиенте нет зарегистрированного обработчика (NetworkClient.handlers),
    /// и передаёт оригинальному обработчику только «разрешённые» сообщения.
    /// Это предотвращает дисконнект клиента из-за "Unknown message id", если сервер прислал
    /// тип, который клиент не обрабатывает.
    /// </summary>
    public class MessageProxyService : ServiceBase
    {
        // актуальный снимок зарегистрированных на клиенте msgId (синхронизируется из NetworkClient.handlers)
        private static readonly HashSet<ushort> _activeClientSubscriptions = new HashSet<ushort>();
        // кеш словаря NetworkClient.handlers (internal) - источник правды о зарегистрированных обработчиках
        private static Dictionary<ushort, NetworkMessageDelegate> _mirrorClientHandlers;
        private Action<ArraySegment<byte>, int> originalMirrorCallback;

        /// <summary>
        /// Синхронизирует _activeClientSubscriptions с NetworkClient.handlers.
        /// Вызывать ПОСЛЕ всех регистраций/отмен регистраций обработчиков.
        /// </summary>
        public static void UpdateActiveSubscriptions()
        {
            CacheMirrorHandlers();
            _activeClientSubscriptions.Clear();

            if (_mirrorClientHandlers == null)
            {
                return;
            }

            foreach (ushort msgId in _mirrorClientHandlers.Keys)
            {
                _activeClientSubscriptions.Add(msgId);
            }
        }

        private static void CacheMirrorHandlers()
        {
            if (_mirrorClientHandlers != null)
            {
                return;
            }

            FieldInfo field = typeof(NetworkClient).GetField("handlers", BindingFlags.Static | BindingFlags.NonPublic);

            _mirrorClientHandlers = (Dictionary<ushort, NetworkMessageDelegate>)field?.GetValue(null);

            if (_mirrorClientHandlers == null)
            {
                Debug.LogWarning("[MessageProxyService] Can't access NetworkClient.handlers. " +
                                 "Filters will safely ignore all messages until UpdateActiveSubscriptions.");
            }
        }

        private static bool IsRegistered(ushort msgId)
        {
            return _activeClientSubscriptions.Contains(msgId);
        }

        public override void Init()
        {
            if (Transport.active == null)
            {
                Debug.LogError("[MessageProxyService] Can't init: Transport is not active");
                return;
            }

            UpdateActiveSubscriptions();

            originalMirrorCallback = Transport.active.OnClientDataReceived;

            if (originalMirrorCallback == null)
            {
                Debug.LogWarning("[MessageProxyService] OnClientDataReceived is null. " +
                    "Make sure Init() is called AFTER NetworkClient.Connect() " +
                    "so NetworkClient.OnTransportData is already subscribed.");
            }

            Transport.active.OnClientDataReceived = OnInterceptedData;
            Debug.Log("[MessageProxyService] Init done.");
        }

        /// <summary>
        /// Перехват данных транспорта: парсит батч Mirror (timestamp + varint-size + msgId + content),
        /// отсеивает незарегистрированные типы сообщений и передаёт оригинальному обработчику
        /// (NetworkClient) только оставшиеся сообщения.
        /// </summary>
        private void OnInterceptedData(ArraySegment<byte> data, int channelId)
        {
            if (originalMirrorCallback == null)
            {
                return;
            }

            using (NetworkWriterPooled writer = NetworkWriterPool.Get())
            using (NetworkReaderPooled reader = NetworkReaderPool.Get(data))
            {
                if (reader.Remaining < Batcher.TimestampSize)
                {
                    return;
                }

                double timestamp = reader.ReadDouble();
                writer.WriteDouble(timestamp);

                bool hasAnyValidMessage = false;

                while (reader.Remaining > 0)
                {
                    try
                    {
                        int messageSize = (int)Compression.DecompressVarUInt(reader);
                        int messageDataPos = reader.Position;

                        if (reader.Remaining < messageSize)
                        {
                            Debug.LogError("[MessageProxyService] Incomplete batch: messageSize exceeds remaining data");
                            break;
                        }

                        ushort msgId = reader.ReadUShort();

                        if (IsRegistered(msgId))
                        {
                            reader.Position = messageDataPos;
                            ArraySegment<byte> fullMessage = reader.ReadBytesSegment(messageSize);

                            Compression.CompressVarUInt(writer, (ulong)fullMessage.Count);
                            writer.WriteBytes(fullMessage.Array, fullMessage.Offset, fullMessage.Count);

                            hasAnyValidMessage = true;
                            //Debug.Log($"[MessageProxyService] Aprooved message: {msgId}");
                        }
                        else
                        {
                            Debug.Log($"[MessageProxyService] Ignoring unknown message: {msgId}");
                            reader.Position = messageDataPos + messageSize;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[MessageProxyService] Package parsing error: {ex.Message}");
                        break;
                    }
                }

                if (hasAnyValidMessage)
                {
                    ArraySegment<byte> filteredData = writer.ToArraySegment();
                    originalMirrorCallback.Invoke(filteredData, channelId);
                }
            }
        }

        public override void Dispose()
        {
            if (Transport.active != null && originalMirrorCallback != null)
            {
                Transport.active.OnClientDataReceived = originalMirrorCallback;
                originalMirrorCallback = null;
            }
        }        
    }
}