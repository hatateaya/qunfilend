/*
 * Copyright (c) 2021
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

// Mirai-CSharp巨大多神秘bug, 可能根本没适配Mirai的新版本协议. 官方示例都无法正常使用.
// Cocoa2才发布没一个月, 但是Mirai-CSharp过于年久失修, 还是用Cocoa罢(而且其似乎还好用些), 不过之后可能要经常性的改代码和升级版本.
// 另外为了减少发布后的大小, 没用Newtonsoft.JSON而是用了MS新加的System.Text.JSON, 但是一看Cocoa貌似需要依赖其, 草

// 急需多线程异步化
// TODO: 多群群文件整合 文本上传文件列表 上传移动支持 其他优化(例如dict) 自动获取skey
// 错误需自行debug(例如skey错误)

using Qqqfapi;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Maila.Cocoa.Framework;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;
using Maila.Cocoa.Framework.Support;
using Maila.Cocoa.Beans.Models;

namespace Qqqfqqq
{

    class Program
    {
        public static string fileListString;
        public static GroupList groupList;
        static async Task Main()
        {
            Console.WriteLine("[INFO] QqqFqqq Version: 0.8.19.1");
            Console.WriteLine("[INFO] License: AGPLv3 or Later.");
            Console.WriteLine("[INFO] Please place your Mirai things and Group things into config.json.");
            Console.WriteLine("--------------------------------------------------------------------------------");
            Config programConfig = JsonSerializer.Deserialize<Config>(System.IO.File.ReadAllText(Environment.CurrentDirectory + "\\config.json"));
            BotStartupConfig config = new(programConfig.token, programConfig.qid, programConfig.host, programConfig.port);
            var succeed = await BotStartup.Connect(config);
            if (succeed)
            {
                Console.WriteLine("[INFO] Startup OK.");
                //await BotAPI.UploadFileAndSend(sample, "\\","D:\\Downloads\\java.zip");
                await BotStartup.Disconnect();
                Console.Read();
            }
            else
            {
                Console.WriteLine("[ERROR] Startup Failed.");
            }
            /*
            List<Group> groups = new();
            for (int i = 0; i != programConfig.groups.Count; i++)
            {
                groups.Add(new(programConfig.groups[i].skey, programConfig.groups[i].qid, programConfig.groups[i].gid));
            }
            for (int i = 0; i != groups.Count; i++)
            {
                fileListString += FileListStringGenerate(groups[i].fileList,i);
            }
            groupList = new(groups);
            Console.WriteLine(fileListString);
            */
        }
        static string FileListStringGenerate(FileList fileList,int groupId)
        {
            StringBuilder stringBuilder = new();
            File file;
            File fileInner;
            for (int i = 0; i != fileList.file_list.Count; i++)
            {
                file = fileList.file_list[i];
                stringBuilder.Append((groupId+"."+i).PadLeft(5,' ') + " | ");
                stringBuilder.Append((file.size / 1048576).ToString().PadLeft(3, ' ') + "MB | ");
                stringBuilder.Append(file.name + "\n");
                if (file.type == 2 && file.FileList.file_list != null)
                {
                    for (int ii = 0; ii != file.FileList.file_list.Count; ii++)
                    {
                        fileInner = file.FileList.file_list[ii];
                        stringBuilder.Append("    " + (groupId+"."+i+"."+ii).PadLeft(9,' ') + " | ");
                        stringBuilder.Append((fileInner.size / 1048576).ToString().PadLeft(3, ' ') + "MB | ");
                        stringBuilder.Append(fileInner.name + "\n");
                    }
                }
            }
            return stringBuilder.ToString();
        }
        static public Bitmap TextToBitmap(string text, Font font, Rectangle rect, Color fontcolor, Color backColor)
        {
            Graphics g;
            Bitmap bmp;
            StringFormat format = new(StringFormatFlags.NoClip);
            if (rect == Rectangle.Empty)
            {
                bmp = new Bitmap(1, 1);
                g = Graphics.FromImage(bmp);
                SizeF sizef = g.MeasureString(text, font, PointF.Empty, format);
                int width = (int)(sizef.Width + 1);
                int height = (int)(sizef.Height + 1);
                rect = new Rectangle(0, 0, width, height);
                bmp.Dispose();

                bmp = new Bitmap(width, height);
            }
            else
            {
                bmp = new Bitmap(rect.Width, rect.Height);
            }
            g = Graphics.FromImage(bmp);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.FillRectangle(new SolidBrush(backColor), rect);
            g.DrawString(text, font, Brushes.Black, rect, format);
            return bmp;
        }
        private class GroupsItem
        {
            public string skey { get; set; }
            public string qid { get; set; }
            public string gid { get; set; }
        }
        private class Config
        {
            public List<GroupsItem> groups { get; set; }
            public string token { get; set; }
            public long qid { get; set; }
            public string host { get; set; }
            public int port { get; set; }
        }
    }
    [BotModule]
    public class Message : BotModuleBase
    {
        protected override bool OnMessage(MessageSource src, QMessage msg)
        {
            if (msg.PlainText != null)
            {
                Console.WriteLine("[INFO] " + DateTime.Now.ToString() + "|" + src.User.Id.ToString() + "|" + src.Group.Id.ToString() + "|" + msg.PlainText);
                if (msg.PlainText.ToLower().StartsWith("qqqfqqq"))
                {
                    src.Send("QQQFQQQ(基于QQ群文件的文件相关QQ机器人)(测试) \nAGPLv3 or later许可证(需要源码请联系该账号) \n程序依赖:Maila.Cocoa.Framework,Mirai,Mirai-api-http,qqqfapi \n需要文件列表请发送“文件列表” \n需要下载请发送“下载”加文件的4字符id. ");
                }
                if (msg.PlainText.StartsWith("文件列表"))
                {
                    string text = Program.fileListString;
                    Bitmap bmp = Program.TextToBitmap(text, new Font(new FontFamily("Microsoft Sans Serif"), 8.25f), Rectangle.Empty, Color.Black, Color.White);
                    bmp.Save(System.IO.Path.GetTempPath() + "\\save.jpg", ImageFormat.Jpeg);
                    //src.SendImage(@"D:\Pictures\1.jpg");
                    src.SendImage(System.IO.Path.GetTempPath() + "\\save.jpg");
                }
                if (msg.PlainText.StartsWith("下载"))
                {
                    string[] numberIdStrings = msg.PlainText[2..].Split('.');
                    File file = new();
                    if (numberIdStrings.Length == 2)
                    {
                        file = Program.groupList.GetFileFromNumberId(int.Parse(numberIdStrings[0]),int.Parse(numberIdStrings[1]));
                    }
                    else if (numberIdStrings.Length == 3)
                    {
                        file = Program.groupList.GetFileFromNumberId(int.Parse(numberIdStrings[0]), int.Parse(numberIdStrings[1]), int.Parse(numberIdStrings[2]));
                    }
                    else
                    {
                        src.Send("参数错误");
                    }
                    if (file.id != null)
                    {
                        src.Send(file.name);
                        src.Send(file.Download());
                    }
                    else
                    {
                        src.Send("出现错误");
                    }
                }
            }
            return true;
        }
    }
}