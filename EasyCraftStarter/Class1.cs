using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace EasyCraftStarter
{
    public static class Starter
    {
        // !!! 请修改下列信息 !!!
        public static Dictionary<string, string> StarterInfo = new()
        {
            { "name", "EasyCraft 默认开服器" }, // 开服器友好名称
            { "id", "top.easycraft.starter.basic" }, // 开服器 ID
            { "version", "1.0.0" }, // 版本号
            { "description", "介绍" }, // 简介信息
            { "author", "Kengwang" } // 作者
        };

        public static Dictionary<int, Process> Processes = new();

        public static Dictionary<string, string> InitializeStarter()
        {
            // 你可以在此处做一点启动前的准备工作, 不过你需要在其他线程进行.
            // 此方法应当快速返回
            return StarterInfo;
        }

        public static bool OnServerInput(dynamic Server, string input)
        {
            if (Processes.ContainsKey(Server.BaseInfo.Id))
            {
                ((Process)Processes[Server.BaseInfo.Id]).StandardInput.WriteLine(input);
                return true;
            }

            return false;
        }

        public static bool ServerStart(dynamic Server, string program, string argument, Encoding inputenc,
            Encoding outputenc)
        {
            try
            {
                Server.StatusInfo.OnConsoleOutput("欢迎使用 EasyCraft 默认开服器");
                var p = new Process();
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.StandardInputEncoding = inputenc;
                p.StartInfo.StandardOutputEncoding = outputenc;
                p.StartInfo.StandardErrorEncoding = outputenc;
                p.StartInfo.ErrorDialog = false;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WorkingDirectory = Server.ServerDir;
                p.StartInfo.FileName = program;
                p.StartInfo.Arguments = argument;
                p.ErrorDataReceived += (_, args) => Server.StatusInfo.OnConsoleOutput(args.Data, true);
                p.OutputDataReceived += (_, args) => Server.StatusInfo.OnConsoleOutput(args.Data);
                Processes[Server.BaseInfo.Id] = p;
                p.Start();
                Server.StatusInfo.Status = 2;
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                Task.Run(async () =>
                {
                    await p.WaitForExitAsync();
                    Server.StatusInfo.OnConsoleOutput("服务器进程结束");
                    Server.StatusInfo.Status = 0;
                });
                return true;
            }
            catch (Exception e)
            {
                Server.StatusInfo.OnConsoleOutput("启动失败: " + e.Message);
                Server.StatusInfo.Status = 0;
                return false;
            }
        }

        public static bool ServerStop(dynamic Server)
        {
            try
            {
                if (Processes.ContainsKey(Server.BaseInfo.Id))
                {
                    ((Process)Processes[Server.BaseInfo.Id]).StandardInput.WriteLine("stop");
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Server.StatusInfo.OnConsoleOutput("关闭失败: " + e.Message);
                return false;
            }
        }
    }
}