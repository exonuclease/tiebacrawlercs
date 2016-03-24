using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Collections;

namespace tiebacrawlercs
{
    public partial class Form1 : Form
    {
        string url;
        int pagestart;
        int pageend;
        List<string> keywords = new List<string>();
        List<string> responsebodys = new List<string>();
        List<HttpResponseMessage> ress = new List<HttpResponseMessage>();
        Hashtable freplys = new Hashtable();

        static bool fkw(List<string> kws, string html)
        {
            foreach (string kw in kws)
            {
                if (!(html.IndexOf(kw) > -1))
                    return false;
            }
            return true;
        }
        public Form1()
        {
            InitializeComponent();
            richTextBox1.ReadOnly = true;
            richTextBox1.DetectUrls = true;
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().Length < 7 || textBox1.Text.Trim().Substring(0, 7) != "http://")
            {
                MessageBox.Show("请输入正确的地址");
                textBox1.Focus();
            }
            else
                url = textBox1.Text.Trim();
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            Regex isnum = new Regex(@"\d+");
            if (isnum.IsMatch(textBox2.Text.Trim()))
            {
                pagestart = Convert.ToInt32(textBox2.Text.Trim());
            }
            else
            {
                MessageBox.Show("请输入正确的起始楼层");
                textBox2.Focus();
            }
        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            Regex isnum = new Regex(@"\d+");
            if (isnum.IsMatch(textBox3.Text.Trim()))
            {
                pageend = Convert.ToInt32(textBox3.Text.Trim());
            }
            else
            {
                MessageBox.Show("请输入正确的结束楼层");
                textBox3.Focus();
            }
        }

        private void textBox4_Leave(object sender, EventArgs e)
        {
            string[] keywordarray = textBox4.Text.Trim().Split(new Char[] { ',' });
            keywords.Clear();
            foreach (string keyword in keywordarray)
            {
                keywords.Add(keyword);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            ress.Clear();
            responsebodys.Clear();
            richTextBox1.Clear();
            freplys.Clear();
            await createtask();
        }

        private async Task createtask()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/14.14291");
            for (int i = pagestart; i <= pageend; i++)
            {
                ress.Add(await client.GetAsync(url + "?pn=" + i));
            }
            for (int i = 0; i <= pageend - pagestart; i++)
            {
                responsebodys.Add(await ress[i].Content.ReadAsStringAsync());
            }
            foreach (string responsebody in responsebodys)
            {
                Regex matchreply = new Regex(@"<div id=""post_content_\d*"" class=""d_post_content j_d_post_content "">.*?</div>");
                MatchCollection matchedreplysc = matchreply.Matches(responsebody);
                List<string> matchedreplys = new List<string>();
                foreach (Match matchedreplyc in matchedreplysc)
                {
                    matchedreplys.Add(matchedreplyc.Value);
                }
                foreach (string matchedreply in matchedreplys)
                {
                    if (fkw(keywords, matchedreply))
                    {
                        Regex mreply = new Regex(@"(?<=>).*?(?=</div>)");
                        Regex mpid = new Regex(@"(?<=post_content_).*?(?="")");
                        string pid = mpid.Match(matchedreply).Value.Trim();
                        string reply = mreply.Match(matchedreply).Value.Trim().Replace("<br>","\n");
                        reply = Regex.Replace(reply,@"\n+","\n");
                        try
                        {
                            freplys.Add(reply, pid);
                        }
                        catch(ArgumentException)
                        {

                        }
                    }
                }
            }
            foreach(string freply in freplys.Keys)
            {
                richTextBox1.Text += freply + "\n";
                richTextBox1.Text += url + "?pid=" + freplys[freply] + "&cid=0#" + freplys[freply];
                richTextBox1.Text += "\n\n";
            }
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
    }
}
