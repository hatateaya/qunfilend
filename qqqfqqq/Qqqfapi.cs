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

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;

/// <summary>
/// Qun QQ File API
/// </summary>
namespace Qqqfapi
{
    public class Group
    {
        public string skey;
        public string uin;
        public string gid;
        public string bkn;
        public string qid;
        public FileList fileList;
        public Group(string skeyInput, string qidInput, string gidInput)
        {
            skey = skeyInput;
            qid = qidInput;
            gid = gidInput;
            uin = "o" + qid.PadLeft(10, '0');
            bkn = GenerateBkn(skey);
            fileList = GetFileList("/");
            fileList.Init(this);
        }
        public FileList GetFileList(string folderId)
        {
            string fileListJson = HttpGet("https://pan.qun.qq.com/cgi-bin/group_file/get_file_list?src=qpan&gc=" + gid + "&bkn=" + bkn + "&folder_id=" + folderId + "&start_index=0&cnt=2147483647&filter_code=0&show_onlinedoc_folder=0");
            FileList fileListInner = JsonSerializer.Deserialize<FileList>(fileListJson);
            fileListInner.Init(this);
            return fileListInner;
        }
        public string HttpGet(string url)
        {
            CookieContainer cookieContainer = new();
            cookieContainer.Add(new Cookie("skey", skey, "/", ".qq.com"));
            cookieContainer.Add(new Cookie("uin", uin, "/", ".qq.com"));
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.CookieContainer = cookieContainer;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new(response.GetResponseStream(), Encoding.UTF8);
            string content = reader.ReadToEnd();
            reader.Close();
            return HttpUtility.HtmlDecode(content);
        }
        public string HttpPost(string url, Dictionary<string, string> dic)
        {
            CookieContainer cookieContainer = new();
            cookieContainer.Add(new Cookie("skey", skey, "/", ".qq.com"));
            cookieContainer.Add(new Cookie("uin", uin, "/", ".qq.com"));
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = cookieContainer;
            StringBuilder builder = new();
            int i = 0;
            foreach (var item in dic)
            {
                if (i > 0)
                    builder.Append('&');
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }
            byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
            request.ContentLength = data.Length;
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            using StreamReader reader = new(stream, Encoding.UTF8);
            string content = reader.ReadToEnd();
            reader.Close();
            return HttpUtility.HtmlDecode(content);
        }
        static private string GenerateBkn(string skey)
        {
            int e, t, n;
            for (e = 5381,
                t = 0,
                n = skey.Length;
                t < n;
                ++t)
            {
                e += (e << 5) + skey.ToCharArray()[t];
            }
            long bkn = 2147483647 & e;
            return bkn.ToString();
        }
    }
    public class GroupList
    {
        public List<Group> groups { get; set; }
        public File GetFileFromNumberId(int groupId,int folderId,int fileId)
        {
            return groups[groupId].fileList.file_list[folderId].FileList.file_list[fileId];
        }
        public File GetFileFromNumberId(int groupId, int fileId)
        {
            return groups[groupId].fileList.file_list[fileId];
        }
        public GroupList(List<Group> groupsInput)
        {
            groups = groupsInput;
        }
    }
    public class File
    {
        public Group group { get; set; }
        public long bus_id { get; set; }
        public long create_time { get; set; }
        public string id { get; set; }
        public string modify_name { get; set; }
        public long modify_time { get; set; }
        public long modify_uin { get; set; }
        public string name { get; set; }
        public string owner_name { get; set; }
        public long owner_uin { get; set; }
        public string parent_id { get; set; }
        public long size { get; set; }
        // File is 1 and Folder is 2.
        public long type { get; set; }
        public FileList FileList;
        public string Download()
        {
            string downloadResponseJson = group.HttpGet("https://pan.qun.qq.com/cgi-bin/group_share_get_downurl?uin=" + group.qid + "&groupid=" + group.gid + "&pa=" + "%2F" + bus_id + Uri.EscapeDataString(id) + "&charset=utf-8&g_tk=" + group.bkn);
            downloadResponseJson = downloadResponseJson[10..^2];
            DownloadResponse downloadResponse = JsonSerializer.Deserialize<DownloadResponse>(downloadResponseJson);
            return downloadResponse.data.url;
        }
        public void Rename(string newName)
        {
            Dictionary<string, string> dictionary = new();
            dictionary.Add("app_id", "4");
            dictionary.Add("bkn", group.bkn);
            dictionary.Add("bus_id", bus_id.ToString());
            dictionary.Add("file_id", id);
            dictionary.Add("gc", group.gid);
            dictionary.Add("new_file_name", newName);
            dictionary.Add("parent_folder_id", parent_id);
            dictionary.Add("src", "qpan");
            group.HttpPost("https://pan.qun.qq.com/cgi-bin/group_file/rename_file", dictionary);
        }
        public void Delete()
        {
            Dictionary<string, string> dictionary = new();
            dictionary.Add("src", "qpan");
            dictionary.Add("gc", group.gid);
            dictionary.Add("bkn", group.bkn);
            dictionary.Add("bus_id", bus_id.ToString());
            dictionary.Add("file_id", id);
            dictionary.Add("app_id", "4");
            dictionary.Add("parent_folder_id", parent_id);
            dictionary.Add("file_list", "{\"file_list\":[{\"gc\":" + group.gid + ",\"app_id\":4,\"bus_id\":" + bus_id + ",\"file_id\":\"" + id + "\",\"parent_folder_id\":\"" + parent_id + "\"}]}");
            group.HttpPost("https://pan.qun.qq.com/cgi-bin/group_file/delete_file", dictionary);
        }
        public class DownloadResponseData
        {
            public string cookie { get; set; }
            public string dns { get; set; }
            public long ismember { get; set; }
            public string md5 { get; set; }
            public long ret { get; set; }
            public string sha { get; set; }
            public string sha3 { get; set; }
            public string sip { get; set; }
            public string url { get; set; }
        }
        public class DownloadResponse
        {
            public long code { get; set; }
            public DownloadResponseData data { get; set; }
            public long @default { get; set; }
            public string message { get; set; }
            public long subcode { get; set; }
        }
    }
    public class FileList
    {
        public Group group { get; set; }
        public long ec { get; set; }
        public List<File> file_list { get; set; }
        public long next_index { get; set; }
        public long open_flag { get; set; }
        public long total_cnt { get; set; }
        public long user_role { get; set; }
        public void Init(Group groupInput)
        {
            group = groupInput;
            if (file_list != null)
            {
                for (int i = 0; i != file_list.Count; i++)
                {
                    file_list[i].group = group;
                }
                if (file_list.Count != 0 && file_list[0].parent_id == "/")
                {
                    for (int i = 0; i != file_list.Count; i++)
                    {
                        if (file_list[i].type == 2)
                        {
                            file_list[i].FileList = group.GetFileList(file_list[i].id);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}