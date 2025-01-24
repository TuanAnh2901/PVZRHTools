﻿using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using ToolModData;
using static ToolModData.Modifier;

namespace PVZRHTools
{
    public partial class App : Application
    {
        static App()
        {
            DataSync = new();
        }

        public void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception + "\n" + e.Exception.InnerException + "\n" + e.Exception.InnerException?.InnerException);
            File.WriteAllText("./ModifierError.txt", e.Exception + "\n" + e.Exception.InnerException + "\n" + e.Exception.InnerException?.InnerException);
            e.Handled = true;
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (inited)
            {
                try
                {
                    DataSync.Value.SendData(new Exit());
                    Thread.Sleep(100);
                    DataSync.Value.modifierSocket.Shutdown(SocketShutdown.Both);
                    DataSync.Value.modifierSocket.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    File.WriteAllText("./ModifierError.txt", ex + "\n" + ex.InnerException + "\n" + ex.InnerException?.InnerException);
                }
            }
            inited = false;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                if (e.Args[0] == CommandLineToken)
                {
                    DataSync = new(new DataSync(Convert.ToInt32(e.Args[1])));
                    InitData = JsonSerializer.Deserialize(File.ReadAllText("./PVZRHTools/InitData.json"), InitDataSGC.Default.InitData);
                }
                else
                {
                    Shutdown();
                }
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("请直接启动游戏本体，修改窗口不允许单独启动。");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Environment.Exit(0);
            }
        }

        public static Lazy<DataSync> DataSync { get; set; }

        public static InitData? InitData { get; set; }

        public static bool inited = false;
    }
}