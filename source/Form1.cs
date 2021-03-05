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
using System.Net;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenQA.Selenium;

namespace SWToR_RUS
{
    public partial class Form1 : Form
    {
        public string CurDir = AppDomain.CurrentDomain.BaseDirectory; //Путь к программе

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

        public int is_run = 1;

        public IWebDriver driver;

        private ManagementEventWatcher startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));

        private ProcessStartInfo startInfo = new ProcessStartInfo();

        public Form1()
        {
            InitializeComponent();
            /*App_Updater();*/
            if (is_run == 1)
            {
                if (!File.Exists("SWToR_RUS.exe.Config")) //Проверяем существует ли файл конфига
                {
                    CreateConfig(); //Создаём новый файл конфигурации
                    ConfigurationManager.RefreshSection("appSettings");
                }
                vpo.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString(); //Версия программы
                //Далее проверяем наличие элементов конфига
                if (Config.AppSettings.Settings["a_translate"] == null) //Параметр отвечающий за автоматический перевод новых патчей
                {
                    Config.AppSettings.Settings.Add("a_translate", "0"); //Добавляем отсутствующий параметр
                    Config.Save(ConfigurationSaveMode.Minimal); //Сохраняем конфиг
                    ConfigurationManager.RefreshSection("appSettings"); // Обновляем конфиг для приложения
                }
                if (Config.AppSettings.Settings["changes"] == null) //Параметр отвечающий за проверку изменений в текстах после новых патчей
                {
                    Config.AppSettings.Settings.Add("changes", "0");
                    Config.Save(ConfigurationSaveMode.Minimal);
                    ConfigurationManager.RefreshSection("appSettings");
                }
                if (ConfigurationManager.AppSettings["backup_row"] == null) //Параметр, в котором хранится бэкап бд
                {
                    Config.AppSettings.Settings.Add("backup_row", "0");
                    Config.Save(ConfigurationSaveMode.Minimal);
                    ConfigurationManager.RefreshSection("appSettings");
                }
                if (ConfigurationManager.AppSettings["auth_translate"] == null) //Параметр, отвечает заизменение заблокированных переводов
                {
                    Config.AppSettings.Settings.Add("auth_translate", "0");
                    Config.Save(ConfigurationSaveMode.Minimal);
                    ConfigurationManager.RefreshSection("appSettings");
                }
                if (ConfigurationManager.AppSettings["translate_restrict"] == null) //Параметр, отвечает заизменение заблокированных переводов
                {
                    Config.AppSettings.Settings.Add("translate_restrict", "0");
                    Config.Save(ConfigurationSaveMode.Minimal);
                    ConfigurationManager.RefreshSection("appSettings");
                }
                ManagementClass managementClass = new ManagementClass("Win32_Process"); // Смотрим запущен ли лаучер игры
                foreach (ManagementObject instance in managementClass.GetInstances())
                {
                    if (instance["Name"].Equals("launcher.exe"))
                    {
                        launcher_status.Text = "Лаунчер SWToR запущен";
                        launcher_status.ForeColor = Color.Green;
                        launch_status = 1;
                        break; //Лаунчер найден - прерываем перебор запущенных процессов
                    }
                }
                if (launch_status == 0) //Если лаунчер не найден - сообщаем об этом и блокируем часть действий
                {
                    launcher_status.Text = "Лаунчер SWToR не запущен";
                    launcher_status.ForeColor = Color.Red;
                }

                GamePath = Config.AppSettings.Settings["gamepath"].Value;

                if (GamePath == "" || launch_status == 0)
                    Install_btn.Enabled = false;

                if (File.Exists(GamePath + "\\Assets\\swtor_en-us_global_1_tmp.tor") || File.Exists(GamePath + "\\Assets\\swtor_main_global_1_tmp.tor"))
                    TryFix(); //Видимо русификатор некорректно завершил работу, пытаемся вернуть оригинальные файлы на место
                
                if(File.Exists(GamePath + "\\Assets\\swtor_maln_gfx_assets_1.tor")) //Запоминаем если русификатор уже стоит
                    RusInstalled = 1;

                if (File.Exists(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor_backup")) //Запоминаем если установлены только шрифты
                    RusFontsInstalled = 1;

                if (GamePath != "") //Если есть путь к расположению игры
                {
                    if (File.Exists(GamePath + "launcher.exe"))// Проверяем если игра в этой папке
                    {
                        GamePathTextBox.Text = GamePath;
                        if (GamePath.ToLower().IndexOf("steamapps") > 0) //Проверяем какая версия игры (Steam или нет)
                            steam_game.Checked = true;
                        else
                            steam_game.Checked = false;
                        if (Config.AppSettings.Settings["firstrun"].Value == "1") //Если это первый запуск программы
                            del_btn.Enabled = false;
                        else
                        {
                            string hash_in_config = Config.AppSettings.Settings["hash"].Value; //Считываем хэши из конфига и оригинального файла
                            if (hash_in_config != "")
                            {
                                string hash_original_file;
                                hash_original_file = CalculateMD5(GamePath + "\\Assets\\swtor_main_global_1.tor");
                                if (hash_in_config == hash_original_file || RusInstalled == 1) //Если хэши совпадают отключаем внопку Установки
                                {
                                    Install_btn.Text = "Переустановить";
                                    del_btn.Enabled = true;
                                    ins_font.Enabled = false;
                                }
                                else
                                    LogBox.AppendText("Hash не совпадает. Проведите переустановку русификатора.\n");
                            }
                            if (RusFontsInstalled == 1)
                                ins_font.Text = "Удалить шрифты";
                        }
                        if (steam_game.Checked == true && launch_status == 1 && RusInstalled == 1) //Если версия Steam, Лаунчер запушен и русификатор установлен
                            Steam_Rename();
                    }
                }
                if (Config.AppSettings.Settings["gender"].Value == "1") //Выставляем выключатели по конфигу
                    ChooseMen.Checked = true;
                else
                {
                    ChooseWomen.Checked = true;
                    gender = 0;
                    Config.AppSettings.Settings["gender"].Value = "0";
                    Config.Save(ConfigurationSaveMode.Modified);
                }
                if (Config.AppSettings.Settings["sith"].Value == "0")
                    ChooseSith.Checked = true;
                else
                {
                    ChooseSit.Checked = true;
                    Config.AppSettings.Settings["sith"].Value = "1";
                    Config.Save(ConfigurationSaveMode.Modified);
                }
                if (Config.AppSettings.Settings["skill"].Value == "2")
                    dis_skills.Checked = true;
                else
                {
                    dis_skills.Checked = false;
                    Config.AppSettings.Settings["skill"].Value = "0";
                    Config.Save(ConfigurationSaveMode.Modified);
                }
                if (Config.AppSettings.Settings["google"].Value == "1")
                {
                    google_opt.Checked = true;
                    auto_translate.Enabled = true;
                }
                else
                {
                    auto_translate.Enabled = false;
                    google_opt.Checked = false;
                    Config.AppSettings.Settings["google"].Value = "0";
                    Config.Save(ConfigurationSaveMode.Modified);
                }
                if (Config.AppSettings.Settings["a_translate"].Value == "1" && Config.AppSettings.Settings["google"].Value == "1" && Config.AppSettings.Settings["skill"].Value == "0")
                    auto_translate.Checked = true;
                else
                {
                    auto_translate.Checked = false;
                    Config.AppSettings.Settings["a_translate"].Value = "0";
                    Config.Save(ConfigurationSaveMode.Modified);
                }
                if (Config.AppSettings.Settings["a_translate"].Value == "1" && Config.AppSettings.Settings["google"].Value == "1" && Config.AppSettings.Settings["skill"].Value == "0" && Config.AppSettings.Settings["changes"].Value == "1")
                    changes.Checked = true;
                else
                {
                    changes.Checked = false;
                    if (Config.AppSettings.Settings["a_translate"].Value == "0")
                        changes.Enabled = false;
                    Config.AppSettings.Settings["changes"].Value = "0";
                    Config.Save(ConfigurationSaveMode.Modified);
                }
                if (Config.AppSettings.Settings["auth_translate"].Value == "1")
                    auth_translate.Checked = true;
                else
                    auth_translate.Checked = false;
                startWatch.EventArrived += startWatch_EventArrived; //Начинаем следить за процессами, чтобы отловить игру или лаунчер
                startWatch.Start();
            }
        }
        public void startWatch_EventArrived(object sender, EventArrivedEventArgs e) //Остлеживаем появление процессов
        {
            if (e.NewEvent.Properties["ProcessName"].Value.ToString() == "swtor.exe") //Отслеживаем запуск игр
            {
                if (steam_game.Checked == false) //Если не steam версия на ходу подменяем файлы
                {
                    startWatch.Stop();
                    Rus_for_orinal_game();
                }
            }

            if (e.NewEvent.Properties["ProcessName"].Value.ToString() == "launcher.exe") //Отслеживаем запуск лаунчера
            {
                Invoke((MethodInvoker)delegate
                {
                    launcher_status.Text = "Лаунчер SWToR запущен";
                    launcher_status.ForeColor = Color.Green;
                    if (GamePath != "")
                    {
                        if (RusInstalled != 1)
                        {
                            Install_btn.Enabled = true;
                            del_btn.Enabled = false;
                        }
                        else
                            del_btn.Enabled = true;
                    }
                    if (steam_game.Checked == true && RusInstalled == 1) //Если версия Steam //Производим подготовку файлов для запуска игры, если русификатор установлен
                        Steam_Rename();
                });
            }
        }
        public async void Rus_for_orinal_game() //Для обычной версии программа сама отслеживает запуск игры
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
            if (gender == 1) //Подменяем аргументы в зависимости от пола персонажа
                arg = arg.Replace("main,en-us", "maln,ru-wm");
            else if (gender == 0)
                arg = arg.Replace("main,en-us", "maln,ru-ww");
            startInfo.FileName = heh1;
            startInfo.Arguments = arg;
            startInfo.WorkingDirectory = work;
            string hash_in_config = Config.AppSettings.Settings["hash"].Value;
            string hash_original = CalculateMD5(GamePath + "\\Assets\\swtor_main_global_1.tor");
            if (hash_in_config == hash_original) //Повторно запускаем игру с подменёнными параметрами
                Process.Start(startInfo);
            else //Хэши не совпадают, запускаем переустановку русификатора в автоматическом режиме
            {
                Thread thread = new Thread((ThreadStart)delegate
                {
                    LogBox.AppendText("Hash не совпадает. Производится переустановка русификатора и игра запустится автоматически.\n");
                });
                thread.IsBackground = true;
                thread.Start();
                await install();
                Process.Start(startInfo);
            }
        }

        public void CreateConfig() //Создаём конфигурационный файл
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
            stringBuilder.AppendLine("    <add key=\"google\" value=\"0\" />");
            stringBuilder.AppendLine("    <add key=\"a_translate\" value=\"0\" />");
            stringBuilder.AppendLine("    <add key=\"changes\" value=\"0\" />");
            stringBuilder.AppendLine("    <add key=\"author\" value=\"\" />");
            stringBuilder.AppendLine("    <add key=\"password\" value=\"\" />");
            stringBuilder.AppendLine("    <add key=\"email\" value=\"\" />");
            stringBuilder.AppendLine("    <add key=\"backup_row\" value=\"0\" />");
            stringBuilder.AppendLine("    <add key=\"auth_translate\" value=\"0\" />");
            stringBuilder.AppendLine("    <add key=\"row_updated_from_server\" value=\"23.08.2020 16:45:04\" />");
            stringBuilder.AppendLine("  </appSettings>");
            stringBuilder.AppendLine("</configuration>");
            File.WriteAllText(Assembly.GetEntryAssembly().Location + ".config", stringBuilder.ToString());
        }
        private async void Install_btn_Click(object sender, EventArgs e) //Устанавливаем русификатор
        {
            Install_btn.Enabled = false; //Отключаем на время установки все элементы
            del_btn.Enabled = false;
            ChangePathButton.Enabled = false;
            ChooseSith.Enabled = false;
            ChooseSit.Enabled = false;
            ChooseMen.Enabled = false;
            ChooseWomen.Enabled = false;
            dis_skills.Enabled = false;
            btn_info.Enabled = false;
            google_opt.Enabled = false;
            auto_translate.Enabled = false;
            changes.Enabled = false;
            recover.Enabled = false;
            ins_font.Enabled = false;
            auth_translate.Enabled = false;
            await install();
        }
        public async Task install() //Установка русификатора
        {
            if (RusFontsInstalled == 1 && File.Exists(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor_backup") && File.Exists(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor"))
            {
                File.Delete(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor");
                File.Move(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor_backup", GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor");
                ins_font.Text = "Установить шрифт";
            }
            if (steam_game.Checked == true) //Возвращаем файлы в исходное положение
                TryFix();
            int num = EndOff(GamePath + "\\Assets\\swtor_en-us_global_1.tor"); //Проверяем оригинальные ли файлы игры
            int num2 = EndOff(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor");
            if (num == 0 || num2 == 0)
                LogBox.AppendText("Оригинальные файлы повреждены! Восспользуйтесь функцией проверки в лаунчере игры.\n");
            else if (num == 2 || num2 == 2)
                LogBox.AppendText("Необходимо обновить игру перед установкой русификатора!\n");
            else
            {
                LogBox.AppendText(Properties.Resources.patchhosts);
                PatchHosts(); //Патчим host файл, чтобы заблокировать отправку отчётов
                LogBox.AppendText(Properties.Resources.copyfiles);
                await CopyfilesAsync(); //Копируем оригинальные файлы
                LogBox.AppendText(Properties.Resources.Done + "\n");
                LogBox.AppendText(Properties.Resources.Patch);
                Patch patch = new Patch();
                await Task.Run(delegate
                {
                    patch.ConnectDB(); //Патчим файлы
                });
                LogBox.AppendText(Properties.Resources.Done + "\n");                
                Config.AppSettings.Settings["firstrun"].Value = "0";
                Config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                if (steam_game.Checked == true)
                    Steam_Rename();
                Install_btn.Text = "Переустановить";//Включаем элементы и переименовываем кнопку Установки
                del_btn.Enabled = true;
                ChangePathButton.Enabled = true;
                ChooseSith.Enabled = true;
                ChooseSit.Enabled = true;
                ChooseMen.Enabled = true;
                ChooseWomen.Enabled = true;
                btn_info.Enabled = true;
                recover.Enabled = true;
                dis_skills.Enabled = true;
                google_opt.Enabled = true;
                Install_btn.Enabled = true;
                if (google_opt.Checked == true)
                    auto_translate.Enabled = true;
                if (auto_translate.Checked == true)
                    changes.Enabled = true;
                auth_translate.Enabled = true;
                LogBox.AppendText("Установка закончена.\n");
            }
        }
        private void PatchHosts() //Патчер host файла
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
            ProgressBar1.Value += 3;
            await CopyFileAsync(GamePath + "\\Assets\\swtor_main_global_1.tor", GamePath + "\\Assets\\swtor_maln_global_1.tor");
            string hash_original = CalculateMD5(GamePath + "\\Assets\\swtor_main_global_1.tor");
            ProgressBar1.Value += 4;
            Config.AppSettings.Settings["hash"].Value = hash_original;
            Config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        public async Task CopyFileAsync(string sourcePath, string destinationPath) //Ассинхронное копирование файлов
        {
            using (Stream source = File.OpenRead(sourcePath))
            {
                using (Stream destination = File.Create(destinationPath))
                {
                    await source.CopyToAsync(destination);
                }
            }
        }
        public static string CalculateMD5(string filename)
        {
            using (MD5 mD = MD5.Create())
            {
                using (FileStream inputStream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(mD.ComputeHash(inputStream)).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        private void ChangePathButton_Click(object sender, EventArgs e) //Обработка смена пути к игре
        {
            ChoosePath();
        }
        private void ChoosePath()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Укажите путь к SWToR";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                if (folderBrowserDialog.SelectedPath.ToLower().IndexOf("steamapps") > 0 || folderBrowserDialog.SelectedPath.ToLower().IndexOf("SteamApps") > 0) //Проверяем путь к обычной версии или к Steam
                    steam_game.Checked = true;
                else if (steam_game.Checked == true)
                    steam_game.Checked = false;
                GamePath = folderBrowserDialog.SelectedPath;
                if (!GamePath.EndsWith("\\"))
                    GamePath += "\\";
                if (launch_status == 1)
                {
                    TryFix();
                    Steam_Rename();
                }
                GamePathTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }
        private void ChooseSith_CheckedChanged(object sender, EventArgs e) //Переключатель варианта переводс Ситх\Сит
        {
            Config.AppSettings.Settings["sith"].Value = "0";
            Config.Save(ConfigurationSaveMode.Modified);
        }
        private void ChooseSit_CheckedChanged(object sender, EventArgs e) //Переключатель варианта переводс Ситх\Сит
        {
            Config.AppSettings.Settings["sith"].Value = "1";
            Config.Save(ConfigurationSaveMode.Modified);
        }
        private void ChooseMen_CheckedChanged(object sender, EventArgs e) //Переключатель мужской\женский персонаж
        {
            Config.AppSettings.Settings["gender"].Value = "1";
            Config.Save(ConfigurationSaveMode.Modified);
            gender = 1;
            if (steam_game.Checked == true)
            {
                if (File.Exists(GamePath + "\\Assets\\swtor_en-us_global_1_tmp.tor"))
                {
                    if (File.Exists(GamePath + "\\Assets\\swtor_ru-ww_global_1.tor"))
                        File.Move(GamePath + "\\Assets\\swtor_en-us_global_1.tor", GamePath + "\\Assets\\swtor_ru-wm_global_1.tor");
                    else if (File.Exists(GamePath + "\\Assets\\swtor_ru-wm_global_1.tor"))
                        File.Move(GamePath + "\\Assets\\swtor_en-us_global_1.tor", GamePath + "\\Assets\\swtor_ru-ww_global_1.tor");
                    File.Move(GamePath + "\\Assets\\swtor_ru-wm_global_1.tor", GamePath + "\\Assets\\swtor_en-us_global_1.tor");
                }
            }
        }
        private void ChooseWomen_CheckedChanged(object sender, EventArgs e) //Переключатель мужской\женский персонаж
        {
            Config.AppSettings.Settings["gender"].Value = "0";
            Config.Save(ConfigurationSaveMode.Modified);
            gender = 0;
            if (steam_game.Checked == true)
            {
                if (File.Exists(GamePath + "\\Assets\\swtor_en-us_global_1_tmp.tor"))
                {
                    if (File.Exists(GamePath + "\\Assets\\swtor_ru-ww_global_1.tor"))
                        File.Move(GamePath + "\\Assets\\swtor_en-us_global_1.tor", GamePath + "\\Assets\\swtor_ru-wm_global_1.tor");
                    else if (File.Exists(GamePath + "\\Assets\\swtor_ru-wm_global_1.tor"))
                        File.Move(GamePath + "\\Assets\\swtor_en-us_global_1.tor", GamePath + "\\Assets\\swtor_ru-ww_global_1.tor");
                    File.Move(GamePath + "\\Assets\\swtor_ru-ww_global_1.tor", GamePath + "\\Assets\\swtor_en-us_global_1.tor");
                }
            }
        }
        private void GamePathTextBox_TextChanged(object sender, EventArgs e) //Проверяем изменение пути к игре
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
            Config.AppSettings.Settings["gamepath"].Value = NewPath;
            Config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        
        private void del_btn_Click(object sender, EventArgs e) //Удаляем русификатор
        {
            Install_btn.Enabled = false; //Отключаем временно все элементы
            del_btn.Enabled = false;
            ChangePathButton.Enabled = false;
            ChooseSith.Enabled = false;
            ChooseSit.Enabled = false;
            ChooseMen.Enabled = false;
            ChooseWomen.Enabled = false;
            dis_skills.Enabled = false;
            btn_info.Enabled = false;
            google_opt.Enabled = false;
            auto_translate.Enabled = false;
            changes.Enabled = false;
            recover.Enabled = false;        
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
            Config.AppSettings.Settings["firstrun"].Value = "1";
            Config.Save(ConfigurationSaveMode.Modified);
            LogBox.AppendText(Properties.Resources.Done + "\n");
            LogBox.AppendText("Удаление закончено.\n");
            Install_btn.Enabled = true;
            ins_font.Enabled = true;
            del_btn.Enabled = false;
            Application.Exit();
        }
        private void db_convertor_Click(object sender, EventArgs e)
        {
            string line;
            string line2 = "";
            using (SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; "))
            {
                sqlite_conn.Open();
                using (SQLiteCommand sqlite_cmd = new SQLiteCommand(sqlite_conn))
                {
                    StreamReader file = new StreamReader("db\\deepl_trans.txt");
                    using (SQLiteTransaction transaction = sqlite_conn.BeginTransaction())
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            if (line2 == "")
                            {
                                if (line.IndexOf("');") != -1)
                                {
                                    sqlite_cmd.CommandText = line;
                                    sqlite_cmd.ExecuteNonQuery();
                                    line2 = "";
                                }
                                else
                                {
                                    line2 = line;
                                }
                            }
                            else
                            {
                                line2 += line;
                                if (line2.IndexOf("');") != -1)
                                {
                                    sqlite_cmd.CommandText = line2;
                                    sqlite_cmd.ExecuteNonQuery();
                                    line2 = "";
                                }
                            }
                            
                            
                        }
                        transaction.Commit();
                    }
                }
                sqlite_conn.Close();
            }
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://vk.com/togruth");
        }
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://vk.com/swtor_jk");
        }
        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://vk.com/club195326840");
        }
        private void btn_info_Click(object sender, EventArgs e) //Окно Информация
        {
            if (ActiveForm.Height == 400)
            {
                ActiveForm.Height = 600;

                if(row_translated.Text.ToString() == "")
                {
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
                            row_translated.Invoke((MethodInvoker)(() => row_translated.Text = Math.Round(percentag, 2) + "% (" + count_trans_rows + "/" + count_all_rows + ")"));
                            string sql_insert = "SELECT translator_m,translator_w FROM Translated GROUP by translator_m";
                            sqlite_cmd.CommandText = sql_insert;
                            SQLiteDataReader r = sqlite_cmd.ExecuteReader();
                            while (r.Read())
                            {
                                if (!list_translators.Contains(WebUtility.HtmlDecode(r["translator_m"].ToString()), StringComparer.OrdinalIgnoreCase))
                                    list_translators.Add(WebUtility.HtmlDecode(r["translator_m"].ToString()));
                                if (!list_translators.Contains(WebUtility.HtmlDecode(r["translator_w"].ToString()), StringComparer.OrdinalIgnoreCase))
                                    list_translators.Add(WebUtility.HtmlDecode(r["translator_w"].ToString()));
                            }
                            r.Close();
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
                }
            }
            else 
            {
                ActiveForm.Height = 400;
            }
                
        }
        private void google_opt_CheckedChanged(object sender, EventArgs e) //Переключатель Машинного Переводчика
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
        private void dis_skills_CheckedChanged(object sender, EventArgs e) //Переключатель отключения перевода скилов
        {
            string js;
            if (dis_skills.Checked)
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

        private void recover_Click(object sender, EventArgs e) //Восстановление резервной копии БД
        {
            if (File.Exists("db\\translate_backup.db3"))
            {
                File.Delete("db\\translate.db3");
                File.Copy("db\\translate_backup.db3", "db\\translate.db3");
                File.Delete("db\\translate_backup.db3");
                Config.AppSettings.Settings["row_updated_from_server"].Value = Config.AppSettings.Settings["backup_row"].Value;
                Config.Save(ConfigurationSaveMode.Modified);
               
                LogBox.AppendText("Резервная копия БД восстановлена!\n");
            }
        }
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.swtor.com/r/zlfJtV");
        }
        private void label1_Click(object sender, EventArgs e) //Скачиваем и открываем Инстукцию
        {
            if (File.Exists("Инструкция.pdf"))
                File.Delete("Инструкция.pdf");
            Downloading_Files("https://drive.google.com/uc?export=download&id=1fXQML3tazPL50Q6yy0x2qWauupouYkol", "Инструкция.pdf");
            Process.Start("Инструкция.pdf");
        }
        private async void Update_app_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) //Запуск обновления приложения
        {
            /*recover.Enabled = false;
            ChangePathButton.Enabled = false;
            Install_btn.Enabled = false;
            del_btn.Enabled = false;
            LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Обновляем программу...\n")));
            await Task.Run(() => update_app_method());*/
        }
        private void update_app_method() //Запуск обновления приложения
        {
            /*if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "updater.exe"))
                Downloading_Files("https://drive.google.com/uc?export=download&id=1QiIVmdCQ-12d1cbMuaCSv7YiarFK0TYs", "updater.exe"); //Загружаем обновление            
            Process proc = new Process();
            proc.StartInfo.WorkingDirectory = Application.StartupPath;
            proc.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "updater.exe";
            proc.StartInfo.Arguments = "/u \"" + AppDomain.CurrentDomain.BaseDirectory + "SWToR_RUS.exe" + "\""; // Аргументы командной строки
            proc.Start(); // Запускаем!
            Invoke((MethodInvoker)delegate
            {
                Application.Exit();
            });*/
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e) //Обработчик закрытия приложения
        {
            if (is_run == 0)
                Application.Exit();            
            if (steam_game.Checked == true)
                TryFix();
            Application.Exit();
        }
        private void auto_translate_CheckedChanged(object sender, EventArgs e) //Переключатель авто-переводчика
        {
            string js;
            if (google_opt.Checked == true)
            {
                if (auto_translate.Checked)
                {
                    js = "1";
                    dis_skills.Checked = false;
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
        private void changes_CheckedChanged(object sender, EventArgs e) //Переключатель проверки изменений
        {
            string js;
            if (google_opt.Checked == true && auto_translate.Checked == true)
            {
                if (changes.Checked)
                    js = "1";
                else
                    js = "0";
                Config.AppSettings.Settings["changes"].Value = js;
                Config.Save(ConfigurationSaveMode.Modified);
            }
        }
        private void App_Updater() //Обновление программы
        {
            /*string[] keys = Environment.GetCommandLineArgs(); //Получаем аргументы командной строки
            if (keys.Length > 1)
            {
                int loop = 10; //Количество попыток
                if (keys[1] == "/u") //Если ключ "u" запускаем updater.exe
                {
                    is_run = 0;
                    while (--loop > 0 && File.Exists(CurDir + "SWToR_RUS.exe")) //Удаляем оригинальный файл
                        try
                        {
                            File.Delete(CurDir + "SWToR_RUS.exe");
                        }
                        catch
                        {
                            Thread.Sleep(200); //Небольшая задержка, если файл занят
                        }
                    File.Copy(CurDir + "updater.exe", CurDir + "SWToR_RUS.exe"); // Копируем скачанный файл в оригинальное имя файла
                    Process proc = new Process(); // Запускаем Программу с ключом "d"
                    proc.StartInfo.WorkingDirectory = Application.StartupPath;
                    proc.StartInfo.FileName = CurDir + "SWToR_RUS.exe";
                    proc.StartInfo.Arguments = "/d \"" + CurDir + "SWToR_RUS.exe" + "\""; //Аргументы командной строки
                    proc.Start();
                    Close(); //Закрываем текущее приложение
                }
                else if (keys[1] == "/d") //Если ключ "d" удаляем updater.exe
                {
                    while (--loop > 0 && File.Exists(CurDir + "updater.exe"))
                        try
                        {
                            File.Delete(CurDir + "updater.exe");
                        }
                        catch
                        {
                            Thread.Sleep(200); //Небольшая задержка, если файл занят
                        }

                    LogBox.AppendText("Программа обновлена...\n");
                    is_run = 1;
                }
                else
                    is_run = 1;
            }
            else
                is_run = 1;
            if (is_run == 1) //Проверяем наличие новой версии
            {
                try
                {
                    if (File.Exists(CurDir + "updater.exe"))
                        File.Delete(CurDir + "updater.exe");
                    if (!Directory.Exists("tmp"))
                        Directory.CreateDirectory("tmp");
                    if (File.Exists(CurDir + "tmp\\info.txt"))
                        File.Delete(CurDir + "tmp\\info.txt");
                    Downloading_Files("https://drive.google.com/uc?export=download&id=1VjlEABAWwP1K-0gKgskSkO29gpimKvkC", "tmp\\info.txt"); //Загружаем файл с информацией
                    string version_current = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    string version_new = version_current;
                    version_new = File.ReadLines("tmp\\info.txt").Skip(0).First(); //Считываем первую строку, в ней указана версия
                    if (version_new != version_current) //Показываем ссылку обновлеия программы
                    {
                        Updater.Visible = true;
                        Update_app.Visible = true;
                    }
                }
                catch (Exception e) //Отлавливаем ошибку
                {
                    Logging(e.Message); //Записываем в лог ошибку
                    LogBox.AppendText("Не удалось проверить наличие новой версии. Следите за обновлениями в группе ВК.\n");
                }
            }*/
        }
        private void Downloading_Files(string link, string filename) //Загрузка файлов из интернета
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFile(new Uri(link), filename);
        }
        private void Logging(string Log) //Запись Логов
        {
            using (StreamWriter logging = new StreamWriter("log.txt", true, encoding: Encoding.UTF8))
                logging.WriteLine(Log);
        }
        private void TryFix() //Если файлы называются неправильно, пробуем их чинить
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
        private void Steam_Rename() //Переименование файлов для Steam версии
        {
            if (File.Exists(GamePath + "\\Assets\\swtor_ru-ww_global_1.tor") && File.Exists(GamePath + "\\Assets\\swtor_ru-wm_global_1.tor") && File.Exists(GamePath + "\\Assets\\swtor_maln_global_1.tor")) //Если русификатор был установлен - подменяем файлы
            {
                File.Move(GamePath + "\\Assets\\swtor_en-us_global_1.tor", GamePath + "\\Assets\\swtor_en-us_global_1_tmp.tor");
                File.Move(GamePath + "\\Assets\\swtor_main_global_1.tor", GamePath + "\\Assets\\swtor_main_global_1_tmp.tor");
                File.Move(GamePath + "\\Assets\\swtor_maln_global_1.tor", GamePath + "\\Assets\\swtor_main_global_1.tor");
                if (gender == 1) //Если персонаж мужской
                    File.Move(GamePath + "\\Assets\\swtor_ru-wm_global_1.tor", GamePath + "\\Assets\\swtor_en-us_global_1.tor");
                else
                    File.Move(GamePath + "\\Assets\\swtor_ru-ww_global_1.tor", GamePath + "\\Assets\\swtor_en-us_global_1.tor");
            }
        }

        private async void Form1_Shown(object sender, EventArgs e)
        {
            LogBox.AppendText("Производится запуск русификатора...подождите...\n");
            LogBox.AppendText("Проверка наличия обновлений...\n");            
            await Task.Run(() => loading_info());
        }
        private void loading_info()
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "geckodriver.exe"))
            {
                LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Загружаем новые компоненты...\n")));
                Downloading_Files("https://drive.google.com/uc?export=download&id=1y3f42gldFXrisycqGboHrYzBRwcNQEbY", "geckodriver.exe"); //Загружаем обновление
                if (File.Exists("db\\translate.db3"))
                    File.Delete("db\\translate.db3");
                FileDownloader.DownloadFileFromURLToPath("https://drive.google.com/file/d/1fqQhx8I3fWjmm2SYmZLkfk_Ndmd-5P6M/view?usp=sharing", "db\\translate.db3");
                Downloading_Files("https://drive.google.com/uc?export=download&id=17LYGNgjgARgIxixgytyFoXaOw5C58aBN", "db\\hashes_filename.txt"); //Загружаем обновление
            }
            if (!File.Exists("WebDriver.dll"))
            {
                Downloading_Files("https://drive.google.com/uc?export=download&id=1lgTw0r1I85tg18Y_K3Rs5QMxYuEQR58Q", "WebDriver.dll"); //Загружаем обновление
                Downloading_Files("https://drive.google.com/uc?export=download&id=1Q74tFuTeb1MU_esTBLQ1EfLSRRxNoIOg", "WebDriver.xml"); //Загружаем обновление
            }

            LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Готово!\n")));
            LogBox.Invoke((MethodInvoker)(() => LogBox.AppendText("Русификатор запущен! Приятного использования!\n")));
        }

        private async void ins_font_Click(object sender, EventArgs e)
        {
            Install_btn.Enabled = false; //Отключаем на время установки все элементы
            del_btn.Enabled = false;
            ChangePathButton.Enabled = false;
            ChooseSith.Enabled = false;
            ChooseSit.Enabled = false;
            ChooseMen.Enabled = false;
            ChooseWomen.Enabled = false;
            dis_skills.Enabled = false;
            btn_info.Enabled = false;
            google_opt.Enabled = false;
            auto_translate.Enabled = false;
            changes.Enabled = false;
            recover.Enabled = false;
            ins_font.Enabled = false;
            auth_translate.Enabled = false;
            if (ins_font.Text == "Удалить шрифты" && RusFontsInstalled == 1 && File.Exists(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor_backup") && File.Exists(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor"))
            {
                LogBox.AppendText("Удаление шрифтов...\n");
                File.Delete(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor");
                File.Move(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor_backup", GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor");
                LogBox.AppendText("Удаление шрифтов завершено!\n");
            }
            else
            {
                LogBox.AppendText("Установка шрифтов...\n");
                await CopyFileAsync(GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor", GamePath + "\\Assets\\swtor_main_gfx_assets_1.tor_backup");
                byte[] array = File.ReadAllBytes("db\\fonts.gfx");
                int z = 290505;
                int f = 465311;
                string str = ConfigurationManager.AppSettings["gamepath"];
                FileStream fileStream = new FileStream(str + "\\Assets\\swtor_main_gfx_assets_1.tor", FileMode.Open, FileAccess.ReadWrite);
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
                ins_font.Text = "Удалить шрифты";
                LogBox.AppendText("Установка шрифтов завершена!\n");
                del_btn.Enabled = true;
                ChangePathButton.Enabled = true;
                ChooseSith.Enabled = true;
                ChooseSit.Enabled = true;
                ChooseMen.Enabled = true;
                ChooseWomen.Enabled = true;
                btn_info.Enabled = true;
                recover.Enabled = true;
                dis_skills.Enabled = true;
                google_opt.Enabled = true;
                Install_btn.Enabled = true;
                if (google_opt.Checked == true)
                    auto_translate.Enabled = true;
                if (auto_translate.Checked == true)
                    changes.Enabled = true;
                auth_translate.Enabled = true;
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

        private void vk_link_Click(object sender, EventArgs e)
        {
            Process.Start("https://vk.com/club195326840");
        }

        private void discord_link_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/E6adxtWWfd");
        }

        private void auth_translate_CheckedChanged(object sender, EventArgs e)
        {
            string js;
            if (auth_translate.Checked)
                js = "1";
            else
                js = "0";
            Config.AppSettings.Settings["auth_translate"].Value = js;
            Config.Save(ConfigurationSaveMode.Modified);
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
            NETRESOURCE nr = new NETRESOURCE();
            nr.dwType = RESOURCETYPE_DISK;
            nr.lpRemoteName = uri;

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
