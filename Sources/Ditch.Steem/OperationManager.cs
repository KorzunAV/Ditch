﻿using System;
using System.Collections.Generic;
using System.Threading;
using Cryptography.ECDSA;
using Ditch.Core;
using Ditch.Core.Helpers;
using Ditch.Core.JsonRpc;
using Ditch.Steem.Helpers;
using Ditch.Steem.JsonRpc;
using Ditch.Steem.Models.ApiObj;
using Ditch.Steem.Models.Operations;
using Ditch.Steem.Models.Other;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ditch.Steem
{
    public partial class OperationManager
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly IConnectionManager _connectionManager;
        private List<string> _urls;

        public byte[] ChainId { get; private set; }
        public bool IsConnected => _connectionManager.IsConnected;
        private readonly Config _config;

        #region Constructors

        public OperationManager(IConnectionManager connectionManage, JsonSerializerSettings jsonSerializerSettings, Config config)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _connectionManager = connectionManage;
            _config = config;
        }

        public OperationManager(IConnectionManager connectionManage, JsonSerializerSettings jsonSerializerSettings)
            : this(connectionManage, jsonSerializerSettings, new Config())
        {
        }

        #endregion Constructors


        /// <summary>
        /// 
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <returns></returns>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        public string TryConnectTo(List<string> urls, CancellationToken token)
        {
            _urls = urls;

            foreach (var url in urls)
            {
                try
                {
                    var connectedTo = _connectionManager.ConnectTo(url, token);
                    if (string.IsNullOrEmpty(connectedTo))
                        continue;

                    if (TryLoadConfig(token))
                        return url;
                }
                catch
                {
                    //todo nothing
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <returns></returns>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        public string RetryConnect(CancellationToken token)
        {
            return TryConnectTo(_urls, token);
        }

        /// <summary>
        /// Create and Broadcast a transaction to the network
        /// 
        /// The transaction will be checked for validity in the local database prior to broadcasting. If it fails to apply locally, an error will be thrown and the transaction will not be broadcast.
        /// </summary>
        /// <param name="userPrivateKeys"></param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <param name="operations"></param>
        /// <returns></returns>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        public JsonRpcResponse BroadcastOperations(IEnumerable<byte[]> userPrivateKeys, CancellationToken token, params BaseOperation[] operations)
        {
            var prop = GetDynamicGlobalProperties(token);
            if (prop.IsError)
                return prop;

            var transaction = CreateTransaction(prop.Result, userPrivateKeys, token, operations);
            return BroadcastTransaction(transaction, token);
        }

        /// <summary>
        /// Create and Broadcast a transaction to the network
        /// 
        /// This call will not return until the transaction is included in a block. 
        /// </summary>
        /// <param name="userPrivateKeys"></param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <param name="operations"></param>
        /// <returns></returns>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        public JsonRpcResponse BroadcastOperationsSynchronous(IEnumerable<byte[]> userPrivateKeys, CancellationToken token, params BaseOperation[] operations)
        {
            var prop = GetDynamicGlobalProperties(token);
            if (prop.IsError)
                return prop;

            var transaction = CreateTransaction(prop.Result, userPrivateKeys, token, operations);
            return BroadcastTransactionSynchronous(transaction, token);
        }

        /// <summary>
        /// Execute custom user method
        /// Возвращает TRUE если транзакция подписана правильно
        /// </summary>
        /// <param name="userPrivateKeys"></param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <param name="testOps"></param>
        /// <returns></returns>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        public JsonRpcResponse<bool> VerifyAuthority(IEnumerable<byte[]> userPrivateKeys, CancellationToken token, params BaseOperation[] testOps)
        {
            var prop = DynamicGlobalPropertyApiObj.Default;
            var transaction = CreateTransaction(prop, userPrivateKeys, token, testOps);
            return CustomGetRequest<bool>("verify_authority", token, new[] { transaction });
        }

        /// <summary>
        /// Create and execute custom json-rpc method
        /// </summary>
        /// <typeparam name="T">Custom type. JsonConvert will try to convert json-response to you custom object</typeparam>
        /// <param name="method">Sets json-rpc "method" field</param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <param name="data">Sets to json-rpc params field. JsonConvert use`s for convert array of data to string.</param>
        /// <returns></returns>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        public JsonRpcResponse<T> CustomGetRequest<T>(string method, CancellationToken token, params object[] data)
        {
            var jsonRpc = new JsonRpcRequest(_jsonSerializerSettings, method, data);
            return _connectionManager.Execute<T>(jsonRpc, token);
        }

        /// <summary>
        /// Create and execute custom json-rpc method
        /// </summary>
        /// <param name="method">Sets json-rpc "method" field</param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <param name="data">Sets to json-rpc params field. JsonConvert use`s for convert array of data to string.</param>
        /// <returns></returns>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        public JsonRpcResponse CustomGetRequest(string method, CancellationToken token, params object[] data)
        {
            var jsonRpc = new JsonRpcRequest(_jsonSerializerSettings, method, data);
            return _connectionManager.Execute(jsonRpc, token);
        }

        /// <summary>
        /// Create and execute custom json-rpc method
        /// </summary>
        /// <param name="api">Api name</param>
        /// <param name="method">Api method</param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <param name="data">Sets to json-rpc params field. JsonConvert use`s for convert array of data to string.</param>
        /// <returns></returns>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        public JsonRpcResponse CallRequest(string api, string method, object[] data, CancellationToken token)
        {
            var jsonRpc = new JsonRpcRequest(_jsonSerializerSettings, "call", new object[] { api, method, data });
            return _connectionManager.Execute(jsonRpc, token);
        }

        /// <summary>
        /// Create and execute custom json-rpc method
        /// </summary>
        /// <param name="api">Api name</param>
        /// <param name="method">Api method</param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <param name="data">Sets to json-rpc params field. JsonConvert use`s for convert array of data to string.</param>
        /// <returns></returns>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        public JsonRpcResponse<T> CallRequest<T>(string api, string method, object[] data, CancellationToken token)
        {
            var jsonRpc = new JsonRpcRequest(_jsonSerializerSettings, "call", new object[] { api, method, data });
            return _connectionManager.Execute<T>(jsonRpc, token);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyApiObj"></param>
        /// <param name="userPrivateKeys"></param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <param name="operations"></param>
        /// <returns></returns>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        public SignedTransaction CreateTransaction(DynamicGlobalPropertyApiObj propertyApiObj, IEnumerable<byte[]> userPrivateKeys, CancellationToken token, params BaseOperation[] operations)
        {
            var transaction = new SignedTransaction
            {
                ChainId = ChainId,
                RefBlockNum = (ushort)(propertyApiObj.HeadBlockNumber & 0xffff),
                RefBlockPrefix = (uint)BitConverter.ToInt32(Hex.HexToBytes(propertyApiObj.HeadBlockId), 4),
                Expiration = propertyApiObj.Time.AddSeconds(30),
                BaseOperations = operations
            };

            var msg = SerializeHelper.TransactionToMessage(transaction);
            var data = Secp256k1Manager.GetMessageHash(msg);

            foreach (var userPrivateKey in userPrivateKeys)
            {
                token.ThrowIfCancellationRequested();
                var sig = Secp256k1Manager.SignCompressedCompact(data, userPrivateKey);
                transaction.Signatures.Add(sig);
            }

            return transaction;
        }

        public virtual bool TryLoadConfig(CancellationToken token)
        {
            var resp = GetConfig<JObject>(token);
            if (!resp.IsError)
            {
                var conf = resp.Result;
                JToken jToken;
                conf.TryGetValue(_config.ChainFieldName, out jToken);
                if (jToken == null)
                    return false;

                var str = jToken.Value<string>();
                ChainId = Hex.HexToBytes(str);

                return true;
            }
            return false;
        }
    }
}
