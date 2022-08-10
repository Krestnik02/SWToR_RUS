﻿using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Xml;
using System.Net;
using System.Data.SQLite;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System.IO.Compression;

namespace SWToR_RUS
{
    public partial class Form1 : Form
    {
        public string CurDir = AppDomain.CurrentDomain.BaseDirectory; //Путь к программе

        public string connStr_mysql = "server=" + "195.234.5.250" + //Адрес сервера (для локальной базы пишите "localhost")
                    ";user=" + "swtor" + //Имя пользователя
                    ";database=" + "swtor_ru" + //Имя базы данных
                    ";port=" + "3306" + //Порт для подключения
                    ";password=" + "KHUS86!JHksds" + //Пароль для подключения
                    ";default command timeout=18000;" +//Таймаут
                    ";pooling=false;";//Не храним соединения в пуле после закрытия приложения

        public Configuration Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None); //Доступ к конфигурации

        public int gender = 1; //Пол персонажа

        public int launch_status = 0; //Статус лаунчера игры

        public string GamePath; //Путь к игре

        public int RusInstalled; //Установлен ли русификатор

        public int RusFontsInstalled; //Установлены ли шрифты

        public string heh = "";

        public string heh1 = "";

        public string arg = "";

        public string work = "";

        public int endtable;

        public int dbverasd;

        public int lastoffes;

        public int cou;

        public uint filescount;

        public string pathxml;

        public ulong vOut;

        public ulong vOut_old;

        public int keyxml;

        public string stjk;

        public uint hash_g;

        public string my_filename;

        public int is_run = 1;//Запускаем ли приложение, 1 -да,0-нет

        public IWebDriver driver;

        private readonly ManagementEventWatcher startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));

        private ProcessStartInfo startInfo = new ProcessStartInfo();

        public Form1()
        {
            InitializeComponent();
            App_Updater();//Обновление приложения, проверка новых версий
            if (is_run == 1)//Если нет обновлений запускаем приложение
            {
                Config_Work();//Работаем с конфигурационным файлом (проверка выставление отметок в интерфейсе)
                ManagementClass managementClass = new ManagementClass("Win32_Process");//Смотрим запущен ли лаучер игры
                foreach (ManagementObject instance in managementClass.GetInstances())
                {
                    if (instance["Name"].Equals("launcher.exe"))
                    {
                        launcher_status.Text = "Лаунчер SWToR запущен";
                        launcher_status.ForeColor = Color.Green;
                        launch_status = 1;
                        break;//Лаунчер найден - прерываем перебор запущенных процессов
                    }
                }
                if (launch_status == 0)//Если лаунчер не найден - сообщаем об этом
                {
                    launcher_status.Text = "Лаунчер SWToR не запущен";
                    launcher_status.ForeColor = Color.Red;
                }
                if (GamePath == "" || launch_status == 0)
                    Install_btn.Enabled = false;
                if (GamePath != "")//Если есть путь к расположению игры
                {
                    if (File.Exists(GamePath + "launcher.exe"))//Проверяем если игра в этой папке
                    {
                        GamePathTextBox.Text = GamePath;
                        if (GamePath.ToLower().IndexOf("steamapps") > 0)//Проверяем какая версия игры (Steam или нет)
                            steam_game.Checked = true;
                        else
                            steam_game.Checked = false;
                        if (Config.AppSettings.Settings["firstrun"].Value == "1")//Если это первый запуск программы
                        { del_btn.Enabled = false; }
                        else
                        {
                            string hash_in_config = Config.AppSettings.Settings["hash"].Value;//Считываем хэши из конфига и оригинального файла
                            if (hash_in_config != "")
                            {
                                string hash_original_file;
                                hash_original_file = CalculateMD5(GamePath + "\\Assets\\swtor_main_global_1.tor");
                                if (hash_in_config == hash_original_file)//Если хэши совпадают отключаем внопку Установки
                                {
                                    if (File.Exists(GamePath + "\\Assets\\swtor_en-us_global_1_tmp.tor") || File.Exists(GamePath + "\\Assets\\swtor_main_global_1_tmp.tor"))
                                        TryFix();//Видимо русификатор некорректно завершил работу, пытаемся вернуть оригинальные файлы на место
                                    if (File.Exists(GamePath + "\\Assets\\swtor_maln_gfx_assets_1.tor") || File.Exists(GamePath + "\\swtor\\retailclient\\main_gfx_1.backup"))//Запоминаем если русификатор уже стоит
                                        RusInstalled = 1;
                                    Install_btn.Text = "Переустановить";
                                    del_btn.Enabled = true;
                                    Ins_font.Enabled = false;
                                }
                                else
                                    LogBox.AppendText("Hash не совпадает. Нажмите кнопку 'Переустановить'.\n");
                            }
                            if (File.Exists(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor_backup"))//Запоминаем если установлены только шрифты
                            {
                                RusFontsInstalled = 1;
                                Ins_font.Text = "Удалить шрифты";
                                del_btn.Enabled = false;
                            }
                        }
                        if (steam_game.Checked == true && launch_status == 1 && RusInstalled == 1)//Если версия Steam, Лаунчер запушен и русификатор установлен - подготавливаем файлы для запуска
                            Steam_Rename();
                    }
                }
                startWatch.EventArrived += StartWatch_EventArrived;//Начинаем следить за процессами, чтобы отловить игру или лаунчер
                startWatch.Start();
            }
        }

        public void StartWatch_EventArrived(object sender, EventArrivedEventArgs e) //Остлеживаем появление процессов, чтобы отловить игру или лаунчер
        {
            if (e.NewEvent.Properties["ProcessName"].Value.ToString() == "swtor.exe") //Отслеживаем запуск игры
            {
                startWatch.Stop();
                if (steam_game.Checked == false)//Если не steam версия на ходу подменяем файлы
                    Rus_for_orinal_game();//Подмена файлов для обычной версии с Bitraider'ом
            }
            if (e.NewEvent.Properties["ProcessName"].Value.ToString() == "launcher.exe")//Отслеживаем запуск лаунчера
            {
                Invoke((MethodInvoker)delegate
                {
                    launcher_status.Text = "Лаунчер SWToR запущен";
                    launcher_status.ForeColor = Color.Green;
                    if (steam_game.Checked == true && RusInstalled == 1)//Если версия Steam подготавливаем файлы для запуска
                        Steam_Rename();
                });
            }
        }

        public async void Rus_for_orinal_game()//Для обычной версии программа сама отслеживает запуск игры
        {
            ManagementClass managementClass = new ManagementClass("Win32_Process");
            foreach (ManagementObject instance in managementClass.GetInstances()) //Перебираем процессы в поисках игры
            {
                if (instance["Name"].Equals("swtor.exe")) //Собираем параметры и прекращаем процесс
                {
                    heh = instance["CommandLine"].ToString();
                    heh1 = instance["ExecutablePath"].ToString();
                    arg = heh.Substring(heh.LastIndexOf('"') + 2);
                    work = heh1.Remove(heh1.Length - 9);
                    instance.InvokeMethod("Terminate", null);
                    break;
                }
            }
            if (ServiceController.GetServices().FirstOrDefault((ServiceController s) => s.ServiceName == "BRSptStub") != null) //Отключаем Битрейдер
            {
                ServiceController serviceController = new ServiceController("BRSptStub");
                if (serviceController.Status.Equals(ServiceControllerStatus.Running))
                {
                    serviceController.Stop();
                    serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                }
            }
            foreach (ManagementObject instance2 in managementClass.GetInstances()) //Отключаем Битрейдер
            {
                if (instance2["Name"].Equals("brwc.exe"))
                {
                    instance2.InvokeMethod("Terminate", null);
                    break;
                }
            }
            foreach (ManagementObject instance3 in managementClass.GetInstances()) //Отключаем Битрейдер
            {
                if (instance3["Name"].Equals("BRSptSvc.exe"))
                {
                    instance3.InvokeMethod("Terminate", null);
                    break;
                }
            }
            if (gender == 1)//Подменяем аргументы в зависимости от пола персонажа
                arg = arg.Replace("main,en-us", "maln,ru-wm");
            else if (gender == 0)
                arg = arg.Replace("main,en-us", "maln,ru-ww");
            startInfo.FileName = heh1;
            startInfo.Arguments = arg;
            startInfo.WorkingDirectory = work;
            string hash_in_config = Config.AppSettings.Settings["hash"].Value;
            string hash_original = CalculateMD5(GamePath + "\\Assets\\swtor_main_global_1.tor");
            if (hash_in_config == hash_original)//Повторно запускаем игру с подменёнными параметрами
                Process.Start(startInfo);
            else//Хэши не совпадают, запускаем переустановку русификатора в автоматическом режиме
            {
                Thread thread = new Thread((ThreadStart)delegate
                {
                    LogBox.AppendText("Hash не совпадает. Производится переустановка русификатора и игра запустится автоматически.\n");
                })
                {
                    IsBackground = true
                };
                thread.Start();
                await Install();
                Process.Start(startInfo);
            }
        }

        public void CreateConfig()//Создаём конфигурационный файл
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            stringBuilder.AppendLine("<configuration>");
            stringBuilder.AppendLine("  <startup> ");
            stringBuilder.AppendLine("      <supportedRuntime version=\"v4.0\" sku=\".NETFramework,Version=v4.7.2\" />");
            stringBuilder.AppendLine("  </startup>");
            stringBuilder.AppendLine("  <appSettings>");
            stringBuilder.AppendLine("    <add key=\"gamepath\" value=\"\" />");
            stringBuilder.AppendLine("    <add key=\"firstrun\" value=\"1\" />");
            stringBuilder.AppendLine("    <add key=\"hash\" value=\"\" />");
            stringBuilder.AppendLine("    <add key=\"gender\" value=\"1\" />");
            stringBuilder.AppendLine("    <add key=\"sith\" value=\"0\" />");
            stringBuilder.AppendLine("    <add key=\"skill\" value=\"0\" />");
            stringBuilder.AppendLine("    <add key=\"items\" value=\"0\" />");
            stringBuilder.AppendLine("    <add key=\"non_dialoge\" value=\"0\" />");
            stringBuilder.AppendLine("    <add key=\"google\" value=\"1\" />");
            stringBuilder.AppendLine("    <add key=\"a_translate\" value=\"1\" />");
            stringBuilder.AppendLine("    <add key=\"changes\" value=\"1\" />");
            stringBuilder.AppendLine("    <add key=\"author\" value=\"\" />");
            stringBuilder.AppendLine("    <add key=\"password\" value=\"\" />");
            stringBuilder.AppendLine("    <add key=\"email\" value=\"\" />");
            stringBuilder.AppendLine("    <add key=\"backup_row\" value=\"0\" />");
            stringBuilder.AppendLine("    <add key=\"auth_translate\" value=\"0\" />");
            stringBuilder.AppendLine("    <add key=\"row_updated_from_server\" value=\"06.03.2021 06:38:28\" />");
            stringBuilder.AppendLine("  </appSettings>");
            stringBuilder.AppendLine("</configuration>");
            File.WriteAllText(Assembly.GetEntryAssembly().Location + ".config", stringBuilder.ToString());
        }

        private async void Install_btn_Click(object sender, EventArgs e) //Устанавливаем русификатор
        {
            Buttons_activity(0);
            await Install();
        }

        public async Task Install()//Установка русификатора
        {
            if (RusFontsInstalled == 1 && File.Exists(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor_backup") && File.Exists(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor"))
            {
                File.Delete(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor");
                File.Move(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor_backup", GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor");
                Ins_font.Text = "Установить шрифт";
            }
            if (steam_game.Checked == true)//Возвращаем файлы в исходное положение
                TryFix();
            int num = EndOff(GamePath + "\\Assets\\swtor_en-us_global_1.tor");//Проверяем оригинальные ли файлы игры
            int num2 = EndOff(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor");
            if (num == 0 || num2 == 0)
                LogBox.AppendText("Оригинальные файлы повреждены! Восспользуйтесь функцией проверки файлов игры.\n");
            else if (num == 2 || num2 == 2)
                LogBox.AppendText("Необходимо обновить игру перед установкой русификатора!\n");
            else
            {
                LogBox.AppendText(Properties.Resources.patchhosts);
                PatchHosts();//Патчим host файл, чтобы заблокировать отправку отчётов
                LogBox.AppendText(Properties.Resources.copyfiles);
                await CopyfilesAsync();//Копируем оригинальные файлы
                LogBox.AppendText(Properties.Resources.Done + "\n");
                LogBox.AppendText(Properties.Resources.Patch);
                Patch patch = new Patch();
                await Task.Run(delegate
                {
                    patch.ConnectDB();//Патчим файлы
                });
                LogBox.AppendText(Properties.Resources.Done + "\n");
                Config.AppSettings.Settings["firstrun"].Value = "0";
                Config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                if (steam_game.Checked == true)
                    Steam_Rename();
                Install_btn.Text = "Переустановить";//Включаем элементы и переименовываем кнопку Установки
                RusInstalled = 1;
                Buttons_activity(1);
                LogBox.AppendText("Установка закончена.\n");
            }
        }

        private void PatchHosts()//Патчер host файла
        {
            string path = "C:\\Windows\\System32\\drivers\\etc\\hosts";
            if (File.Exists(path))
            {
                if (!File.ReadAllLines(path).Contains("# SWTOR crash send"))
                {
                    try
                    {
                        File.AppendAllLines(path, new string[6]
                        {
                            "\n",
                            "# SWTOR crash send",
                            "0.0.0.0 bugcatcher.swtor.com",
                            "0.0.0.0 crash.swtor.com",
                            "0.0.0.0 patcher-crash.swtor.com",
                            "\n"
                        });
                        LogBox.AppendText(Properties.Resources.Done + "\n");
                    }
                    catch (Exception)
                    {
                        LogBox.AppendText(Properties.Resources.hostsblock + "\n");
                    }
                }
                else
                    LogBox.AppendText(Properties.Resources.hostsalreadypatched + "\n");
            }
            else
                LogBox.AppendText(Properties.Resources.hostsnotfound + "\n");
        }

        public async Task CopyfilesAsync() //Копирование оригинальных файлов
        {
            ProgressBar1.Value = 0;
            ProgressBar1.Maximum = 10;
            await CopyFileAsync(GamePath + "\\Assets\\swtor_en-us_global_1.tor", GamePath + "\\Assets\\swtor_ru-wm_global_1.tor");
            ProgressBar1.Value += 1;
            await CopyFileAsync(GamePath + "\\Assets\\swtor_en-us_global_1.tor", GamePath + "\\Assets\\swtor_ru-ww_global_1.tor");
            ProgressBar1.Value += 2;
            await CopyFileAsync(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor", GamePath + "\\Assets\\swtor_maln_gfx_assets_1.tor");
            await CopyFileAsync(GamePath + "\\swtor\\retailclient\\main_gfx_1.tor", GamePath + "\\swtor\\retailclient\\main_gfx_1.backup");
            ProgressBar1.Value += 3;
            await CopyFileAsync(GamePath + "\\Assets\\swtor_main_global_1.tor", GamePath + "\\Assets\\swtor_maln_global_1.tor");
            string hash_original = CalculateMD5(GamePath + "\\Assets\\swtor_main_global_1.tor");
            ProgressBar1.Value += 4;
            Config.AppSettings.Settings["hash"].Value = hash_original;
            Config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public async Task CopyFileAsync(string sourcePath, string destinationPath)//Ассинхронное копирование файлов
        {
            using (Stream source = File.OpenRead(sourcePath))
            {
                using (Stream destination = File.Create(destinationPath))
                {
                    await source.CopyToAsync(destination);
                }
            }
        }

        public static string CalculateMD5(string filename)//Вычисляем Hash
        {
            using (MD5 mD = MD5.Create())
            {
                using (FileStream inputStream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(mD.ComputeHash(inputStream)).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        private void ChangePathButton_Click(object sender, EventArgs e)//Обработка изменения пути к игре
        {
            ChoosePath();
        }

        private void ChoosePath()//Обработка изменения пути к игре
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
                Description = "Укажите путь к SWToR"
            };
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                if (folderBrowserDialog.SelectedPath.ToLower().IndexOf("steamapps") > 0)//Проверяем путь к обычной версии или к Steam
                    steam_game.Checked = true;
                else if (steam_game.Checked == true)
                    steam_game.Checked = false;
                GamePath = folderBrowserDialog.SelectedPath;
                if (!GamePath.EndsWith("\\"))
                    GamePath += "\\";
                if (launch_status == 1)
                    TryFix();
                GamePathTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void ChooseSith_CheckedChanged(object sender, EventArgs e)//Переключатель варианта перевода Ситх\Сит
        {
            Config.AppSettings.Settings["sith"].Value = "0";
            Config.Save(ConfigurationSaveMode.Modified);
        }

        private void ChooseSit_CheckedChanged(object sender, EventArgs e)//Переключатель варианта перевода Ситх\Сит
        {
            Config.AppSettings.Settings["sith"].Value = "1";
            Config.Save(ConfigurationSaveMode.Modified);
        }

        private void ChooseMen_CheckedChanged(object sender, EventArgs e)//Переключатель мужской\женский персонаж
        {
            Config.AppSettings.Settings["gender"].Value = "1";
            Config.Save(ConfigurationSaveMode.Modified);
            gender = 1;
            if (steam_game.Checked == true && RusInstalled==1)
                Gender_steam_change("ru-wm");
        }

        private void ChooseWomen_CheckedChanged(object sender, EventArgs e)//Переключатель мужской\женский персонаж
        {
            Config.AppSettings.Settings["gender"].Value = "0";
            Config.Save(ConfigurationSaveMode.Modified);
            gender = 0;
            if (steam_game.Checked == true && RusInstalled == 1)
                Gender_steam_change("ru-ww");
        }

        public void Gender_steam_change(string first)//Переключатель мужской\женский персонаж для Steam версии игры
        {
            if (File.Exists(GamePath + "\\Assets\\swtor_en-us_global_1_tmp.tor"))
            {
                if (File.Exists(GamePath + "\\Assets\\swtor_ru-ww_global_1.tor"))
                    File.Move(GamePath + "\\Assets\\swtor_en-us_global_1.tor", GamePath + "\\Assets\\swtor_ru-wm_global_1.tor");
                else if (File.Exists(GamePath + "\\Assets\\swtor_ru-wm_global_1.tor"))
                    File.Move(GamePath + "\\Assets\\swtor_en-us_global_1.tor", GamePath + "\\Assets\\swtor_ru-ww_global_1.tor");
                File.Move(GamePath + "\\Assets\\swtor_"+ first + "_global_1.tor", GamePath + "\\Assets\\swtor_en-us_global_1.tor");
            }
        }

        private void GamePathTextBox_TextChanged(object sender, EventArgs e)//Проверяем изменение пути к игре
        {            
            string NewPath = GamePathTextBox.Text;
            if (!NewPath.EndsWith("\\"))
                NewPath += "\\";
            if (!File.Exists(NewPath + "launcher.exe")) //Если игры нет предлагаем заново выбрать путь
            {
                MessageBox.Show(Properties.Resources.WrongPath);
                ChoosePath();
                return;
            }
            Install_btn.Enabled = true;
            Ins_font.Enabled = true;
            Config.AppSettings.Settings["gamepath"].Value = NewPath;
            Config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        
        private void Del_btn_Click(object sender, EventArgs e)//Удаляем русификатор
        {
            Buttons_activity(0);
            LogBox.AppendText(Properties.Resources.deletefiles);
            try
            {
                if (steam_game.Checked == true)
                    TryFix();

                if (File.Exists(GamePath + "\\Assets\\swtor_ru-wm_global_1.tor"))
                    File.Delete(GamePath + "\\Assets\\swtor_ru-wm_global_1.tor");
                if (File.Exists(GamePath + "\\Assets\\swtor_ru-ww_global_1.tor"))
                    File.Delete(GamePath + "\\Assets\\swtor_ru-ww_global_1.tor");
                if (File.Exists(GamePath + "\\Assets\\swtor_maln_gfx_assets_1.tor"))
                    File.Delete(GamePath + "\\Assets\\swtor_maln_gfx_assets_1.tor");
                if (File.Exists(GamePath + "\\Assets\\swtor_maln_global_1.tor"))
                    File.Delete(GamePath + "\\Assets\\swtor_maln_global_1.tor");
                if (File.Exists(GamePath + "\\swtor\\retailclient\\main_gfx_1.backup"))
                {
                    File.Copy(GamePath + "\\swtor\\retailclient\\main_gfx_1.backup", GamePath + "\\swtor\\retailclient\\main_gfx_1.tor", true);
                    File.Delete(GamePath + "\\swtor\\retailclient\\main_gfx_1.backup");
                }
            }
            catch (Exception)
            {
            }
            LogBox.AppendText(Properties.Resources.Done + "\n");
            LogBox.AppendText(Properties.Resources.deletefromhosts);
            string path = "C:\\Windows\\System32\\drivers\\etc\\hosts";
            if (File.Exists(path))
            {
                try
                {
                    File.WriteAllLines(path, (from s in File.ReadLines(path)
                                              where s != "# SWTOR crash send"
                                              select s).ToList());
                    File.WriteAllLines(path, (from s in File.ReadLines(path)
                                              where s != "0.0.0.0 bugcatcher.swtor.com"
                                              select s).ToList());
                    File.WriteAllLines(path, (from s in File.ReadLines(path)
                                              where s != "0.0.0.0 crash.swtor.com"
                                              select s).ToList());
                    File.WriteAllLines(path, (from s in File.ReadLines(path)
                                              where s != "0.0.0.0 patcher-crash.swtor.com"
                                              select s).ToList());
                }
                catch (Exception)
                {
                }                
            }
            LogBox.AppendText(Properties.Resources.Done + "\n");
            LogBox.AppendText("Удаление закончено.\n");
            Install_btn.Text = "Установить";
            RusInstalled = 0;
            Buttons_activity(1);
        }
        private void Db_convertor_Click(object sender, EventArgs e)
        {
            cou = 0;
            FileStream fileStream = new FileStream("swtor_main_gfx_assets_1.tor", FileMode.Open, FileAccess.ReadWrite);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            binaryReader.BaseStream.Seek(12L, SeekOrigin.Begin);
            uint num = binaryReader.ReadUInt32();
            binaryReader.BaseStream.Seek(24L, SeekOrigin.Begin);
            filescount = binaryReader.ReadUInt32();
            endtable = 0;
            endtable = EndOff(fileStream);
            int num2 = (int)Math.Ceiling((double)filescount / 1000.0);
            binaryReader.BaseStream.Seek(num, SeekOrigin.Begin);
            long position = binaryReader.BaseStream.Position;
            while (num2 > 0)
            {
                binaryReader.BaseStream.Position = position;
                uint num3 = binaryReader.ReadUInt32();
                if (num3 != 1000)
                    break;
                if (binaryReader.ReadUInt32() == 0)
                    lastoffes = (int)binaryReader.BaseStream.Position - 8;
                binaryReader.ReadUInt32();
                position = binaryReader.BaseStream.Position;
                for (int i = 0; i < num3; i++)
                {
                    binaryReader.BaseStream.Position = position;
                    uint num4 = binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    uint zsize = binaryReader.ReadUInt32();
                    uint size = binaryReader.ReadUInt32();
                    uint hash = binaryReader.ReadUInt32();
                    uint num5 = binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt16();
                    position = binaryReader.BaseStream.Position;
                    if (num4 == 0)
                        break;

                    //Console.WriteLine(num4 + "---" + zsize + "---" + size + "---" + hash + "---" + num5.ToString("X") + "---" + num5);
                    /*if (num5.ToString("X") == "CE7C8110")
                    {
                        Console.WriteLine(num4 + "---" + zsize + "---" + size + "---" + hash + "---" + num5.ToString("X") + "---" + num5);
                        
                        Console.WriteLine("aaaaaaaaaaaaaaa");
                        //Packfont(fileStream, num4, zsize, size, hash, num5, endtable, "bwaui_character_create_window.gfx");
                    }*/
                    if (num5.ToString("X") == "994442C9" )
                    {
                        Packfont(fileStream, num4, zsize, size, hash, num5, endtable, "swtor_fonts.gfx");
                        Console.WriteLine("aaaaaaaaaaaaaaa");
                    }
                    if (num5.ToString("X") == "EC874439")
                    { 
                        Packfont(fileStream, num4, zsize, size, hash, num5, endtable, "drawtext_fonts.swf");
                        Console.WriteLine("aaaaaaaaaaaaaaa");
                    }
                }
                num2--;
            }
            int num6 = (int)filescount % 1000;
            if (cou + num6 + 1 > 1000)
            {
                BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                binaryWriter.BaseStream.Position = lastoffes;
                binaryWriter.Write(cou + num6 + 1);
                binaryWriter.Close();
                fileStream.Close();
            }
            Console.WriteLine("----");
        }
        public void Packfont(FileStream fileStream, uint off, uint zsize, uint size, uint hash2, uint hash1, int endtable,string file_font_new)
        {
            BinaryReader binaryReader = new BinaryReader(fileStream);
            binaryReader.BaseStream.Position = off;
            binaryReader.ReadBytes(36);
            byte[] input = binaryReader.ReadBytes((int)zsize);
            byte[] buffer = new byte[size];
            Inflater inflater = new Inflater();
            inflater.SetInput(input);
            inflater.Inflate(buffer);




           







            Console.WriteLine(off + "---" + zsize + "---" + size + "---" + hash2 + "---" + hash1 + "---" + endtable);


            MemoryStream memoryStream = new MemoryStream();
            using (FileStream file = new FileStream(file_font_new, FileMode.Open, FileAccess.Read))
                file.CopyTo(memoryStream);
            MemoryStream memoryStream2 = new MemoryStream();
            Deflater deflater = new Deflater(9);
            deflater.SetInput(memoryStream.ToArray());
            deflater.Finish();
            byte[] array = new byte[size * 3];
            while (!deflater.IsNeedingInput)
            {
                int count = deflater.Deflate(array);
                memoryStream2.Write(array, 0, count);
                if (deflater.IsFinished)
                    break;
            }
            deflater.Reset();
            int newsize = memoryStream.ToArray().Length;
            int newzsize = memoryStream2.ToArray().Length;

            byte[] array2 = new byte[36];
            array2[0] = 2;
            array2[2] = 32;
            byte[] array3 = new byte[array2.Length + memoryStream2.ToArray().Length];
            array2.CopyTo(array3, 0);
            memoryStream2.ToArray().CopyTo(array3, array2.Length);

            Console.WriteLine(off + "---" + newzsize + "---" + newsize + "---" + hash2 + "---" + hash1 + "---" + endtable);
            Hhh(fileStream, array3, newzsize, newsize, hash2, hash1, endtable);


        }
        public int EndOff(FileStream fileStream)
        {
            int result = 0;
            BinaryReader binaryReader = new BinaryReader(fileStream);
            binaryReader.BaseStream.Seek(12L, SeekOrigin.Begin);
            uint num = binaryReader.ReadUInt32();
            binaryReader.BaseStream.Seek(24L, SeekOrigin.Begin);
            uint num2 = binaryReader.ReadUInt32();
            int num3 = (int)Math.Ceiling((double)num2 / 1000.0);
            binaryReader.BaseStream.Seek(num, SeekOrigin.Begin);
            long position = binaryReader.BaseStream.Position;
            int num4 = 0;
            long num5 = 0L;
            while (num3 > 0)
            {
                binaryReader.BaseStream.Position = position;
                uint num6 = binaryReader.ReadUInt32();
                if (num6 != 1000)
                {
                    break;
                }
                position = binaryReader.ReadUInt32();
                binaryReader.ReadUInt32();
                for (int i = 0; i < num6; i++)
                {
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt16();
                    num5 = binaryReader.BaseStream.Position;
                    num4++;
                    if (num4 == num2)
                    {
                        result = (int)num5 + 34;
                        break;
                    }
                }
                num3--;
            }
            return result;
        }
        public void Hhh(FileStream fileStream, byte[] z, int newzsize, int newsize, uint hash2, uint hash1, int endtable)
        {
            BinaryWriter binaryWriter = new BinaryWriter(fileStream);
            uint textoffset = (uint)fileStream.Length;
            binaryWriter.BaseStream.Position = fileStream.Length;
            binaryWriter.Write(z);
            FileTable(fileStream, endtable, textoffset, newzsize, newsize, hash2, hash1);
        }
        public void FileTable(FileStream fileStream, int endtable, uint textoffset, int newzsize, int newsize, uint hash2, uint hash1)
        {
            cou++;
            BinaryWriter binaryWriter = new BinaryWriter(fileStream);
            binaryWriter.BaseStream.Position = endtable;
            binaryWriter.Write(textoffset);
            binaryWriter.Write(0);
            binaryWriter.Write(36);
            binaryWriter.Write(newzsize);
            binaryWriter.Write(newsize);
            binaryWriter.Write(hash2);
            binaryWriter.Write(hash1);
            binaryWriter.Write(0);
            binaryWriter.Write((short)1);
            this.endtable += 34;
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://vk.com/togruth");
        }

        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://vk.com/swtor_jk");
        }

        private void LinkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://vk.com/club195326840");
        }

        private void Btn_info_Click(object sender, EventArgs e)//Окно Информация
        {
            if (ActiveForm.Height == 400)
            {
                Enabled = false;
                List<string> list_translators = new List<string>();
                using (SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; "))
                {
                    using (SQLiteCommand sqlite_cmd = new SQLiteCommand(sqlite_conn))
                    {
                        sqlite_conn.Open();
                        string sqllite_select = "SELECT COUNT(DISTINCT text_en) FROM Translated";
                        sqlite_cmd.CommandText = sqllite_select;
                        float count_all_rows = Convert.ToInt32(sqlite_cmd.ExecuteScalar());
                        sqllite_select = "SELECT COUNT(DISTINCT text_en) from Translated WHERE translator_m!='Deepl'";
                        sqlite_cmd.CommandText = sqllite_select;
                        float count_trans_rows = Convert.ToInt32(sqlite_cmd.ExecuteScalar());
                        float percentag = (count_trans_rows / count_all_rows) * 100;
                        row_translated.Invoke((MethodInvoker)(() => row_translated.Text = Math.Round(percentag, 2) + "% (" + count_trans_rows + "/" + count_all_rows + ")"));//процентр перевода показываем
                        sqllite_select = "SELECT translator_m,translator_w FROM Translated GROUP by translator_m";
                        sqlite_cmd.CommandText = sqllite_select;
                        SQLiteDataReader r = sqlite_cmd.ExecuteReader();
                        while (r.Read())
                        {
                            if (!list_translators.Contains(WebUtility.HtmlDecode(r["translator_m"].ToString()), StringComparer.OrdinalIgnoreCase))
                                list_translators.Add(WebUtility.HtmlDecode(r["translator_m"].ToString()));
                            if (!list_translators.Contains(WebUtility.HtmlDecode(r["translator_w"].ToString()), StringComparer.OrdinalIgnoreCase))
                                list_translators.Add(WebUtility.HtmlDecode(r["translator_w"].ToString()));
                        }
                        r.Close();
                        sqlite_cmd.Dispose();
                        sqlite_conn.Close();
                    }
                }
                string list_translator = "";
                foreach (string list_trans in list_translators)
                {
                    if (list_translator != "" && list_trans == "1")
                        list_translator += ", Togruth";
                    else if (list_trans == "1")
                        list_translator = "Togruth";
                    if (list_translator != "" && list_trans == "2")
                        list_translator += ", JKC";
                    else if (list_trans == "2")
                        list_translator = "JKC";
                    if (list_translator != "" && list_trans == "3")
                        list_translator += ", Deepl";
                    else if (list_trans == "3")
                        list_translator = "Deepl";
                    if (list_translator != "" && list_trans == "4")
                        list_translator += ", Krestnik02";
                    else if (list_trans == "4")
                        list_translator = "Krestnik02";
                    if (list_translator != "" && list_trans == "5")
                        list_translator += ", Другие переводчики";
                    else if (list_trans == "5")
                        list_translator = "Другие переводчики";
                    if (list_translator != "" && list_trans != "" && list_trans != "1" && list_trans != "2" && list_trans != "3" && list_trans != "4" && list_trans != "5")
                        list_translator += ", " + list_trans;
                    else if (list_trans != "" && list_trans != "1" && list_trans != "2" && list_trans != "3" && list_trans != "4" && list_trans != "5")
                        list_translator = list_trans;
                }
                info_trans.Invoke((MethodInvoker)(() => info_trans.AppendText("Переводчики игры: \n")));
                info_trans.Invoke((MethodInvoker)(() => info_trans.AppendText(list_translator + "\n\n")));
                using (StreamReader sr_trans = new StreamReader("db\\info.txt", Encoding.Default))
                {
                    while (!sr_trans.EndOfStream)
                        info_trans.Invoke((MethodInvoker)(() => info_trans.AppendText(sr_trans.ReadLine() + "\n")));
                }
                Height = 600;
                Enabled = true;
            }
            else
                Height = 400;
        }

        private void Google_opt_CheckedChanged(object sender, EventArgs e)//Переключатель Машинного Переводчика
        {
            string js;
            if (google_opt.Checked)
            {
                js = "1";
                auto_translate.Enabled = true;
            }
            else
            {
                js = "0";
                auto_translate.Checked = false;
                auto_translate.Enabled = false;
                changes.Checked = false;
                changes.Enabled = false;
                Config.AppSettings.Settings["changes"].Value = "0";
                Config.AppSettings.Settings["a_translate"].Value = "0";
            }
            Config.AppSettings.Settings["google"].Value = js;
            Config.Save(ConfigurationSaveMode.Modified);
        }

        private void Editor_btn_Click_1(object sender, EventArgs e)//Открываем окно редакора
        {
            this.Hide();
            Form2 form2 = new Form2();
            form2.Show();            
        }

        private void Dis_skills_CheckedChanged(object sender, EventArgs e)//Переключатель отключения перевода скилов
        {
            string js;
            if (Dis_skills.Checked)
            {
                js = "2";
                auto_translate.Checked = false;
                auto_translate.Enabled = false;
                changes.Checked = false;
                changes.Enabled = false;
            }
            else
            {
                js = "0";
                if (google_opt.Checked == true)
                    auto_translate.Enabled = true;
            }
            Config.AppSettings.Settings["skill"].Value = js;
            Config.Save(ConfigurationSaveMode.Modified);
        }

        private async void Upload_to_server_Click(object sender, EventArgs e)//Выгрузка переводов на сервер
        {
            Buttons_activity(0);
            LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Начинаем выгрузку переводов на сервер...\n")));            
            ProgressBar1.Value = 0;            
            await Task.Run(() => Upload_to_server_method());
            Buttons_activity(1);
            LogBox.AppendText("Выгрузка закончена!\n");
        }

        public void Upload_to_server_method()//Выгрузка переводов на сервер
        {
            string key_import = "";
            string text_ru_m_import = "";
            string translator_m_import = "";
            string text_ru_w_import = "";
            string translator_w_import = "";
            string text_en_import = "";
            string sql_update = "";
            string sql_insert = "";
            int num_edited_rows = 0;
            DateTime time = DateTime.UtcNow;
            string format = "dd.MM.yyyy HH:mm:ss";
            string mysql_time_export = time.ToString(format);
            string[] allfiles = Directory.GetFiles("user_translation\\", "*.xml", SearchOption.TopDirectoryOnly);
            using (MySqlConnection conn = new MySqlConnection(connStr_mysql))
            {
                conn.Open();
                int verified_user = 0;
                //смотрим пользователя
                if (Config.AppSettings.Settings["author"].Value != "" && Config.AppSettings.Settings["email"].Value != "")
                {
                    string sql_select_user = "SELECT id, email, name, pass,status FROM users WHERE email='" + Config.AppSettings.Settings["email"].Value + "' AND name = '" + Config.AppSettings.Settings["author"].Value + "' AND verified='1';";
                    MySqlCommand command = new MySqlCommand(sql_select_user, conn);
                    MySqlDataReader row = command.ExecuteReader();
                    if (row.HasRows)
                        verified_user = 1;
                    else
                        verified_user = 0;
                    row.Close();
                    command.Dispose();
                }
                else
                    verified_user = 0;
                //Console.WriteLine(verified_user);
                if (verified_user==0)
                {
                    LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Вы не идентифицированы! Авторизуйтесь в редакторе и напишите в дискорд канал для получения статуса переводчика https://discord.gg/dhJKxQjpgu .\n")));
                }
                else
                {
                    foreach (string filename in allfiles)
                    {
                        LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Работаем с файлом " + filename + "\n")));
                        ProgressBar1.Invoke((MethodInvoker)(() => ProgressBar1.Value = 0));
                        int lineCount = File.ReadLines(filename).Count();
                        ProgressBar1.Invoke((MethodInvoker)(() => ProgressBar1.Maximum = lineCount - 2));
                        XmlDocument xDoc1 = new XmlDocument();
                        xDoc1.Load(filename);
                        XmlElement xRoot1 = xDoc1.DocumentElement;
                        int jks = 1;
                        foreach (XmlNode childnode in xRoot1)
                        {
                            if (childnode.Name == "key")
                                key_import = childnode.InnerText;
                            if (childnode.Name == "text_en")
                                text_en_import = WebUtility.HtmlDecode(childnode.InnerText);
                            if (childnode.Name == "text_ru_m")
                            {
                                text_ru_m_import = WebUtility.HtmlDecode(childnode.InnerText);
                                translator_m_import = WebUtility.HtmlDecode(childnode.Attributes.GetNamedItem("transl").Value);
                            }
                            if (childnode.Name == "text_ru_w")
                            {
                                text_ru_w_import = WebUtility.HtmlDecode(childnode.InnerText);
                                translator_w_import = WebUtility.HtmlDecode(childnode.Attributes.GetNamedItem("transl").Value);
                            }
                            if (jks % 4 == 0)
                            {
                                if (text_ru_w_import != "")
                                {
                                    if (translator_m_import != "Deepl" && translator_w_import != "Deepl")
                                    {
                                        sql_update = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "',translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "',text_ru_w='" + WebUtility.HtmlEncode(text_ru_w_import) + "',translator_w='" + WebUtility.HtmlEncode(translator_w_import) + "',tr_datetime=STR_TO_DATE('" + mysql_time_export + "', '%d.%m.%Y %H:%i:%s'),veryfied='" + verified_user + "' WHERE key_unic ='" + key_import + "' AND (translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "' OR translator_w='" + WebUtility.HtmlEncode(translator_w_import) + "')";
                                        sql_insert = "INSERT INTO Translated(key_unic,text_en,text_ru_m,text_ru_w,translator_m,translator_w,veryfied) VALUES ('" + key_import + "','" + WebUtility.HtmlEncode(text_en_import) + "','" + WebUtility.HtmlEncode(text_ru_m_import) + "','" + WebUtility.HtmlEncode(text_ru_w_import) + "','" + WebUtility.HtmlEncode(translator_m_import) + "','" + WebUtility.HtmlEncode(translator_w_import) + "','" + verified_user + "')";
                                    }
                                    else if (translator_m_import != "Deepl")
                                    {
                                        sql_update = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "',translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "',tr_datetime=STR_TO_DATE('" + mysql_time_export + "', '%d.%m.%Y %H:%i:%s'),veryfied='" + verified_user + "' WHERE key_unic ='" + key_import + "' AND translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "'";
                                        sql_insert = "INSERT INTO Translated(key_unic,text_en,text_ru_m,translator_m,veryfied) VALUES ('" + key_import + "','" + WebUtility.HtmlEncode(text_en_import) + "','" + WebUtility.HtmlEncode(text_ru_m_import) + "','" + WebUtility.HtmlEncode(translator_m_import) + "','" + verified_user + "')";
                                    }
                                    else if (translator_w_import != "Deepl")
                                    {
                                        sql_update = "UPDATE Translated SET text_ru_w='" + WebUtility.HtmlEncode(text_ru_w_import) + "',translator_w='" + WebUtility.HtmlEncode(translator_w_import) + "',tr_datetime=STR_TO_DATE('" + mysql_time_export + "', '%d.%m.%Y %H:%i:%s'),veryfied='" + verified_user + "' WHERE key_unic ='" + key_import + "' AND translator_w='" + WebUtility.HtmlEncode(translator_w_import) + "'";
                                        sql_insert = "INSERT INTO Translated(key_unic,text_en,text_ru_w,translator_w,veryfied) VALUES ('" + key_import + "','" + WebUtility.HtmlEncode(text_en_import) + "','" + WebUtility.HtmlEncode(text_ru_w_import) + "','" + WebUtility.HtmlEncode(translator_w_import) + "','" + verified_user + "')";
                                    }
                                }
                                else if (text_ru_m_import != "")
                                {
                                    if (translator_m_import != "Deepl")
                                    {
                                        sql_update = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "',translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "',tr_datetime=STR_TO_DATE('" + mysql_time_export + "', '%d.%m.%Y %H:%i:%s'),veryfied='" + verified_user + "' WHERE key_unic ='" + key_import + "' AND translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "'";
                                        sql_insert = "INSERT INTO Translated(key_unic,text_en,text_ru_m,translator_m,veryfied) VALUES ('" + key_import + "','" + WebUtility.HtmlEncode(text_en_import) + "','" + WebUtility.HtmlEncode(text_ru_m_import) + "','" + WebUtility.HtmlEncode(translator_m_import) + "','" + verified_user + "')";
                                    }
                                }
                                MySqlCommand update = new MySqlCommand(sql_update, conn);
                                int numRowsUpdated = update.ExecuteNonQuery();
                                update.Dispose();
                                if (numRowsUpdated == 0)
                                {
                                    MySqlCommand insert = new MySqlCommand(sql_insert, conn);
                                    insert.ExecuteNonQuery();
                                    insert.Dispose();
                                }
                                num_edited_rows++;
                                ProgressBar1.Invoke((MethodInvoker)(() => ProgressBar1.Value += 1));
                            }
                            jks++;
                        }
                        if (!Directory.Exists("user_translation\\done"))
                            Directory.CreateDirectory("user_translation\\done");
                        string[] tokens0 = filename.Split(new char[] { '\\' });
                        Console.WriteLine("user_translation\\done\\" + tokens0.Last() + ".xml");
                        if (File.Exists("user_translation\\done\\" + tokens0.Last()))
                        {
                            DialogResult dialogResult = MessageBox.Show("Файл с таким именем уже существует. Вы уверены что хотите перенести новые переводы в него?", "Подтверждение", MessageBoxButtons.YesNo);
                            if (dialogResult == DialogResult.Yes)
                            {
                                var lines = File.ReadAllLines("user_translation\\done\\" + tokens0.Last());
                                var lines2 = File.ReadAllLines("user_translation\\" + tokens0.Last());
                                File.WriteAllLines("user_translation\\done\\" + tokens0.Last(), lines.Take(lines.Length - 1).ToArray(), encoding: Encoding.UTF8);
                                using (StreamWriter file_for_exam =
                                                                                new StreamWriter("user_translation\\done\\" + tokens0.Last(), true, encoding: Encoding.UTF8))
                                {
                                    for (int jk = 1; jk <= lines2.Length - 2; jk++)
                                        file_for_exam.WriteLine(lines2[jk]);
                                    file_for_exam.WriteLine("</rezult>");
                                }
                                File.Delete("user_translation\\" + tokens0.Last());
                            }
                            else
                            {
                                string[] tokens1 = tokens0.Last().Split(new string[] { ".xm" }, StringSplitOptions.None);
                                File.Move(filename, "user_translation\\done\\" + tokens1[0] + "1.xml");
                            }
                        }
                        else
                        {
                            File.Move(filename, "user_translation\\done\\" + tokens0.Last());
                        }
                    }
                }
                
                conn.Close();
                ProgressBar1.Invoke((MethodInvoker)(() => ProgressBar1.Value = 0));
                LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Выгрузка закончена. Выгружено " + num_edited_rows + " строк.\n")));
            }
        }

        private async void Upload_from_server_Click(object sender, EventArgs e) //Загрузка переводов с сервера
        {
            Buttons_activity(0);
            LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Начинаем загрузку переводов с сервера...\n")));
            ProgressBar1.Value = 0;
            await Task.Run(() => Upload_from_server_method());
            Buttons_activity(1);
            LogBox.AppendText("Загрузка переводов закончена!\n");  
        }
        public void Upload_from_server_method()//Загрузка переводов с сервера
        {
            if (!File.Exists("db\\translate_backup.db3"))
                File.Copy("db\\translate.db3", "db\\translate_backup.db3");
            else
            {
                File.Delete("db\\translate_backup.db3");
                File.Copy("db\\translate.db3", "db\\translate_backup.db3");
            }
            if (!Directory.Exists("tmp"))
                Directory.CreateDirectory("tmp");
            if (File.Exists("tmp\\server_update.xml"))
                File.Delete("tmp\\server_update.xml");
            string sqllite_update = "";
            int num_edited_rows = 0;
            string mysql_time_export = "";
            DateTime time = DateTime.UtcNow;
            string format = "dd.MM.yyyy HH:mm:ss";
            mysql_time_export = time.ToString(format);
            string xml_name = mysql_time_export.Replace(":", "");
            int count_for_xml = 0;
            List<string> update_list = new List<string>();
            using (SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; "))
            {
                sqlite_conn.Open();
                using (SQLiteCommand sqlite_cmd = new SQLiteCommand(sqlite_conn))
                {
                    List<string> add_list = new List<string>();
                    List<string> blocked_users = new List<string>();
                    ProgressBar1.Invoke((MethodInvoker)(() => ProgressBar1.Value = 0));
                    string sql = "";
                    string xml_text = "";
                    using (MySqlConnection conn = new MySqlConnection(connStr_mysql))
                    {
                        conn.Open();
                        if (Config.AppSettings.Settings["auth_translate"].Value == "1")//Если стоит отметка запрета загрузки заблокированных переводов, получаем список заблокированных авторов
                        {
                            sql = "SELECT key_unic,text_en,text_ru_m,text_ru_w,translator_m,translator_w FROM translated WHERE (tr_datetime>STR_TO_DATE('" + Config.AppSettings.Settings["row_updated_from_server"].Value + "', '%d.%m.%Y %H:%i:%s') AND veryfied='1')";
                            /*sql = "SELECT name FROM users WHERE status=1";
                            MySqlCommand command = new MySqlCommand(sql, conn);
                            MySqlDataReader reader1 = command.ExecuteReader();
                            while (reader1.Read())
                            {
                                blocked_users.Add(reader1["name"].ToString());
                            }
                            reader1.Close();*/
                        }
                        else
                            sql = "SELECT key_unic,text_en,text_ru_m,text_ru_w,translator_m,translator_w FROM translated WHERE tr_datetime>STR_TO_DATE('" + Config.AppSettings.Settings["row_updated_from_server"].Value + "', '%d.%m.%Y %H:%i:%s')";
                        MySqlCommand command2 = new MySqlCommand(sql, conn)
                        {
                            CommandText = sql
                        };
                        MySqlDataReader reader = command2.ExecuteReader();
                        using (StreamWriter tmp_save = new StreamWriter("tmp\\server_update.xml", true, encoding: Encoding.UTF8))
                        {
                            tmp_save.WriteLine("<rezult>");
                        }
                        while (reader.Read())//перебираем все новые строки
                        {
                            string xml_text1 = "<key>" + WebUtility.HtmlEncode(reader["key_unic"].ToString()) + "</key><text_en>" + WebUtility.HtmlEncode(reader["text_en"].ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(reader["translator_m"].ToString()) + "\">" + WebUtility.HtmlEncode(reader["text_ru_m"].ToString()) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(reader["translator_w"].ToString()) + "\">" + WebUtility.HtmlEncode(reader["text_ru_w"].ToString()) + "</text_ru_w>";
                            using (StreamWriter tmp_save = new StreamWriter("tmp\\server_update.xml", true, encoding: Encoding.UTF8))
                            {
                                tmp_save.WriteLine(xml_text1);
                            }
                        }
                        reader.Close();
                        conn.Close();
                    }
                    using (StreamWriter tmp_save = new StreamWriter("tmp\\server_update.xml", true, encoding: Encoding.UTF8))
                    {
                        tmp_save.WriteLine("</rezult>");
                    }
                    LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Строки загружены с сервера...\n")));
                    LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Подготавливаем строки...\n")));
                    string key_import = "";
                    string text_ru_m_import = "";
                    string translator_m_import = "";
                    string text_ru_w_import = "";
                    string translator_w_import = "";
                    string text_en_import = "";
                    XmlDocument xDoc1 = new XmlDocument();
                    xDoc1.Load("tmp\\server_update.xml");
                    XmlElement xRoot1 = xDoc1.DocumentElement;
                    int jks = 1;
                    int lineCount = File.ReadLines("tmp\\server_update.xml").Count();
                    ProgressBar1.Invoke((MethodInvoker)(() => ProgressBar1.Maximum = lineCount-2));
                    foreach (XmlNode childnode in xRoot1)
                    {
                        if (childnode.Name == "key")
                            key_import = childnode.InnerText;
                        if (childnode.Name == "text_en")
                            text_en_import = WebUtility.HtmlDecode(childnode.InnerText);
                        if (childnode.Name == "text_ru_m")
                        {
                            text_ru_m_import = WebUtility.HtmlDecode(childnode.InnerText);
                            translator_m_import = WebUtility.HtmlDecode(childnode.Attributes.GetNamedItem("transl").Value);
                        }
                        if (childnode.Name == "text_ru_w")
                        {
                            text_ru_w_import = WebUtility.HtmlDecode(childnode.InnerText);
                            translator_w_import = WebUtility.HtmlDecode(childnode.Attributes.GetNamedItem("transl").Value);
                        }
                        if (jks % 4 == 0)
                        {
                            sqllite_update = "";
                            if (text_ru_m_import != "" && text_ru_w_import != "")//Если в строке и М и Ж варианты перевода
                            {
                                /*if (Config.AppSettings.Settings["auth_translate"].Value == "1" || Config.AppSettings.Settings["translate_restrict"].Value == "1")
                                {
                                    string sql_select = "SELECT text_en, text_ru_m, text_ru_w, translator_m, translator_w FROM Translated WHERE key_unic='" + key_import + "'";
                                    sqlite_cmd.CommandText = sql_select;
                                    SQLiteDataReader reader1 = sqlite_cmd.ExecuteReader();
                                    while (reader1.Read()) //получили старых авторов этой строки перевода
                                    {
                                        xml_text = "";
                                        sqllite_update = "";
                                        if (Config.AppSettings.Settings["auth_translate"].Value == "1") //Если стоит отметка запрета загрузки заблокированных переводов,
                                        {
                                            if (blocked_users.Contains(reader1["translator_m"].ToString()) && reader1["translator_m"].ToString() != translator_m_import && blocked_users.Contains(reader1["translator_w"].ToString()) && reader1["translator_w"].ToString() != translator_w_import)
                                                xml_text = "<key>" + WebUtility.HtmlEncode(key_import) + "</key><text_en>" + WebUtility.HtmlEncode(reader1["text_en"].ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(translator_m_import) + "\">" + WebUtility.HtmlEncode(text_ru_m_import) + "</text_ru_m><text_ru_w  transl=\"" + WebUtility.HtmlEncode(translator_w_import) + "\">" + WebUtility.HtmlEncode(text_ru_w_import) + "</text_ru_w>";
                                            else if (blocked_users.Contains(reader1["translator_m"].ToString()) && reader1["translator_m"].ToString() != translator_m_import) //Если пользователь переводчик старого варианта строки М и есть новый переводчик
                                            {
                                                xml_text = "<key>" + WebUtility.HtmlEncode(key_import) + "</key><text_en>" + WebUtility.HtmlEncode(reader1["text_en"].ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(translator_m_import) + "\">" + WebUtility.HtmlEncode(text_ru_m_import) + "</text_ru_m><text_ru_w  transl=\"\"></text_ru_w>";
                                                sqllite_update = "UPDATE Translated SET text_ru_w='" + WebUtility.HtmlEncode(text_ru_w_import) + "',translator_w='" + WebUtility.HtmlEncode(translator_w_import) + "' WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "';";
                                                    
                                            }
                                            else if (blocked_users.Contains(reader1["translator_w"].ToString()) && reader1["translator_w"].ToString() != translator_w_import) //Если пользователь переводчик старого варианта строки Ж и есть новый переводчик
                                            {
                                                xml_text = "<key>" + WebUtility.HtmlEncode(key_import) + "</key><text_en>" + WebUtility.HtmlEncode(reader1["text_en"].ToString()) + "</text_en><text_ru_m transl=\"\"></text_ru_m><text_ru_w  transl=\"" + WebUtility.HtmlEncode(translator_w_import) + "\">" + WebUtility.HtmlEncode(text_ru_w_import) + "</text_ru_w>";
                                                sqllite_update = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "',translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "' WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "';";  
                                            }
                                            else if (!blocked_users.Contains(translator_m_import) && !blocked_users.Contains(translator_w_import))
                                                sqllite_update = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "',translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "',text_ru_w='" + WebUtility.HtmlEncode(text_ru_w_import) + "',translator_w='" + WebUtility.HtmlEncode(translator_w_import) + "' WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "';";
                                        }
                                        else if (Config.AppSettings.Settings["translate_restrict"].Value == "1")//Если запрещёно редактирование перевода пользователя
                                        {
                                            if (reader1["translator_m"].ToString() == Config.AppSettings.Settings["author"].Value.ToString() && reader1["translator_w"].ToString() == Config.AppSettings.Settings["author"].Value.ToString() && translator_m_import != Config.AppSettings.Settings["author"].Value.ToString() && translator_w_import != Config.AppSettings.Settings["author"].Value.ToString())
                                                xml_text = "<key>" + WebUtility.HtmlEncode(key_import) + "</key><text_en>" + WebUtility.HtmlEncode(reader1["text_en"].ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(translator_m_import) + "\">" + WebUtility.HtmlEncode(text_ru_m_import) + "</text_ru_m><text_ru_w  transl=\"" + WebUtility.HtmlEncode(translator_w_import) + "\">" + WebUtility.HtmlEncode(text_ru_w_import) + "</text_ru_w>";
                                            else if (reader1["translator_m"].ToString() == Config.AppSettings.Settings["author"].Value.ToString() && translator_m_import != Config.AppSettings.Settings["author"].Value.ToString()) //Если пользователь переводчик старого варианта строки М и есть новый переводчик
                                            {
                                                xml_text = "<key>" + WebUtility.HtmlEncode(key_import) + "</key><text_en>" + WebUtility.HtmlEncode(reader1["text_en"].ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(translator_m_import) + "\">" + WebUtility.HtmlEncode(text_ru_m_import) + "</text_ru_m><text_ru_w  transl=\"\"></text_ru_w>";
                                                sqllite_update = "UPDATE Translated SET text_ru_w='" + WebUtility.HtmlEncode(text_ru_w_import) + "',translator_w='" + WebUtility.HtmlEncode(translator_w_import) + "' WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "';";
                                            }
                                            else if (reader1["translator_w"].ToString() == Config.AppSettings.Settings["author"].Value.ToString() && translator_w_import != Config.AppSettings.Settings["author"].Value.ToString())//Если пользователь переводчик старого варианта строки Ж и есть новый переводчик
                                            {
                                                xml_text = "<key>" + WebUtility.HtmlEncode(key_import) + "</key><text_en>" + WebUtility.HtmlEncode(reader1["text_en"].ToString()) + "</text_en><text_ru_m transl=\"\"></text_ru_m><text_ru_w  transl=\"" + WebUtility.HtmlEncode(translator_w_import) + "\">" + WebUtility.HtmlEncode(text_ru_w_import) + "</text_ru_w>";
                                                sqllite_update = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "',translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "' WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "';";
                                            }
                                            else //Если переводчик строки М и Ж тот же самый
                                                sqllite_update = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "',translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "',text_ru_w='" + WebUtility.HtmlEncode(text_ru_w_import) + "',translator_w='" + WebUtility.HtmlEncode(translator_w_import) + "' WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "';";
                                        }
                                        else
                                            sqllite_update = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "',translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "',text_ru_w='" + WebUtility.HtmlEncode(text_ru_w_import) + "',translator_w='" + WebUtility.HtmlEncode(translator_w_import) + "' WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "';";
                                    }
                                    reader1.Close();
                                }
                                else*/
                                    sqllite_update = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "',translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "',text_ru_w='" + WebUtility.HtmlEncode(text_ru_w_import) + "',translator_w='" + WebUtility.HtmlEncode(translator_w_import) + "' WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "';";
                            }
                            else if (text_ru_m_import != "")//Если в строке только М вариант перевода
                            {
                                /*if (Config.AppSettings.Settings["auth_translate"].Value == "1" || Config.AppSettings.Settings["translate_restrict"].Value == "1")
                                {
                                    string sql_select = "SELECT text_en, text_ru_m, translator_m FROM Translated WHERE key_unic='" + key_import + "'";
                                    sqlite_cmd.CommandText = sql_select;
                                    SQLiteDataReader reader1 = sqlite_cmd.ExecuteReader();
                                    while (reader1.Read())//получили старого автора этой строки перевода
                                    {
                                        if (Config.AppSettings.Settings["auth_translate"].Value == "1")//Если стоит отметка запрета загрузки заблокированных переводов
                                        {
                                            if (reader1["translator_m"].ToString() != translator_m_import && blocked_users.Contains(reader1["translator_m"].ToString()))
                                                xml_text = "<key>" + WebUtility.HtmlEncode(key_import) + "</key><text_en>" + WebUtility.HtmlEncode(reader1["text_en"].ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(translator_m_import) + "\">" + WebUtility.HtmlEncode(text_ru_m_import) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(translator_w_import) + "\">" + WebUtility.HtmlEncode(text_ru_w_import) + "</text_ru_w>";
                                            else if (!blocked_users.Contains(reader1["translator_m"].ToString()) || reader1["translator_m"].ToString()== translator_m_import)
                                                sqllite_update = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "',translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "' WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "';";
                                        }
                                        else if (Config.AppSettings.Settings["translate_restrict"].Value == "1")// Если запрещёно редактирование перевода пользователя
                                        {
                                            if (reader1["translator_m"].ToString() == Config.AppSettings.Settings["author"].Value.ToString() && translator_m_import != Config.AppSettings.Settings["author"].Value.ToString())
                                                xml_text = "<key>" + WebUtility.HtmlEncode(key_import) + "</key><text_en>" + WebUtility.HtmlEncode(reader1["text_en"].ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(translator_m_import) + "\">" + WebUtility.HtmlEncode(text_ru_m_import) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(translator_w_import) + "\">" + WebUtility.HtmlEncode(text_ru_w_import) + "</text_ru_w>";
                                            else
                                                sqllite_update = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "',translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "' WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "';";
                                        }
                                        else
                                            sqllite_update = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "',translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "' WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "';";
                                    }
                                    reader1.Close();
                                }
                                else*/
                                    sqllite_update = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "',translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "' WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "';";
                            }
                            else if (text_ru_w_import != "")//Если в строке только Ж вариант перевода
                            {
                                /*if (Config.AppSettings.Settings["auth_translate"].Value == "1" || Config.AppSettings.Settings["translate_restrict"].Value == "1")
                                {
                                    string sql_select = "SELECT text_en, text_ru_m, translator_m FROM Translated WHERE key_unic='" + key_import + "'";
                                    sqlite_cmd.CommandText = sql_select;
                                    SQLiteDataReader reader1 = sqlite_cmd.ExecuteReader();
                                    while (reader1.Read())//получили старого автора этой строки перевода
                                    {
                                        if (Config.AppSettings.Settings["auth_translate"].Value == "1") //Если стоит отметка запрета загрузки заблокированных переводов
                                        {
                                            if (reader1["translator_w"].ToString() != translator_w_import && blocked_users.Contains(translator_w_import))
                                                xml_text = "<key>" + WebUtility.HtmlEncode(key_import) + "</key><text_en>" + WebUtility.HtmlEncode(reader1["text_en"].ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(translator_m_import) + "\">" + WebUtility.HtmlEncode(text_ru_m_import) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(translator_w_import) + "\">" + WebUtility.HtmlEncode(text_ru_w_import) + "</text_ru_w>";
                                            else if (!blocked_users.Contains(reader1["translator_w"].ToString()) || reader1["translator_w"].ToString() == translator_w_import)
                                                sqllite_update = "UPDATE Translated SET text_ru_w='" + WebUtility.HtmlEncode(text_ru_w_import) + "',translator_w='" + WebUtility.HtmlEncode(translator_w_import) + "' WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "';";
                                        }
                                        else if (Config.AppSettings.Settings["translate_restrict"].Value == "1")// Если запрещёно редактирование перевода пользователя
                                        {
                                            if (reader1["translator_w"].ToString() == Config.AppSettings.Settings["author"].Value.ToString() && translator_w_import != Config.AppSettings.Settings["author"].Value.ToString())
                                                xml_text = "<key>" + WebUtility.HtmlEncode(key_import) + "</key><text_en>" + WebUtility.HtmlEncode(reader1["text_en"].ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(translator_m_import) + "\">" + WebUtility.HtmlEncode(text_ru_m_import) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(translator_w_import) + "\">" + WebUtility.HtmlEncode(text_ru_w_import) + "</text_ru_w>";
                                            else
                                                sqllite_update = "UPDATE Translated SET text_ru_w='" + WebUtility.HtmlEncode(text_ru_w_import) + "',translator_w='" + WebUtility.HtmlEncode(translator_w_import) + "' WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "';";
                                        }
                                        else
                                            sqllite_update = "UPDATE Translated SET text_ru_w='" + WebUtility.HtmlEncode(text_ru_w_import) + "',translator_w='" + WebUtility.HtmlEncode(translator_w_import) + "' WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "';";
                                    }
                                    reader1.Close();
                                }
                                else*/
                                    sqllite_update = "UPDATE Translated SET text_ru_w='" + WebUtility.HtmlEncode(text_ru_w_import) + "',translator_w='" + WebUtility.HtmlEncode(translator_w_import) + "' WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "';";
                            }
                            if (sqllite_update != "")
                            {
                                update_list.Add(sqllite_update);
                                add_list.Add(key_import);
                                num_edited_rows++;
                            }
                            /*if (xml_text != "")
                            {
                                if (!Directory.Exists("blocked_translations"))//Создаём папку для блокированных переводов
                                    Directory.CreateDirectory("blocked_translations");
                                count_for_xml++;
                                if (count_for_xml == 1)
                                {
                                    using (StreamWriter tmp_save = new StreamWriter("blocked_translations\\" + xml_name + ".xml", true, encoding: Encoding.UTF8))
                                    {
                                        tmp_save.WriteLine("<rezult>");
                                    }
                                }
                                using (StreamWriter tmp_save = new StreamWriter("blocked_translations\\" + xml_name + ".xml", true, encoding: Encoding.UTF8))
                                {
                                    tmp_save.WriteLine(xml_text);
                                }
                            }*/
                            ProgressBar1.Invoke((MethodInvoker)(() => ProgressBar1.Value += 1));
                        }
                        jks++;
                    }
                    ProgressBar1.Invoke((MethodInvoker)(() => ProgressBar1.Maximum = num_edited_rows));
                    ProgressBar1.Invoke((MethodInvoker)(() => ProgressBar1.Value = 0));
                    LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Подготовка строк завершена...\n")));
                    LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Обновляем локальную базу...\n")));
                    using (SQLiteTransaction transaction = sqlite_conn.BeginTransaction())
                    {
                        update_list.ForEach(delegate (String name)
                        {
                            sqlite_cmd.CommandText = name;
                            sqlite_cmd.ExecuteNonQuery();
                            ProgressBar1.Invoke((MethodInvoker)(() => ProgressBar1.Value += 1));
                        });
                        transaction.Commit();
                    }
                    File.Delete("tmp\\server_update.xml");
                    /*if (count_for_xml != 0)
                    {
                        using (StreamWriter tmp_save = new StreamWriter("blocked_translations\\" + xml_name + ".xml", true, encoding: Encoding.UTF8))
                        {
                            tmp_save.WriteLine("</rezult>");
                        }
                    }*/
                    string[] allrows = add_list.ToArray();
                    sqllite_update = string.Format("SELECT fileinfo FROM Translated WHERE key_unic in ({0}) GROUP BY fileinfo", string.Join(", ", allrows));
                    sqlite_cmd.CommandText = sqllite_update;
                    SQLiteDataReader r = sqlite_cmd.ExecuteReader();
                    LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Внесены изменения в следующие файлы:\n")));
                    while (r.Read())
                        LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText(r["fileinfo"].ToString() + "\n")));
                    r.Close();
                }
                sqlite_conn.Close();
            }
            
            updatedownload.Invoke((MethodInvoker)(() => updatedownload.Text = "Соединение установлено! Новых переводов не обнаружено!"));
            updatedownload.Invoke((MethodInvoker)(() => updatedownload.ForeColor = Color.Green));
            Config.AppSettings.Settings["backup_row"].Value = Config.AppSettings.Settings["row_updated_from_server"].Value;
            Config.AppSettings.Settings["row_updated_from_server"].Value = mysql_time_export;
            Config.Save(ConfigurationSaveMode.Modified);
            ProgressBar1.Invoke((MethodInvoker)(() => ProgressBar1.Value = 0));
            LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Обновление локальной БД закончено. Загружено " + num_edited_rows + " строк.\n")));
            /*if (count_for_xml != 0)
                LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("При загрузке обнаружены строки, которые заменят ваш перевод! Они сохранены в папке blocked_translations.\n")));
            */
        }
        private void Recover_Click(object sender, EventArgs e)//Восстановление резервной копии БД
        {
            if (File.Exists("db\\translate_backup.db3"))
            {
                File.Delete("db\\translate.db3");
                File.Copy("db\\translate_backup.db3", "db\\translate.db3");
                File.Delete("db\\translate_backup.db3");
                Config.AppSettings.Settings["row_updated_from_server"].Value = Config.AppSettings.Settings["backup_row"].Value;
                Config.Save(ConfigurationSaveMode.Modified);
                // создаём объект для подключения к БД
                MySqlConnection conn = new MySqlConnection(connStr_mysql);
                // устанавливаем соединение с БД
                try
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM translated WHERE tr_datetime>STR_TO_DATE('" + Config.AppSettings.Settings["row_updated_from_server"].Value + "', '%d.%m.%Y %H:%i:%s')";
                    MySqlCommand command = new MySqlCommand(sql, conn);
                    int counrow = Int32.Parse(command.ExecuteScalar().ToString());
                    if (counrow <= Int32.Parse(Config.AppSettings.Settings["row_updated_from_server"].Value))
                    {
                        updatedownload.Text = "Соединение установлено! Новых переводов не обнаружено!";
                        updatedownload.ForeColor = Color.Green;
                        upload_from_server.Enabled = false;
                    }
                    else
                    {
                        int new_conut_row = counrow - Int32.Parse(Config.AppSettings.Settings["row_updated_from_server"].Value);
                        updatedownload.Text = "Соединение установлено! С последнего обновления обнаружено " + new_conut_row + " новых строк перевода!";
                        updatedownload.ForeColor = Color.Blue;
                        upload_from_server.Enabled = true;
                    }
                    command.Dispose();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    updatedownload.Text = "Не удалось соединиться с удалённой базой!" + ex.Message;
                }
                LogBox.AppendText("Резервная копия БД восстановлена!\n");
            }
        }

        private void PictureBox2_Click(object sender, EventArgs e)
        {
            if (steam_game.Checked == true)
                Process.Start("steam://rungameid/1286830");
            else
                Process.Start(GamePath + "\\launcher.exe");
                

        }

        private void Label1_Click(object sender, EventArgs e)//Скачиваем и открываем Инстукцию
        {
            if (File.Exists("Инструкция.pdf"))
                File.Delete("Инструкция.pdf");
            Downloading_Files("https://drive.google.com/uc?export=download&id=1fXQML3tazPL50Q6yy0x2qWauupouYkol", "Инструкция.pdf");
            Process.Start("Инструкция.pdf");
        }

        private async void Update_app_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)//Запуск обновления приложения
        {
            upload_to_server.Enabled = false;
            upload_from_server.Enabled = false;
            recover.Enabled = false;
            editor_btn.Enabled = false;
            ChangePathButton.Enabled = false;
            Install_btn.Enabled = false;
            del_btn.Enabled = false;
            LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Обновляем программу...\n")));
            await Task.Run(() => Update_app_method());
        }

        private void Update_app_method()//Запуск обновления приложения
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "updater.exe"))
                Downloading_Files("https://drive.google.com/uc?export=download&id=13fy8SNiBjnWEBzOc9F4v_ljKTWj3Zih1", "updater.exe"); //Загружаем обновление            
            Process proc = new Process();
            proc.StartInfo.WorkingDirectory = Application.StartupPath;
            proc.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "updater.exe";
            proc.StartInfo.Arguments = "/u \"" + AppDomain.CurrentDomain.BaseDirectory + "SWToR_RUS.exe" + "\"";//Аргументы командной строки
            proc.Start();//Запускаем!
            Invoke((MethodInvoker)delegate
            {
                Application.Exit();
            });
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)//Обработчик закрытия приложения
        {
            if (is_run == 0)
                Application.Exit();            
            if (steam_game.Checked == true)
                TryFix();
            Application.Exit();
        }

        private void Auto_translate_CheckedChanged(object sender, EventArgs e) //Переключатель авто-переводчика
        {
            if (google_opt.Checked == true)
            {
                string js;
                if (auto_translate.Checked)
                {
                    js = "1";
                    Dis_skills.Checked = false;
                    Dis_items.Checked = false;
                    Dis_non_dialoge.Checked = false;
                    changes.Enabled = true;
                }
                else
                {
                    js = "0";
                    changes.Enabled = false;
                    changes.Checked = false;
                    Config.AppSettings.Settings["changes"].Value = "0";
                }
                Config.AppSettings.Settings["a_translate"].Value = js;
                Config.Save(ConfigurationSaveMode.Modified);
            }
            else
            {
                auto_translate.Checked = false;
                changes.Enabled = false;
                changes.Checked = false;
            }
        }

        private void Changes_CheckedChanged(object sender, EventArgs e)//Переключатель проверки изменений
        {
            if (google_opt.Checked == true && auto_translate.Checked == true)
            {
                string js;
                if (changes.Checked)
                    js = "1";
                else
                    js = "0";
                Config.AppSettings.Settings["changes"].Value = js;
                Config.Save(ConfigurationSaveMode.Modified);
            }
        }

        private void App_Updater()//Обновление программы, проверка новых версий
        {
            string[] keys = Environment.GetCommandLineArgs();//Получаем аргументы командной строки
            if (keys.Length > 1)//Если приложение запущено с ключом
            {
                int loop = 10;//Количество попыток
                if (keys[1] == "/u")//Если ключ "u" запускаем updater.exe
                {
                    is_run = 0;
                    while (--loop > 0 && File.Exists(CurDir + "SWToR_RUS.exe"))//Удаляем оригинальный файл
                        try
                        {
                            File.Delete(CurDir + "SWToR_RUS.exe");
                        }
                        catch
                        {
                            Thread.Sleep(200);//Небольшая задержка, если файл занят
                        }
                    File.Copy(CurDir + "updater.exe", CurDir + "SWToR_RUS.exe");//Копируем скачанный файл в оригинальное имя файла
                    Process proc = new Process();//Запускаем Программу с ключом "d"
                    proc.StartInfo.WorkingDirectory = Application.StartupPath;
                    proc.StartInfo.FileName = CurDir + "SWToR_RUS.exe";
                    proc.StartInfo.Arguments = "/d \"" + CurDir + "SWToR_RUS.exe" + "\"";//Аргументы командной строки
                    proc.Start();
                    Close();//Закрываем текущее приложение
                }
                else if (keys[1] == "/d")//Если ключ "d" удаляем updater.exe
                {
                    while (--loop > 0 && File.Exists(CurDir + "updater.exe"))
                        try
                        {
                            File.Delete(CurDir + "updater.exe");
                        }
                        catch
                        {
                            Thread.Sleep(200);//Небольшая задержка, если файл занят
                        }
                    LogBox.AppendText("Приложение обновлено.\n");
                    is_run = 1;
                }
                else
                    is_run = 1;
            }
            else
                is_run = 1;
        }

        private void Downloading_Files(string link, string filename)//Загрузка файлов из интернета
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFile(new Uri(link), filename);
        }

        private void Logging(string Log)//Запись Логов
        {
            using (StreamWriter logging = new StreamWriter("log.txt", true, encoding: Encoding.UTF8))
                logging.WriteLine(Log);
        }

        private void TryFix() //Если файлы называются неправильно, пробуем их чинить (приложение закрылось аварийно)
        {
            if (File.Exists(GamePath + "\\Assets\\swtor_en-us_global_1_tmp.tor"))
            {
                if (File.Exists(GamePath + "\\Assets\\swtor_ru-ww_global_1.tor") && File.Exists(GamePath + "\\Assets\\swtor_en-us_global_1.tor"))
                    File.Move(GamePath + "\\Assets\\swtor_en-us_global_1.tor", GamePath + "\\Assets\\swtor_ru-wm_global_1.tor");
                else if (File.Exists(GamePath + "\\Assets\\swtor_ru-wm_global_1.tor") && File.Exists(GamePath + "\\Assets\\swtor_en-us_global_1.tor"))
                    File.Move(GamePath + "\\Assets\\swtor_en-us_global_1.tor", GamePath + "\\Assets\\swtor_ru-ww_global_1.tor");
                File.Move(GamePath + "\\Assets\\swtor_en-us_global_1_tmp.tor", GamePath + "\\Assets\\swtor_en-us_global_1.tor");
            }
            if (File.Exists(GamePath + "\\Assets\\swtor_main_global_1_tmp.tor"))
            {
                if (File.Exists(GamePath + "\\Assets\\swtor_main_global_1.tor"))
                    File.Move(GamePath + "\\Assets\\swtor_main_global_1.tor", GamePath + "\\Assets\\swtor_maln_global_1.tor");
                File.Move(GamePath + "\\Assets\\swtor_main_global_1_tmp.tor", GamePath + "\\Assets\\swtor_main_global_1.tor");
            }
        }

        private void Steam_Rename()//Переименование файлов для Steam версии
        {
            if (File.Exists(GamePath + "\\Assets\\swtor_ru-ww_global_1.tor") && File.Exists(GamePath + "\\Assets\\swtor_ru-wm_global_1.tor") && File.Exists(GamePath + "\\Assets\\swtor_maln_global_1.tor"))//Если русификатор был установлен - подменяем файлы
            {
                File.Move(GamePath + "\\Assets\\swtor_en-us_global_1.tor", GamePath + "\\Assets\\swtor_en-us_global_1_tmp.tor");
                File.Move(GamePath + "\\Assets\\swtor_main_global_1.tor", GamePath + "\\Assets\\swtor_main_global_1_tmp.tor");
                File.Move(GamePath + "\\Assets\\swtor_maln_global_1.tor", GamePath + "\\Assets\\swtor_main_global_1.tor");
                if (gender == 1)//Если персонаж мужской
                    File.Move(GamePath + "\\Assets\\swtor_ru-wm_global_1.tor", GamePath + "\\Assets\\swtor_en-us_global_1.tor");
                else
                    File.Move(GamePath + "\\Assets\\swtor_ru-ww_global_1.tor", GamePath + "\\Assets\\swtor_en-us_global_1.tor");
            }
        }

        private async void Form1_Shown(object sender, EventArgs e)//Выполняем загрузку недостающих модулей, проверку версии, количество новых строк перевода
        {
            LogBox.AppendText("Производится запуск русификатора...подождите...\n");
            Enabled = false;
            await Task.Run(() => Loading_info());
            Enabled = true;
        }
        private void Loading_info()
        {
            if (!File.Exists("db\\hashes_filename.txt"))//Проверяем наличие файла с хэшами игры
            {
                LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Отсутствует файл hashes_filename.txt...начинаем загрузку...\n")));
                Downloading_Files("https://drive.google.com/uc?export=download&id=19J51bZyJNLoFQ326-HvVOaZ4DFVpYErS", "db\\hashes_filename.txt");//Загружаем обновление
                LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Загрузка завершена...\n")));
            }
            if (!File.Exists("geckodriver.exe"))//Проверяем наличие драйвера для Firefox
            {
                LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Отсутствует файл geckodriver.exe...начинаем загрузку...\n")));
                Downloading_Files("https://drive.google.com/uc?export=download&id=1y3f42gldFXrisycqGboHrYzBRwcNQEbY", "geckodriver.exe"); //Загружаем обновление
                LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Загрузка завершена...\n")));
            }
            if (!File.Exists("db\\translate.db3"))
            {
                LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Отсутствует файл базы переводов translate.db3...начинаем загрузку...\n")));
                FileDownloader.DownloadFileFromURLToPath("https://drive.google.com/uc?export=download&id=1u_pmWfu655HUSLZMnLfUM7wyp1_AAzLJ", "db\\translate.db3");
                LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Загрузка завершена...\n")));
            }
            if (!File.Exists("WebDriver.dll"))
            {
                LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Отсутствует файл WebDriver.dll...начинаем загрузку...\n")));
                Downloading_Files("https://drive.google.com/uc?export=download&id=1lgTw0r1I85tg18Y_K3Rs5QMxYuEQR58Q", "WebDriver.dll"); //Загружаем обновление
                Downloading_Files("https://drive.google.com/uc?export=download&id=1Q74tFuTeb1MU_esTBLQ1EfLSRRxNoIOg", "WebDriver.xml"); //Загружаем обновление
                LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Загрузка завершена...\n")));
            }
            
            
            if (!File.Exists("db\\translate_backup.db3") || Config.AppSettings.Settings["backup_row"] == null)
                recover.Invoke((MethodInvoker)(() => recover.Enabled=false));
            if (!Directory.Exists("user_translation"))
                Directory.CreateDirectory("user_translation");
            if (Directory.GetFiles("user_translation\\", "*.xml").Length == 0)
                upload_to_server.Invoke((MethodInvoker)(() => upload_to_server.Enabled = false));
            LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Готово!\n")));
            LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Русификатор запущен! Приятного использования!\n")));
        }

        private async void Ins_font_Click(object sender, EventArgs e)//Устнавливаем только шрифты
        {
            Buttons_activity(0);
            if (Ins_font.Text == "Удалить шрифты" && RusFontsInstalled == 1 && File.Exists(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor_backup") && File.Exists(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor"))
            {
                LogBox.AppendText("Удаление шрифтов...\n");
                File.Delete(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor");
                File.Move(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor_backup", GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor");
                Ins_font.Text = "Установить шрифт";
                RusFontsInstalled = 0;
                LogBox.AppendText("Удаление шрифтов завершено!\n");
            }
            else
            {
                LogBox.AppendText("Установка шрифтов...\n");
                TryFix();
                await CopyFileAsync(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor", GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor_backup");
                byte[] array = File.ReadAllBytes("db\\fonts.gfx");
                int z = 290505;
                int f = 465311;
                FileStream fileStream = new FileStream(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor", FileMode.Open, FileAccess.ReadWrite);
                int num = EndOff1(fileStream);
                BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                uint num2 = (uint)fileStream.Length;
                binaryWriter.BaseStream.Position = num;
                binaryWriter.Write(num2);
                binaryWriter.Write(0);
                binaryWriter.Write(36);
                binaryWriter.Write(z);
                binaryWriter.Write(f);
                binaryWriter.Write(2844553745u);
                binaryWriter.Write(2571387593u);
                binaryWriter.Write(0);
                binaryWriter.Write((short)1);
                binaryWriter.BaseStream.Position = num2;
                binaryWriter.Write(array);
                binaryWriter.Close();
                fileStream.Close();
                Config.AppSettings.Settings["firstrun"].Value = "0";
                Config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                Ins_font.Text = "Удалить шрифты";
                RusFontsInstalled = 1;
                RusInstalled = 0;
                LogBox.AppendText("Установка шрифтов завершена!\n");
                Buttons_activity(1);
            }
        }
        public int EndOff(string filename)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            using (FileStream input = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(input);
                binaryReader.BaseStream.Seek(12L, SeekOrigin.Begin);
                uint num4 = binaryReader.ReadUInt32();
                binaryReader.BaseStream.Seek(24L, SeekOrigin.Begin);
                num3 = binaryReader.ReadInt32();
                int num5 = (int)Math.Ceiling((double)num3 / 1000.0);
                binaryReader.BaseStream.Seek(num4, SeekOrigin.Begin);
                long position = binaryReader.BaseStream.Position;
                int num6 = 0;
                while (num5 > 0)
                {
                    binaryReader.BaseStream.Position = position;
                    uint num7 = binaryReader.ReadUInt32();
                    if (num7 != 1000)
                        break;
                    position = binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    for (int i = 0; i < num7; i++)
                    {
                        binaryReader.ReadUInt32();
                        binaryReader.ReadUInt32();
                        binaryReader.ReadUInt32();
                        binaryReader.ReadUInt32();
                        binaryReader.ReadUInt32();
                        binaryReader.ReadUInt32();
                        uint num8 = binaryReader.ReadUInt32();
                        binaryReader.ReadUInt32();
                        binaryReader.ReadUInt16();
                        _ = binaryReader.BaseStream.Position;
                        num6++;
                        if (num8 == 2996987697u)
                            num++;
                        if (num8 == 2571387593u)
                            num2++;
                    }
                    num5--;
                }
            }
            if (num > 1)
                return 0;
            if (num2 > 1)
                return 0;
            if (filename.Contains("swtor_en-us_global_1") && num3 < 5312)
                return 2;
            if (filename.Contains("swtor_main_gfx_assets_1") && num3 < 84210)
                return 2;
            return 1;
        }
        public int EndOff1(FileStream fileStream)
        {
            int result = 0;
            BinaryReader binaryReader = new BinaryReader(fileStream);
            binaryReader.BaseStream.Seek(12L, SeekOrigin.Begin);
            uint num = binaryReader.ReadUInt32();
            binaryReader.BaseStream.Seek(24L, SeekOrigin.Begin);
            uint num2 = binaryReader.ReadUInt32();
            int num3 = (int)Math.Ceiling((double)num2 / 1000.0);
            binaryReader.BaseStream.Seek(num, SeekOrigin.Begin);
            long position = binaryReader.BaseStream.Position;
            int num4 = 0;
            long num5 = 0L;
            while (num3 > 0)
            {
                binaryReader.BaseStream.Position = position;
                uint num6 = binaryReader.ReadUInt32();
                if (num6 != 1000)
                {
                    break;
                }
                position = binaryReader.ReadUInt32();
                binaryReader.ReadUInt32();
                for (int i = 0; i < num6; i++)
                {
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt16();
                    num5 = binaryReader.BaseStream.Position;
                    num4++;
                    if (num4 == num2)
                    {
                        result = (int)num5 + 34;
                        break;
                    }
                }
                num3--;
            }
            return result;
        }

        private void Vk_link_Click(object sender, EventArgs e)
        {
            Process.Start("https://vk.com/club195326840");
        }

        private void Discord_link_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/E6adxtWWfd");
        }

        private void Auth_translate_CheckedChanged(object sender, EventArgs e)//Чекбокс изменения загрузки блокированных переводов
        {
            string js;
            if (auth_translate.Checked)
                js = "1";
            else
                js = "0";
            Config.AppSettings.Settings["auth_translate"].Value = js;
            Config.Save(ConfigurationSaveMode.Modified);
        }

        private void Config_Work()//Работаем с конфигурационным файлом (проверка выставление отметок в интерфейсе)
        {
            if (!File.Exists("SWToR_RUS.exe.Config"))//Проверяем существует ли файл конфигурации,если нет создаём новый файл конфигурации
            {
                CreateConfig();
                ConfigurationManager.RefreshSection("appSettings");
            }
            vpo.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString(); //Версия программы
                                                                                     //Далее проверяем наличие элементов конфига
            if (Config.AppSettings.Settings["a_translate"] == null)//Параметр отвечающий за автоматический перевод новых патчей
                Config.AppSettings.Settings.Add("a_translate", "0");
            if (Config.AppSettings.Settings["changes"] == null)//Параметр отвечающий за проверку изменений в текстах после новых патчей
                Config.AppSettings.Settings.Add("changes", "0");
            if (Config.AppSettings.Settings["author"] == null) //Параметр, в котором хранится имя Автора перевода
                Config.AppSettings.Settings.Add("author", "");
            if (Config.AppSettings.Settings["email"] == null) //Параметр, в котором хранится почта Автора перевода
                Config.AppSettings.Settings.Add("email", "");
            if (Config.AppSettings.Settings["password"] == null) //Параметр, в котором хранится пароль Автора перевода
                Config.AppSettings.Settings.Add("password", "");
            if (Config.AppSettings.Settings["backup_row"] == null) //Параметр, в котором хранится бэкап бд
                Config.AppSettings.Settings.Add("backup_row", "0");
            if (Config.AppSettings.Settings["auth_translate"] == null) //Параметр, отвечает за изменение заблокированных переводов
                Config.AppSettings.Settings.Add("auth_translate", "0");
            if (Config.AppSettings.Settings["translate_restrict"] == null)//Параметр, отвечает за блокировку переводов автора
                Config.AppSettings.Settings.Add("translate_restrict", "0");
            if (Config.AppSettings.Settings["items"] == null)//Параметр, отвечает за отключение предметов
                Config.AppSettings.Settings.Add("items", "0");
            if (Config.AppSettings.Settings["non_dialoge"] == null)//Параметр, отвечает за отключение предметов
                Config.AppSettings.Settings.Add("non_dialoge", "0");
            Config.Save(ConfigurationSaveMode.Minimal);//Сохраняем конфигурацию
            ConfigurationManager.RefreshSection("appSettings");//Обновляем конфиг для приложения
            gender = Int32.Parse(Config.AppSettings.Settings["gender"].Value);
            if (gender == 1)//Выставляем выключатели по конфигу
                ChooseMen.Checked = true;
            else
                ChooseWomen.Checked = true;
            if (Config.AppSettings.Settings["sith"].Value == "0")
                ChooseSith.Checked = true;
            else
                ChooseSit.Checked = true;
            if (Config.AppSettings.Settings["skill"].Value == "2")
                Dis_skills.Checked = true;
            else
                Dis_skills.Checked = false;
            if (Config.AppSettings.Settings["items"].Value == "1")
                Dis_items.Checked = true;
            else
                Dis_items.Checked = false;
            if (Config.AppSettings.Settings["non_dialoge"].Value == "1")
            {
                Dis_non_dialoge.Checked = true;
                Dis_skills.Checked = false;
                Dis_skills.Enabled = false;
                Dis_items.Checked = false;
                Dis_items.Enabled = false;
                auto_translate.Checked = false;
                auto_translate.Enabled = false;
                changes.Checked = false;
                changes.Enabled = false;
            }
            else
                Dis_non_dialoge.Checked = false;
            if (Config.AppSettings.Settings["google"].Value == "1")
                google_opt.Checked = true;
            else
            {
                google_opt.Checked = false;
                auto_translate.Checked = false;
                changes.Checked = false;
                auto_translate.Enabled = false;
                changes.Enabled = false;
                Config.AppSettings.Settings["a_translate"].Value = "0";
                Config.AppSettings.Settings["changes"].Value = "0";
            }
            if (Config.AppSettings.Settings["a_translate"].Value == "1" && Config.AppSettings.Settings["google"].Value == "1" && Config.AppSettings.Settings["skill"].Value == "0" && Config.AppSettings.Settings["items"].Value == "0")
            {
                auto_translate.Checked = true;
                if (Config.AppSettings.Settings["changes"].Value == "1")
                    changes.Checked = true;
                else
                    changes.Checked = false;
            }
            else
            {
                auto_translate.Checked = false;
                changes.Checked = false;
                changes.Enabled = false;
                Config.AppSettings.Settings["changes"].Value = "0";
                Config.AppSettings.Settings["a_translate"].Value = "0";
            }
            if (Config.AppSettings.Settings["auth_translate"].Value == "1")
                auth_translate.Checked = true;
            else
                auth_translate.Checked = false;
            GamePath = Config.AppSettings.Settings["gamepath"].Value;
            Config.Save(ConfigurationSaveMode.Modified);//Сохраняем конфигурацию
            ConfigurationManager.RefreshSection("appSettings");//Обновляем конфиг для приложения
        }

        private void Dis_items_CheckedChanged(object sender, EventArgs e)//Переключатель отключения перевода предметов и описаний
        {
            string js;
            if (Dis_items.Checked)
            {
                js = "1";
                auto_translate.Checked = false;
                auto_translate.Enabled = false;
                changes.Checked = false;
                changes.Enabled = false;
            }
            else
            {
                js = "0";
                if (google_opt.Checked == true)
                    auto_translate.Enabled = true;
            }
            Config.AppSettings.Settings["items"].Value = js;
            Config.Save(ConfigurationSaveMode.Modified);
        }

        private void Dis_non_dialoge_CheckedChanged(object sender, EventArgs e)//Переключатель отключения всего кроме диалогов
        {
            string js;
            if (Dis_non_dialoge.Checked)
            {
                js = "1";
                Dis_skills.Checked = false;
                Dis_skills.Enabled = false;
                Dis_items.Checked = false;
                Dis_items.Enabled = false;
                auto_translate.Checked = false;
                auto_translate.Enabled = false;
                changes.Checked = false;
                changes.Enabled = false;
            }
            else
            {
                js = "0";
                Dis_skills.Enabled = true;
                Dis_items.Enabled = true;
                if (google_opt.Checked == true)
                    auto_translate.Enabled = true;
            }
            Config.AppSettings.Settings["non_dialoge"].Value = js;
            Config.Save(ConfigurationSaveMode.Modified);
        }

        public void Buttons_activity(int what_do)
        {
            if (what_do==0)
            {
                Install_btn.Enabled = false;
                Ins_font.Enabled = false;
                del_btn.Enabled = false;
                upload_to_server.Enabled = false;
                upload_from_server.Enabled = false;
                recover.Enabled = false;
                editor_btn.Enabled = false;
                ChangePathButton.Enabled = false;
                btn_info.Enabled = false;
            }
            else if (what_do==1)
            {
                if (updatedownload.Text != "Соединение установлено! Новых переводов не обнаружено!")
                    upload_from_server.Enabled = true;
                if (Directory.GetFiles("user_translation\\", "*.xml").Length >0)
                    upload_to_server.Enabled = true;
                if (File.Exists("db\\translate_backup.db3"))
                    recover.Enabled = true;
                editor_btn.Enabled = true;
                ChangePathButton.Enabled = true;
                btn_info.Enabled = true;
                Install_btn.Enabled = true;
                if (RusInstalled == 1)
                    del_btn.Enabled = true;
                else
                    Ins_font.Enabled = true;
            }
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            if (Form2.upload_to_server_info == 1)
            {
                upload_to_server.Enabled = true;
                Form2.upload_to_server_info = 0;
            }
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {
            Enabled = false;
            LogBox.AppendText("Проверка наличия обновлений...\n");
            //Проверяем наличие новой версии
            try
            {
                if (File.Exists(CurDir + "updater.exe"))//Если остался старый патч, удаляем
                    File.Delete(CurDir + "updater.exe");
                if (!Directory.Exists("tmp"))//Создаём временную директорию
                    Directory.CreateDirectory("tmp");
                if (File.Exists(CurDir + "tmp\\info.txt"))//Удаляем файл с информацией о последнем патче
                    File.Delete(CurDir + "tmp\\info.txt");
                Downloading_Files("https://drive.google.com/uc?export=download&id=1-K2Zv8mzDztoQSOh30Yvcy3Qg6TACqS2", "tmp\\info.txt");//Загружаем файл с информацией о версии приложения
                string version_current = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string version_new = version_current;
                version_new = File.ReadLines("tmp\\info.txt").Skip(0).First();//Считываем первую строку, в ней указана версия
                if (version_new != version_current)//Показываем ссылку обновлеия программы
                {
                    Updater.Visible = true;
                    Update_app.Visible = true;
                }
            }
            catch (Exception es)//Отлавливаем ошибку, в случае если она возникает
            {
                Logging(es.Message);//Записываем в лог ошибку
                LogBox.AppendText("Не удалось проверить наличие новой версии. Следите за обновлениями в группе ВК или в Discord канале.\n");
            }

            MySqlConnection MysSQL_Connection = new MySqlConnection(connStr_mysql); //Объявляем соединение с БД
            string sql = "";
            try
            {
                MysSQL_Connection.Open();//Устанавливаем соединение с БД
                sql = "SELECT COUNT(*) FROM translated WHERE tr_datetime>STR_TO_DATE('" + Config.AppSettings.Settings["row_updated_from_server"].Value + "', '%d.%m.%Y %H:%i:%s')";
                MySqlCommand command = new MySqlCommand(sql, MysSQL_Connection);
                int counrow = Int32.Parse(command.ExecuteScalar().ToString());//Получаем количество записей в таблице пользовательских переводов
                command.Dispose();
                MysSQL_Connection.Close();//Закрываем соединение с удалённой базой
                if (counrow == 0)
                {
                    updatedownload.Invoke((MethodInvoker)(() => updatedownload.Text = "Соединение установлено! Новых переводов не обнаружено!"));
                    updatedownload.Invoke((MethodInvoker)(() => updatedownload.ForeColor = Color.Green));
                    upload_from_server.Invoke((MethodInvoker)(() => upload_from_server.Enabled = false));
                }
                else
                {
                    updatedownload.Invoke((MethodInvoker)(() => updatedownload.Text = "Соединение установлено! С последнего обновления обнаружено " + counrow + " новых строк перевода!"));
                    updatedownload.Invoke((MethodInvoker)(() => updatedownload.ForeColor = Color.Blue));
                    upload_from_server.Invoke((MethodInvoker)(() => upload_from_server.Enabled = true));
                }
            }
            catch (Exception eddd)//Отлавливаем ошибки
            {
                Logging(eddd.Message); //Записываем в лог ошибку                
                updatedownload.Invoke((MethodInvoker)(() => updatedownload.Text = "Не удалось соединиться с удалённой базой!"));
            }
            LogBox.AppendText("Готово.\n");
            Enabled = true;


        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            Process.Start("https://funpay.ru/users/1912764/");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cou = 0;
            FileStream fileStream = new FileStream("main_gfx_1.tor", FileMode.Open, FileAccess.ReadWrite);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            binaryReader.BaseStream.Seek(12L, SeekOrigin.Begin);
            uint num = binaryReader.ReadUInt32();
            binaryReader.BaseStream.Seek(24L, SeekOrigin.Begin);
            filescount = binaryReader.ReadUInt32();
            endtable = 0;
            endtable = EndOff(fileStream);
            int num2 = (int)Math.Ceiling((double)filescount / 1000.0);
            binaryReader.BaseStream.Seek(num, SeekOrigin.Begin);
            long position = binaryReader.BaseStream.Position;
            while (num2 > 0)
            {
                binaryReader.BaseStream.Position = position;
                uint num3 = binaryReader.ReadUInt32();
                if (num3 != 1000)
                    break;
                if (binaryReader.ReadUInt32() == 0)
                    lastoffes = (int)binaryReader.BaseStream.Position - 8;
                binaryReader.ReadUInt32();
                position = binaryReader.BaseStream.Position;
                for (int i = 0; i < num3; i++)
                {
                    binaryReader.BaseStream.Position = position;
                    uint num4 = binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    uint zsize = binaryReader.ReadUInt32();
                    uint size = binaryReader.ReadUInt32();
                    uint hash = binaryReader.ReadUInt32();
                    uint num5 = binaryReader.ReadUInt32();
                    binaryReader.ReadUInt32();
                    binaryReader.ReadUInt16();
                    position = binaryReader.BaseStream.Position;
                    if (num4 == 0)
                        break;

                    //Console.WriteLine(num4 + "---" + zsize + "---" + size + "---" + hash + "---" + num5.ToString("X") + "---" + num5);
                    if (num5.ToString("X") == "F0072695")
                    {
                        Console.WriteLine(num4 + "---" + zsize + "---" + size + "---" + hash + "---" + num5.ToString("X") + "---" + num5);

                        Console.WriteLine("aaaaaaaaaaaaaaa");
                        Packfont2(fileStream, num4, zsize, size, hash, num5, endtable, "bwaui_charactersheet.gfx");
                    }
                    /*if (num5.ToString("X") == "994442C9" )
                    {
                        Packfont(fileStream, num4, zsize, size, hash, num5, endtable, "swtor_fonts.gfx");
                        Console.WriteLine("aaaaaaaaaaaaaaa");
                    }
                    if (num5.ToString("X") == "EC874439")
                    { 
                        Packfont(fileStream, num4, zsize, size, hash, num5, endtable, "drawtext_fonts.swf");
                        Console.WriteLine("aaaaaaaaaaaaaaa");
                    }*/
                }
                num2--;
            }
            int num6 = (int)filescount % 1000;
            if (cou + num6 + 1 > 1000)
            {
                BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                binaryWriter.BaseStream.Position = lastoffes;
                binaryWriter.Write(cou + num6 + 1);
                binaryWriter.Close();
                fileStream.Close();
            }
            Console.WriteLine("----");
        }
        public void Packfont2(FileStream fileStream, uint off, uint zsize, uint size, uint hash2, uint hash1, int endtable, string file_font_new)
        {
            BinaryReader binaryReader = new BinaryReader(fileStream);
            binaryReader.BaseStream.Position = off;
            binaryReader.ReadBytes(36);
            byte[] input = binaryReader.ReadBytes((int)zsize);
            byte[] buffer = new byte[size];

            using (var fs = new FileStream("test.gfx", FileMode.Create, FileAccess.Write))
            {
                fs.Write(input, 0, input.Length);
            }


            Console.WriteLine(off + "---" + zsize + "---" + size + "---" + hash2 + "---" + hash1 + "---" + endtable);


            MemoryStream memoryStream = new MemoryStream();
            using (FileStream file = new FileStream(file_font_new, FileMode.Open, FileAccess.Read))
                file.CopyTo(memoryStream);
            MemoryStream memoryStream2 = new MemoryStream();
            Deflater deflater = new Deflater(9);
            deflater.SetInput(memoryStream.ToArray());
            deflater.Finish();
            byte[] array = new byte[size * 3];
            while (!deflater.IsNeedingInput)
            {
                int count = deflater.Deflate(array);
                memoryStream2.Write(array, 0, count);
                if (deflater.IsFinished)
                    break;
            }
            deflater.Reset();
            int newsize = memoryStream.ToArray().Length;
            int newzsize = memoryStream2.ToArray().Length;

            byte[] array2 = new byte[36];
            array2[0] = 2;
            array2[2] = 32;
            byte[] array3 = new byte[array2.Length + memoryStream2.ToArray().Length];
            array2.CopyTo(array3, 0);
            memoryStream2.ToArray().CopyTo(array3, array2.Length);

            Console.WriteLine(off + "---" + newzsize + "---" + newsize + "---" + hash2 + "---" + hash1 + "---" + endtable);
            Hhh(fileStream, array3, newzsize, newsize, hash2, hash1, endtable);


        }
    }

    public static class NetworkShare
    {
        /// <summary>
        /// Connects to the remote share
        /// </summary>
        /// <returns>Null if successful, otherwise error message.</returns>
        public static string ConnectToShare(string uri, string username, string password)
        {
            //Create netresource and point it at the share
            NETRESOURCE nr = new NETRESOURCE
            {
                dwType = RESOURCETYPE_DISK,
                lpRemoteName = uri
            };

            //Create the share
            int ret = WNetUseConnection(IntPtr.Zero, nr, password, username, 0, null, null, null);

            //Check for errors
            if (ret == NO_ERROR)
                return null;
            else
                return GetError(ret);
        }

        /// <summary>
        /// Remove the share from cache.
        /// </summary>
        /// <returns>Null if successful, otherwise error message.</returns>
        public static string DisconnectFromShare(string uri, bool force)
        {
            //remove the share
            int ret = WNetCancelConnection(uri, force);

            //Check for errors
            if (ret == NO_ERROR)
                return null;
            else
                return GetError(ret);
        }

        #region P/Invoke Stuff
        [DllImport("Mpr.dll")]
        private static extern int WNetUseConnection(
            IntPtr hwndOwner,
            NETRESOURCE lpNetResource,
            string lpPassword,
            string lpUserID,
            int dwFlags,
            string lpAccessName,
            string lpBufferSize,
            string lpResult
            );

        [DllImport("Mpr.dll")]
        private static extern int WNetCancelConnection(
            string lpName,
            bool fForce
            );

        [StructLayout(LayoutKind.Sequential)]
        private class NETRESOURCE
        {
            public int dwScope = 0;
            public int dwType = 0;
            public int dwDisplayType = 0;
            public int dwUsage = 0;
            public string lpLocalName = "";
            public string lpRemoteName = "";
            public string lpComment = "";
            public string lpProvider = "";
        }

        #region Consts
        const int RESOURCETYPE_DISK = 0x00000001;
        const int CONNECT_UPDATE_PROFILE = 0x00000001;
        #endregion

        #region Errors
        const int NO_ERROR = 0;

        const int ERROR_ACCESS_DENIED = 5;
        const int ERROR_ALREADY_ASSIGNED = 85;
        const int ERROR_BAD_DEVICE = 1200;
        const int ERROR_BAD_NET_NAME = 67;
        const int ERROR_BAD_PROVIDER = 1204;
        const int ERROR_CANCELLED = 1223;
        const int ERROR_EXTENDED_ERROR = 1208;
        const int ERROR_INVALID_ADDRESS = 487;
        const int ERROR_INVALID_PARAMETER = 87;
        const int ERROR_INVALID_PASSWORD = 1216;
        const int ERROR_MORE_DATA = 234;
        const int ERROR_NO_MORE_ITEMS = 259;
        const int ERROR_NO_NET_OR_BAD_PATH = 1203;
        const int ERROR_NO_NETWORK = 1222;
        const int ERROR_SESSION_CREDENTIAL_CONFLICT = 1219;

        const int ERROR_BAD_PROFILE = 1206;
        const int ERROR_CANNOT_OPEN_PROFILE = 1205;
        const int ERROR_DEVICE_IN_USE = 2404;
        const int ERROR_NOT_CONNECTED = 2250;
        const int ERROR_OPEN_FILES = 2401;

        private struct ErrorClass
        {
            public int num;
            public string message;
            public ErrorClass(int num, string message)
            {
                this.num = num;
                this.message = message;
            }
        }

        private static ErrorClass[] ERROR_LIST = new ErrorClass[] {
        new ErrorClass(ERROR_ACCESS_DENIED, "Error: Access Denied"),
        new ErrorClass(ERROR_ALREADY_ASSIGNED, "Error: Already Assigned"),
        new ErrorClass(ERROR_BAD_DEVICE, "Error: Bad Device"),
        new ErrorClass(ERROR_BAD_NET_NAME, "Error: Bad Net Name"),
        new ErrorClass(ERROR_BAD_PROVIDER, "Error: Bad Provider"),
        new ErrorClass(ERROR_CANCELLED, "Error: Cancelled"),
        new ErrorClass(ERROR_EXTENDED_ERROR, "Error: Extended Error"),
        new ErrorClass(ERROR_INVALID_ADDRESS, "Error: Invalid Address"),
        new ErrorClass(ERROR_INVALID_PARAMETER, "Error: Invalid Parameter"),
        new ErrorClass(ERROR_INVALID_PASSWORD, "Error: Invalid Password"),
        new ErrorClass(ERROR_MORE_DATA, "Error: More Data"),
        new ErrorClass(ERROR_NO_MORE_ITEMS, "Error: No More Items"),
        new ErrorClass(ERROR_NO_NET_OR_BAD_PATH, "Error: No Net Or Bad Path"),
        new ErrorClass(ERROR_NO_NETWORK, "Error: No Network"),
        new ErrorClass(ERROR_BAD_PROFILE, "Error: Bad Profile"),
        new ErrorClass(ERROR_CANNOT_OPEN_PROFILE, "Error: Cannot Open Profile"),
        new ErrorClass(ERROR_DEVICE_IN_USE, "Error: Device In Use"),
        new ErrorClass(ERROR_EXTENDED_ERROR, "Error: Extended Error"),
        new ErrorClass(ERROR_NOT_CONNECTED, "Error: Not Connected"),
        new ErrorClass(ERROR_OPEN_FILES, "Error: Open Files"),
        new ErrorClass(ERROR_SESSION_CREDENTIAL_CONFLICT, "Error: Credential Conflict"),
    };

        private static string GetError(int errNum)
        {
            foreach (ErrorClass er in ERROR_LIST)
            {
                if (er.num == errNum) return er.message;
            }
            return "Error: Unknown, " + errNum;
        }
        #endregion

        #endregion
    }

}
