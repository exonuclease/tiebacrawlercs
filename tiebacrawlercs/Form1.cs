using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Net.Http;

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
        Dictionary<string, string> freplys = new Dictionary<string, string>();
        List<Task<string>> responsebodyts = new List<Task<string>>();
        static bool fkw(List<string> kws, string html)
        {
            foreach (var kw in kws)
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
            if (textBox1.Text.Trim().Length < 7 || (textBox1.Text.Trim().Substring(0, 7) != "http://" && textBox1.Text.Trim().Substring(0, 8) != "https://"))
            {
                MessageBox.Show("请输入正确的地址(╯‵□′)╯︵┻━┻");
                textBox1.Focus();
            }
            else
                url = textBox1.Text.Trim();
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            var isnum = new Regex(@"\d+");
            if (isnum.IsMatch(textBox2.Text.Trim()))
            {
                pagestart = Convert.ToInt32(textBox2.Text.Trim());
            }
            else
            {
                MessageBox.Show("请输入正确的起始页(╯‵□′)╯︵┻━┻");
                textBox2.Focus();
            }
        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            var isnum = new Regex(@"\d+");
            if (isnum.IsMatch(textBox3.Text.Trim()))
            {
                pageend = Convert.ToInt32(textBox3.Text.Trim());
            }
            else
            {
                MessageBox.Show("请输入正确的结束页(╯‵□′)╯︵┻━┻");
                textBox3.Focus();
            }
        }

        private void textBox4_Leave(object sender, EventArgs e)
        {
            var keywordarray = textBox4.Text.Trim().Split(new Char[] { ',' });
            keywords.Clear();
            foreach (var keyword in keywordarray)
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
            progressBar1.Minimum = pagestart;
            progressBar1.Maximum = pageend + 1;
            progressBar1.Value = pagestart;
            await createtask();
        }

        private async Task<string> downopage(string url, HttpClient client)
        {
            System.Diagnostics.Debug.Write(System.DateTime.Now.ToString() + "\n");
            var res = await client.GetAsync(url);
            var responsebody = await res.Content.ReadAsStringAsync();
            var matchreply = new Regex(@"<div id=""post_content_\d*"" class=""d_post_content j_d_post_content "" style=""display:;"">.*?</div>");
            var matchedreplysc = matchreply.Matches(responsebody);
            var matchedreplys = new List<string>();
            foreach (Match matchedreplyc in matchedreplysc)
            {
                matchedreplys.Add(matchedreplyc.Value);
            }
            foreach (var matchedreply in matchedreplys)
            {
                if (fkw(keywords, matchedreply))
                {
                    var mreply = new Regex(@"(?<=>).*?(?=</div>)");
                    var mpid = new Regex(@"(?<=post_content_).*?(?="")");
                    var pid = mpid.Match(matchedreply).Value.Trim();
                    var reply = mreply.Match(matchedreply).Value.Trim().Replace("<br>", "\n");
                    reply = Regex.Replace(reply, @"\n+", "\n");
                    try
                    {
                        freplys.Add(reply, pid);
                    }
                    catch (ArgumentException)
                    {

                    }
                }
            }
            progressBar1.Value++;
            return responsebody;
        }
        private async Task createtask()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.80 Safari/537.36");
            for (int i = pagestart; i <= pageend; i++)
            {
                responsebodyts.Add(downopage(url + "?pn=" + i, client));
            }
            foreach (var responsebodyt in responsebodyts)
            {
                responsebodys.Add(await responsebodyt);
            }
            if (freplys.Count == 0)
                richTextBox1.Text += "没有找到结果！╮(￣▽￣)╭";
            else
            {
                foreach (var freply in freplys.Keys)
                {
                    richTextBox1.Text += freply + "\n";
                    richTextBox1.Text += url + "?pid=" + freplys[freply] + "&cid=0#" + freplys[freply] + "\n\n";
                }
            }
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }
    }
}
