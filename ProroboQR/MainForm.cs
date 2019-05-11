using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProroboQR
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Initial();
        }

        private void programTextBox_TextChanged(object sender, EventArgs e)
        {
            //もし、文字末尾が\nだったら、反映する。
            if (programTextBox.Text.Length > 0)
            {
                if (programTextBox.Text.Substring(programTextBox.Text.Length - 1, 1) == "\n")
                {
                    if (programTextBox.Text.Contains("!CLEAR"))
                    {
                        Initial();
                        return;
                    }
                    var commmandsList = new List<string>();
                    commmandsList.AddRange(programTextBox.Text.Split('\n'));
                    commmandsList.RemoveAll(s => s.Length == 0);

                    if (commmandsList.Last().Contains("!TRANSFER"))
                    {
                        if (programTextBox.Text.Contains("FINISH"))
                        {
                            transfer(programTextBox.Text);
                        }
                        else
                        {
                            transfer(programTextBox.Text + "\nFINISH");
                        }
                    }
                    ChangeCards(commmandsList);
                    programTextBox.Select(programTextBox.Text.Length, 0);
                }
            }

        }

        private async void transfer(string code)
        {
            int exitCode;
            //code保存
            try
            {
                using (var tw = new System.IO.StreamWriter(Application.StartupPath + "//temp.prbs"))
                {
                    tw.Write(code);
                    tw.Close();
                }
            }
            catch
            {
                SetMsgLabel("コードのファイル書き込みに失敗しました。");
                return;
            }
            try
            {
                using (var p = new System.Diagnostics.Process())
                {
                    p.StartInfo.FileName = Application.StartupPath + "\\cprb\\cprb.exe";
                    p.StartInfo.Arguments = Application.StartupPath + "\\temp.prbs /s";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardInput = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();
                    await Task.Run( () => p.WaitForExit());
                    logTextBox.Text += p.StandardOutput.ReadToEnd();
                    exitCode = p.ExitCode;
                    if (exitCode == 0)
                    {
                        SetMsgLabel("プログラム転送成功。");
                    }
                    else if (exitCode == 1)
                    {
                        if (isReConCehckBox.Checked)
                        {
                            SetMsgLabel("接続失敗しました。3秒後に再接続します。");
                            await Task.Delay(3000);
                            if (programTextBox.Text.Contains("FINISH"))
                            {
                                transfer(programTextBox.Text);
                            }
                            else
                            {
                                transfer(programTextBox.Text + "\nFINISH");
                            }
                        }
                        SetMsgLabel("プロロボとの接続に失敗しました。");
                    }
                    else if (exitCode == 2)
                    {
                        Initial();
                        SetMsgLabel("スクリプトエラーが発生しました。");
                    }
                    else
                    {
                        SetMsgLabel("不明なエラーが発生しました。");
                    }
                }

            }
            catch
            {
                SetMsgLabel("不明なエラーが発生しました。ログを確認してください。");
            }
            logTextBox.Select(logTextBox.Text.Length, 0);
            logTextBox.ScrollToCaret();
        }

        private void Initial()
        {
            programTextBox.Text = "START_UNSAFE\n";
            programTextBox.Text += "M = " + moveTimeNumericUpDown.Value + "\n";
            programTextBox.Text += "T = " + twistTimeNumericUpDown.Value + "\n";
            programTextBox.Select(programTextBox.Text.Length, 0);
            programTextBox.Select();
        }

        private void ChangeCards(List<string> commandsList)
        {
            var cards = new List<PictureBox> { cardPictureBox0, cardPictureBox1 ,cardPictureBox2,
                                                cardPictureBox3, cardPictureBox4, cardPictureBox5,
                                                cardPictureBox6, cardPictureBox7};
            foreach (PictureBox p in cards)
            {
                p.Image = null;
            }
            var i = 0;
            var isLabeled = false;
            var isStarted = false;
            var isJumped = false;
            var isFinished = false;
            var repeatCount = 0;
            var newCommandsList = new List<string>();
            foreach (string s in commandsList)
            {
                if (s.Contains("START"))
                {
                    if (isStarted)
                    {
                        //すでにSTART検知->無効
                        i--;
                    }
                    else
                    {
                        cards[i].Image = ProroboQR.Properties.Resources.START;
                        newCommandsList.Add(s);
                        isStarted = true;
                    }
                }
                else if (s.Contains("STRAIGHT"))
                {
                    cards[i].Image = ProroboQR.Properties.Resources.STRAIGHT;
                    newCommandsList.Add(s);
                }
                else if (s.Contains("BACK"))
                {
                    cards[i].Image = ProroboQR.Properties.Resources.BACK;
                    newCommandsList.Add(s);
                }
                else if (s.Contains("TWIST R"))
                {
                    cards[i].Image = ProroboQR.Properties.Resources.TWIST_R;
                    newCommandsList.Add(s);
                }
                else if (s.Contains("TWIST L"))
                {
                    cards[i].Image = ProroboQR.Properties.Resources.TWIST_L;
                    newCommandsList.Add(s);
                }
                else if (s.Contains("FINISH"))
                {
                    cards[i].Image = ProroboQR.Properties.Resources.END;
                    newCommandsList.Add(s);
                    isFinished = true;
                }
                else if (s.Contains("LABEL"))
                {
                    if (isLabeled)
                    {
                        //すでにLABEL検知->無効
                        i--;
                    }
                    else
                    {
                        cards[i].Image = ProroboQR.Properties.Resources.LABEL;
                        newCommandsList.Add(s);
                        isLabeled = true;
                    }
                }
                else if (s.Contains("REPEAT 3"))
                {
                    repeatCount++;
                    cards[i].Image = ProroboQR.Properties.Resources.REPEAT_3;
                    newCommandsList.Add(s);
                }
                else if (s.Contains("REPEAT 5"))
                {
                    repeatCount++;
                    cards[i].Image = ProroboQR.Properties.Resources.REPEAT_5;
                    newCommandsList.Add(s);
                }
                else if (s.Contains("RETURN"))
                {
                    if (repeatCount == 0)
                    {
                        //REPEATされていない->無効
                        i--;
                    }
                    else
                    {
                        cards[i].Image = ProroboQR.Properties.Resources.RETURN;
                        newCommandsList.Add(s);
                        repeatCount--;
                    }
                }
                else if (s.Contains("JUMP"))
                {
                    cards[i].Image = ProroboQR.Properties.Resources.JUMP;
                    newCommandsList.Add(s);
                    isJumped = true;
                }
                else
                {
                    newCommandsList.Add(s);
                    i--;
                }
                i++;
                if (i > 6)
                {
                    //6命令以上で抜ける。
                    break;
                }
            }
            //確認
            if (!isFinished)
            {
                cards[7].Image = ProroboQR.Properties.Resources.END;
            }
            if (isJumped && !isLabeled)
            {
                //ラベル未定義ジャンプ
                Initial();
                SetMsgLabel("★がみつかりませんでした。");
                return;
            }
            if (repeatCount != 0 && i > 6)
            {
                //Forが終わらない
                Initial();
                SetMsgLabel("くりかえしのおわりがみつかりませんでした。");
                return;
            }
            if (!isStarted)
            {
                Initial();
                SetMsgLabel("START命令が検知されませんでした。");
                return;
            }
            //再構成
            //等価かどうか
            var e = false;
            if (commandsList.Count == newCommandsList.Count)
            {
                e = commandsList.All(s => newCommandsList.Contains(s));
            }
            if (!e)
            {
                string program = "";
                foreach (string s in newCommandsList)
                {
                    program += s + "\n";
                }
                programTextBox.Text = program;
            }
        }

        private async void SetMsgLabel(string msg)
        {
            msgLabel.Text = msg;
            logTextBox.Text += msg + "\n";
            await Task.Delay(3000);
            msgLabel.Text = "";
        }

        private void transferMenuBtn_Click(object sender, EventArgs e)
        {
            if (programTextBox.Text.Contains("FINISH"))
            {
                transfer(programTextBox.Text);
            }
            else
            {
                transfer(programTextBox.Text + "\nFINISH");
            }
        }

        private async void isActiveCheckTimer_Tick(object sender, EventArgs e)
        {
            if (!programTextBox.Focused)
            {
                isActiveCheckTimer.Enabled = false;
                msgLabel.Text = "警告：TextBoxにフォーカスがありません。";
                await Task.Delay(3000);
                msgLabel.Text = "";
            }
            isActiveCheckTimer.Enabled = true;
        }
    }
}
