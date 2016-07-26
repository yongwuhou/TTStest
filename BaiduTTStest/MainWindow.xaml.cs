using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BaiduTTStest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        string AppID = "8000641";
        string APIKey = "pzUfQDz8XODiGyOGNHzSLAa2";
        string SecretKey = "400ba453a3f0e159c3d8a7e99801c17a";
        string AccessToken = "";
      
        //播放MP3
        private const int NULL = 0, ERROR_SUCCESS = NULL;
        [DllImport("WinMm.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int mciSendString(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);

        public MainWindow()
        {
            InitializeComponent();
            AccessToken = getStrAccess(APIKey, SecretKey);
        }



        public string getStrAccess(string para_API_key, string para_API_secret_key)
        {

            //方法参数说明:            
            //para_API_key:API_key         
            //para_API_secret_key         
            //方法返回值说明:            
            //百度认证口令码,access_token            
            string access_html = null;
            string access_token = null;
            string getAccessUrl = "https://openapi.baidu.com/oauth/2.0/token?grant_type=client_credentials" + "&client_id=" + para_API_key + "&client_secret=" + para_API_secret_key;
            try
            {
                HttpWebRequest getAccessRequest = WebRequest.Create(getAccessUrl) as HttpWebRequest;
                //getAccessRequest.Proxy = null;                
                getAccessRequest.ContentType = "multipart/form-data";
                getAccessRequest.Accept = "*/*";
                getAccessRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727)";
                getAccessRequest.Timeout = 30000;//30秒连接不成功就中断                 
                getAccessRequest.Method = "post";
                HttpWebResponse response = getAccessRequest.GetResponse() as HttpWebResponse;
                using (StreamReader strHttpComback = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    access_html = strHttpComback.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                App.log.Error(ex);
            }

            if (access_html.Contains("access_token"))
            {
                //Regex reg = new Regex("\"access_token\":\"(.+)\",\"session_key\"");
                //Match match = reg.Match(access_html);
                //access_token = match.Groups[1].Value;
                JObject jo = JObject.Parse(access_html);
                access_token = jo["access_token"].ToString();//得到返回的toke
                App.log.Debug("AccessToken:" + access_token);
            }            
            return access_token;
        }

         public string getStrText(string para_API_id, string para_API_access_token,string para_API_language, string para_API_record,string para_format,string para_Hz)
        {
            //方法参数说明:
            //para_API_id: API_id (ID)
            //para_API_access_token (access_token口令)
            //para_API_language (要识别的语言,zh,en,ct)
            //para_API_record(语音文件的路径)
            //para_format(语音文件的格式)
            //para_Hz(语音文件的采样率 16000或者8000)
             
            //该方法返回值:
            //该方法执行正确返回值是语音翻译的文本,错误是错误号,可以去看百度语音文档,查看对应错误
             
            string strText = null;
            string error = null;
            string strJSON;
            FileInfo fi = new FileInfo(para_API_record);
            FileStream fs = new FileStream(para_API_record, FileMode.Open);
            byte[] voice = new byte[fs.Length];
            fs.Read(voice, 0, voice.Length);
            fs.Close();
 
            string getTextUrl = "http://vop.baidu.com/server_api?lan="+ para_API_language + "&cuid="+ para_API_id + "&token="+ para_API_access_token;
            HttpWebRequest getTextRequst = WebRequest.Create(getTextUrl) as HttpWebRequest;
 
           /* getTextRequst.Proxy = null;
            getTextRequst.ServicePoint.Expect100Continue = false;
            getTextRequst.ServicePoint.UseNagleAlgorithm = false;
            getTextRequst.ServicePoint.ConnectionLimit = 65500;
            getTextRequst.AllowWriteStreamBuffering = false;*/
 
            getTextRequst.ContentType = "audio /"+para_format+";rate="+para_Hz;
            getTextRequst.ContentLength = fi.Length;
            getTextRequst.Method = "post";
            getTextRequst.Accept = "*/*";
            getTextRequst.KeepAlive = true;
            getTextRequst.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727)";
            getTextRequst.Timeout = 30000;//30秒连接不成功就中断
            using(Stream writeStream = getTextRequst.GetRequestStream())
            {
                writeStream.Write(voice, 0, voice.Length);
            }
 
            HttpWebResponse getTextResponse = getTextRequst.GetResponse() as HttpWebResponse;      
            using(StreamReader strHttpText = new StreamReader(getTextResponse.GetResponseStream(), Encoding.UTF8))
            {
                strJSON = strHttpText.ReadToEnd();
            }
                App.log.Debug(strJSON);
                JObject jsons = JObject.Parse(strJSON);//解析JSON
                if(jsons["err_msg"].Value<string>() == "success.")
                {                
                    strText = jsons["result"][0].ToString();
                    return strText;
                }
                else
                {
                    error = jsons["err_no"].Value<string>() + jsons["err_msg"].Value<string>();
                    return error;
                }
             
        }



         private void button1_Click(object sender, RoutedEventArgs e)
         {
             string lan = "zh";    //语言中文
             string per = "0";     //发音人，0为女声，1为男声，默认女声
             string ctp = "1";     //客户端，web端填1
             string spd = "4";     //语速0-9，默认5
             string pit = "6";       //音调0-9，默认5
             string vol = "9";     //音量0-9，默认5
             string cuid = "100869878456746147846416";    //用户唯一标识
             string tex = "远去的山河沉寂，恋过的风景如昔。" +
                                         "苍何斩落了情迷，生死轻付了别离。" +
                                         "捣一脉相思成泥，沐四海悲风无迹。" +
                                         "往生海烟波又起，妙华镜风雪共历。";
             //string tex = "Sometimes the perfect person for you is the one you least expect.Somewhere, someday, the unexpected encounter with someone changed your life somehow.";
             string rest = "tex={0}&lan={1}&per={2}&ctp={3}&cuid={4}&tok={5}&spd={6}&pit={7}&vol={8}";

             cuid = GetMacAddress();
             if (!string.IsNullOrEmpty(textBox1.Text))
             {
                 tex = textBox1.Text;
             }
             string strUpdateData = string.Format(rest, tex, lan, per, ctp, cuid, AccessToken, spd, pit, vol);
             App.log.Debug(strUpdateData);
             HttpWebRequest req = WebRequest.Create("http://tsn.baidu.com/text2audio") as HttpWebRequest;
             req.Method = "POST";
             req.ContentType = "application/x-www-form-urlencoded";
             req.ContentLength = Encoding.UTF8.GetByteCount(strUpdateData);
             using (StreamWriter sw = new StreamWriter(req.GetRequestStream()))
                 sw.Write(strUpdateData);
             //HttpWebResponse res = req.GetResponse() as HttpWebResponse;
             //using (Stream stream = res.GetResponseStream())
             //{
             //    string strFullFileName = "test.mp3";
             //    using (FileStream fs = new FileStream(strFullFileName, FileMode.Truncate | FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
             //        stream.CopyTo(fs);

             //}
             // string tex = "你好", lan = "zh", ctp = "1", cuid = "100869878456746147846414";
             //var req = WebRequest.Create(string.Format("http://tsn.baidu.com/text2audio?tex={0}&lan={1}&cuid={2}&ctp={3}&tok={4}", tex, lan, cuid, ctp, AccessToken));
             string strFullFileName = AppDomain.CurrentDomain.BaseDirectory + "\\3.mp3";
             using (var res = req.GetResponse())
             {
                 if (res.ContentType == "audio/mp3")
                 {
                     var fs = new FileStream(strFullFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                     res.GetResponseStream().CopyTo(fs);
                     fs.Flush();
                     fs.Close();
                     MessageBox.Show("语音合成成功！");
                     if (mciSendString(string.Format("open \"{0}\" alias app", strFullFileName), null, NULL, NULL) == ERROR_SUCCESS)
                         mciSendString("play app", null, NULL, NULL);

                 }
                 else
                 {
                     //var fs = File.OpenWrite(AppDomain.CurrentDomain.BaseDirectory+"\\4.json");
                     //res.GetResponseStream().CopyTo(fs);
                     //fs.Flush();
                     //fs.Close();
                     string errmsg;
                     System.IO.Stream respStream = res.GetResponseStream();
                     using (System.IO.StreamReader reader = new System.IO.StreamReader(respStream, Encoding.UTF8))
                     {
                         errmsg = reader.ReadToEnd();
                     }
                     App.log.Error(errmsg);
                     MessageBox.Show("合成失败：" + errmsg);
                 }

             }
         }

        /// <summary>
        /// 获取读到的第一个mac地址
        /// </summary>
        /// <returns>获取到的mac地址</returns>
        public string GetMacAddress()
        {
            string _mac = string.Empty;
            NetworkInterface[] _networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in _networkInterfaces)
            {
                _mac = adapter.GetPhysicalAddress().ToString();
                if (!string.IsNullOrEmpty(_mac))
                    break;
            }

            return _mac;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            string fileName =AppDomain.CurrentDomain.BaseDirectory+ "welcome1.wav";
            string para_API_id=GetMacAddress();
            string para_API_language="zh";
            string para_format="wav";
            string para_Hz="8000";
            string result=getStrText(para_API_id,  AccessToken, para_API_language, fileName ,para_format, para_Hz);

            MessageBox.Show("识别结果：" + result);

        }

      


    }
}
