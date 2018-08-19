using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

/*
 * 实现IE弹谷歌，谷歌弹ie
 * 作者：罗鹏远
 * 创建时间：2018-08-05 19:57:00
 */
namespace openBrowser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string chromePath = "";
            string[] softName = new string[2] { "IEXPLORE", "chrome" };
            string[] fileName = new string[2] { "openIE", "openChrome" };
            //生成配置文件并运行
            for (int i = 0; i < fileName.Length; i++)
            {
                //获取浏览器安装路径
                chromePath = getFilePath(softName[i]);
                if (chromePath.Equals("")) {
                    this.Close();
                }
                //拼接安装路径，用于bat文件
                string chromePathStr = "";
                int index = 0;
                foreach (string str in chromePath.Split(new char[1] { '\\' }))
                {
                    if (index == 0)
                    {
                        chromePathStr += str;
                    }
                    else
                    {
                        chromePathStr += "\\\\" + str;
                    }
                    index++;
                }

                //创建配置文件
                createOpenFiles(fileName[i], chromePath, chromePathStr, softName[i]);

                //注册regedit
                runBatFile(fileName[i]);
            }
            //关闭当前窗体程序
            this.Close();
        }

        /*
         * 获取程序安装路径
         */
        private string getFilePath(string softName)
        {
            try
            {
                string strKeyName = string.Empty;
                string softPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\";
                RegistryKey regKey = Registry.LocalMachine;
                RegistryKey regSubKey = regKey.OpenSubKey(softPath + softName + ".exe", false);

                object objResult = regSubKey.GetValue(strKeyName);
                RegistryValueKind regValueKind = regSubKey.GetValueKind(strKeyName);
                if (regValueKind == Microsoft.Win32.RegistryValueKind.String)
                {
                    return objResult.ToString();
                }
            }
            catch
            {
                String err = "";
                if (softName.Equals("chrome"))
                {
                    string userPath = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    userPath += @"\Google\Chrome\Application\chrome.exe";
                    if (File.Exists(userPath))
                    {
                        return userPath;
                    }
                    else if (File.Exists(@"C:\Program Files (x86)\Google\Chrome\chrome.exe"))
                    {
                        return @"C:\Program Files (x86)\Google\Chrome\chrome.exe";
                    }
                    else
                    {
                        MessageBox.Show("未检测到谷歌浏览器！");
                    }
                }
                else if(softName.Equals("IEXPLORE"))
                {
                    MessageBox.Show("IE浏览器目录可能被更改，请联系技术人员还原默认路径！");
                }
                Environment.Exit(0);
            }
            return "";
        }

        /*
         * 创建配置文件
         */
        private void createOpenFiles(string fileName, string browserPath, string browserPathStr, string softName)
        {
            //创建文件夹
            String sPath = "C:\\openBrowser";
            if (!Directory.Exists(sPath))
            {
                Directory.CreateDirectory(sPath);
            }

            // 创建open.reg文件 
            FileStream fsReg = new FileStream(sPath + "\\" + fileName + ".reg", FileMode.OpenOrCreate, FileAccess.ReadWrite); //可以指定盘符，也可以指定任意文件名，还可以为word等文件
            StreamWriter swReg = new StreamWriter(fsReg); // 创建写入流
            // 写入
            swReg.WriteLine("Windows Registry Editor Version 5.00 ");
            swReg.WriteLine("[HKEY_CLASSES_ROOT\\" + fileName + "]");
            swReg.WriteLine("@= \"URL: Alert Protocol\"");
            swReg.WriteLine("\"URL Protocol\" = \"\"");
            swReg.WriteLine("[HKEY_CLASSES_ROOT\\" + fileName + "\\DefaultIcon]");
            swReg.WriteLine("@= \"" + softName + ".exe, 1\"");
            swReg.WriteLine("[HKEY_CLASSES_ROOT\\" + fileName + "\\shell]");
            swReg.WriteLine("[HKEY_CLASSES_ROOT\\" + fileName + "\\shell\\open] ");
            swReg.WriteLine("[HKEY_CLASSES_ROOT\\" + fileName + "\\shell\\open\\command]  ");
            swReg.WriteLine("@=\"cmd /c set m=%1 & \\\"C:\\\\openBrowser\\\\" + fileName + ".bat\\\" %%m%% & exit\"");
            swReg.Close(); //关闭文件

            // 创建open.bat文件 
            FileStream fsBat = new FileStream(sPath + "\\" + fileName + ".bat", FileMode.OpenOrCreate, FileAccess.ReadWrite); //可以指定盘符，也可以指定任意文件名，还可以为word等文件
            StreamWriter swBat = new StreamWriter(fsBat); // 创建写入流
            // 写入
            swBat.WriteLine("@echo off");
            swBat.WriteLine("set m=%m:" + fileName + ":=%");
            swBat.WriteLine("set m=\"%m:separator=&%\"");
            swBat.WriteLine("start \"\" \"" + browserPathStr + "\" %m%");
            swBat.WriteLine("exit");
            swBat.Close(); //关闭文件

            // 创建open.bat文件 
            FileStream fsRunBat = new FileStream(sPath + "\\" + fileName + "Run.bat", FileMode.OpenOrCreate, FileAccess.ReadWrite); //可以指定盘符，也可以指定任意文件名，还可以为word等文件
            StreamWriter swRunBat = new StreamWriter(fsRunBat); // 创建写入流
            swRunBat.WriteLine("REGEDIT /S C:\\\\openBrowser\\\\" + fileName + ".reg"); // 写入
            swRunBat.Close(); //关闭文件

        }

        /**
         * 运行run.bat进行注册信息
         */
        public void runBatFile(string fileName)
        {
            Process proc = null;
            try
            {
                string targetDir = string.Format(@"C:\openBrowser\");//this is where testChange.bat lies
                proc = new Process();
                proc.StartInfo.WorkingDirectory = targetDir;
                proc.StartInfo.FileName = fileName + "Run.bat";
                proc.StartInfo.Arguments = string.Format("10");//this is argument
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;//这里设置DOS窗口不显示，经实践可行
                proc.Start();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
            }
        }
    }
}
