﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Ditch.Golos.Tests
{
    [TestFixture]
    public class OperationManagerConnectionTest
    {
        [Test]
        public void WssNodeTest()
        {
            var urls = new List<string> {
                "wss://ws.golos.io",
                "wss://ws.testnet.golos.io",
                "wss://golosd.steepshot.org"
            };

            var jss = GetJsonSerializerSettings();
            var connectionManager = new WebSocketManager(jss);
            var manager = new OperationManager(connectionManager, jss);

            var sw = new Stopwatch();
            //test all urls
            for (var i = 0; i < urls.Count; i++)
            {
                var buf = new List<string>() { urls[i] };
                sw.Restart();

                var url = manager.TryConnectTo(buf, CancellationToken.None);

                sw.Stop();

                Console.WriteLine($"{i} {(manager.IsConnected ? "conected" : "Not connected")} to {urls[i]} {sw.ElapsedMilliseconds}");

                if (manager.IsConnected)
                {
                    Assert.IsNotNull(manager.ChainId, "ChainId null");
                }
            }
        }

        [Test]
        public void HttpsNodeTest()
        {
            var urls = new List<string> { "https://public-ws.golos.io", "https://golosd.steepshot.org" };

            var jss = GetJsonSerializerSettings();
            var connectionManager = new HttpManager(jss);
            var manager = new OperationManager(connectionManager, jss);

            var sw = new Stopwatch();
            //test all urls
            for (var i = 0; i < urls.Count; i++)
            {
                var buf = new List<string>() { urls[i] };
                sw.Restart();

                var url = manager.TryConnectTo(buf, CancellationToken.None);

                sw.Stop();

                Console.WriteLine($"{i} {(manager.IsConnected ? "conected" : "Not connected")} to {urls[i]} {sw.ElapsedMilliseconds}");

                if (manager.IsConnected)
                {
                    Assert.IsNotNull(manager.ChainId, "ChainId null");
                }
            }
        }

        [Test]
        public async Task TryConnectToHttpsTest()
        {
            var urls = new List<string> { "https://public-ws.golos.io", "https://golosd.steepshot.org" };

            var jss = GetJsonSerializerSettings();
            var manager = new OperationManager(new HttpManager(jss), jss);

            var sw = new Stopwatch();
            for (var i = 0; i < 5; i++)
            {
                sw.Restart();
                var url = manager.TryConnectTo(urls, CancellationToken.None);
                sw.Stop();
                if (manager.IsConnected)
                {
                    Console.WriteLine($"{i} conected to {url} {sw.ElapsedMilliseconds}");
                    Assert.IsNotNull(manager.ChainId, "ChainId null");
                    await Task.Delay(3000);
                }
                else
                {
                    Console.WriteLine($"{i} not conected {sw.ElapsedMilliseconds}");
                }
            }
        }

        [Test]
        public async Task TryConnectToWssTest()
        {
            var urls = new List<string> { "wss://ws.golos.io" };

            var jss = GetJsonSerializerSettings();
            var manager = new OperationManager(new WebSocketManager(jss), jss);

            var sw = new Stopwatch();
            for (var i = 0; i < 5; i++)
            {
                Console.WriteLine($"{i} conect to golos.");
                sw.Restart();
                var url = manager.TryConnectTo(urls, CancellationToken.None);
                sw.Stop();
                if (manager.IsConnected)
                {
                    Console.WriteLine($"{i} conected to {url} {sw.ElapsedMilliseconds}");
                    Assert.IsNotNull(manager.ChainId, "ChainId null");
                    await Task.Delay(3000);
                }
                else
                {
                    Console.WriteLine($"{i} not conected to {url} {sw.ElapsedMilliseconds}");
                }
            }
        }

        protected static JsonSerializerSettings GetJsonSerializerSettings()
        {
            var rez = new JsonSerializerSettings
            {
                DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK",
                Culture = CultureInfo.InvariantCulture
            };
            return rez;
        }
    }
}
