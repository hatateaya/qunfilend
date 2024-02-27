using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Modules;
using Mirai.Net.Sessions;
using Mirai.Net.Sessions.Http.Concretes;
using Mirai.Net.Utils.Extensions;
using Mirai.Net.Utils.Extensions.Actions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QunFileNetdisk
{
    static class Program
    {
        public static string fileListString;
        public static List<List<List<File>>> allFileList;
        public static List<string> directorysNeedsToRead;
        public static string session;
        public static async Task Main(string[] args)
        {
            allFileList = new();
            directorysNeedsToRead = new();
            using var bot = new MiraiBot
            {
                Address = "localhost:8080",
                QQ = 2670165085,
                VerifyKey = "INITKEYhwwiKLmt"
            };
            await bot.Launch();
            var manager = bot.GetManager<MessageManager>();
            var module = new TestModule();
            bot.MessageReceived
                .WhereAndCast<GroupMessageReceiver>()
                .Subscribe(receiver =>
                {
                    module.Execute(receiver, receiver.MessageChain.First());
                });
            Console.WriteLine(HttpGet("http://localhost:8080/about"));
            VerifyRequest verifyRequest = new();
            verifyRequest.verifyKey = "INITKEYhwwiKLmt";
            VerifyResponse verifyResponse = JsonSerializer.Deserialize<VerifyResponse>(HttpPost("http://localhost:8080/verify", JsonSerializer.Serialize(verifyRequest)));
            session = verifyResponse.session;
            BindRequest bindRequest = new();
            bindRequest.qq = 2670165085;
            bindRequest.sessionKey = session;
            BindResponse bindResponse = JsonSerializer.Deserialize<BindResponse>(HttpPost("http://localhost:8080/bind", JsonSerializer.Serialize(bindRequest)));
            Console.WriteLine(bindResponse.msg);
            Console.WriteLine("ok");

            Task<bool> fileListGenTask1 = FileListGenerate("sample-qunid");
            Task<bool> fileListGenTask2 = FileListGenerate("sample-qunid");
            Task<bool> fileListGenTask3 = FileListGenerate("sample-qunid");
            Task<bool> fileListGenTask4 = FileListGenerate("sample-qunid");
            Task<bool> fileListGenTask5 = FileListGenerate("sample-qunid");
            Task<bool> fileListGenTask6 = FileListGenerate("sample-qunid");
            Task<bool> fileListGenTask7 = FileListGenerate("sample-qunid");
            Task<bool> fileListGenTask8 = FileListGenerate("sample-qunid");
            Task<bool> fileListGenTask9 = FileListGenerate("sample-qunid");
            await Task.WhenAll(fileListGenTask1, fileListGenTask2, fileListGenTask3, fileListGenTask4, fileListGenTask5, fileListGenTask6, fileListGenTask7, fileListGenTask8, fileListGenTask9);
            Console.WriteLine("ALLOK");

        }
        public static async Task<bool> FileListGenerate(string gid)
        {
            string result = "";
            await Task.Run(async () =>
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create("http://localhost:8080/file/list?target=" + gid + "&sessionKey=" + session);
                myRequest.Timeout = 1000000;
                using (WebResponse myResponse = await myRequest.GetResponseAsync())
                {
                    using (StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8))
                    {
                        result = sr.ReadToEnd();
                    }
                }
            });
            Console.WriteLine(result);
            List<int> directorysNeedsToReadInner = new();
            FileListResponse response = JsonSerializer.Deserialize<FileListResponse>(result);
            List < List < File >> groupFileList = new();
            List<File> listFile = new();
            for(int i=0; i!=response.data.Count;i++)
            {
                File file = response.data[i];
                if (!file.isDirectory)
                {
                    listFile = new();
                    listFile.Add(file);
                    groupFileList.Add(listFile);
                }
                else 
                {
                    listFile = new();
                    listFile.Add(file);
                    groupFileList.Add(listFile);
                    directorysNeedsToReadInner.Add(i);
                }
            }
            Console.WriteLine("thisOK");
            int gnum = allFileList.Count;
            foreach(int directory in directorysNeedsToReadInner)
            {
                directorysNeedsToRead.Add(gnum+"."+directory);
            }
            allFileList.Add(groupFileList);
            return true;

        }
        static string HttpGet(string url)
        {
            var request = WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 1000000;
            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();
            using var reader = new StreamReader(webStream);
            var data = reader.ReadToEnd();
            return data;
        }
        static string HttpPost(string url ,string json)
        {
            var request = WebRequest.Create(url);
            request.Method = "POST";
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            using var reqStream = request.GetRequestStream();
            reqStream.Write(byteArray, 0, byteArray.Length);
            using var response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            using var respStream = response.GetResponseStream();
            using var reader = new StreamReader(respStream);
            string data = reader.ReadToEnd();
            return data;
        }
        public class Contact
        {
            public long id { get; set; }
            public string name { get; set; }
            public string permission { get; set; }
        }
        public class File
        {
            public string name { get; set; }
            public string id { get; set; }
            public string path { get; set; }
            public File parent { get; set; } // 弃用
            public Contact contact { get; set; }
            public bool isFile { get; set; }
            public bool isDictionary { get; set; }
            public bool isDirectory { get; set; }
            public int size { get; set; }
            public string downloadInfo { get; set; }
        }
        public class FileListResponse
        {
            public int code { get; set; }
            public string msg { get; set; }
            public List<File> data { get; set; }
        }
        public class BindRequest
        {
            public string sessionKey { get; set; }
            public long qq { get; set; }
        }
        public class BindResponse
        {
            public int code { get; set; }
            public string msg { get; set; }
        }
        public class VerifyRequest
        {
            public string verifyKey { get; set; }
        }
        public class VerifyResponse
        {
            public int code { get; set; }
            public string session { get; set; }
        }
    }
    public class TestModule : IModule
    {
        public async void Execute(
          MessageReceiverBase @base,
          MessageBase executeMessage
        )
        {
            if (@base is GroupMessageReceiver receiver)
            {
                if (receiver.Sender.Id == 212471286.ToString())
                {
                    await receiver.SendGroupMessage("Hello, World".Append());
                }
            }
        }
        public bool? IsEnable { get; set; }
    }
}
