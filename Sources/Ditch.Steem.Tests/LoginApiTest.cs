using System;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
namespace Ditch.Steem.Tests
{
    [TestFixture]
    public class LoginApiTest : BaseTest
    {
        [Test]
        public void login()
        {
            var resp = Api.Login(User.Login, User.Wif, CancellationToken.None);
            Console.WriteLine(resp.Error);
            Assert.IsFalse(resp.IsError);
            Console.WriteLine(JsonConvert.SerializeObject(resp.Result));

            var obj = Api.CustomGetRequest<JObject>("login", CancellationToken.None, User.Login, User.Wif);
            TestPropetries(resp.Result.GetType(), obj.Result);
        }

        [Test]
        public void get_api_by_name()
        {
            var resp = Api.GetApiByName(string.Empty, CancellationToken.None);
            Console.WriteLine(resp.Error);
            Assert.IsFalse(resp.IsError);
            Console.WriteLine(JsonConvert.SerializeObject(resp.Result));

            var obj = Api.CustomGetRequest<JObject>("get_api_by_name", CancellationToken.None, string.Empty);
            TestPropetries(resp.Result.GetType(), obj.Result);
        }

        [Test]
        public void get_version()
        {
            var resp = Api.GetVersion(CancellationToken.None);
            Console.WriteLine(resp.Error);
            Assert.IsFalse(resp.IsError);
            Console.WriteLine(JsonConvert.SerializeObject(resp.Result));

            var obj = Api.CustomGetRequest<JObject>("get_version", CancellationToken.None);
            TestPropetries(resp.Result.GetType(), obj.Result);
        }

//        [Test]
//        public void on_api_startup()
//        {
//            var resp = Api.OnApiStartup();
//            Console.WriteLine(resp.Error);
//            Assert.IsFalse(resp.IsError);
//            Console.WriteLine(JsonConvert.SerializeObject(resp.Result));

//            var obj = Api.CustomGetRequest<JObject>("on_api_startup");
//            TestPropetries(resp.Result.GetType(), obj.Result);
//        }
    }
}
