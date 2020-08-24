using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
namespace SWToR_RUS
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.Install_btn = new System.Windows.Forms.Button();
            this.updatedownload = new System.Windows.Forms.Label();
            this.vpo = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.changes = new System.Windows.Forms.CheckBox();
            this.auto_translate = new System.Windows.Forms.CheckBox();
            this.recover = new System.Windows.Forms.Button();
            this.upload_from_server = new System.Windows.Forms.Button();
            this.upload_to_server = new System.Windows.Forms.Button();
            this.editor_btn = new System.Windows.Forms.Button();
            this.google_opt = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.ChooseMen = new System.Windows.Forms.RadioButton();
            this.ChooseWomen = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.dis_skills = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ChooseSit = new System.Windows.Forms.RadioButton();
            this.ChooseSith = new System.Windows.Forms.RadioButton();
            this.ChangePathButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.GamePathTextBox = new System.Windows.Forms.TextBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.row_translated = new System.Windows.Forms.Label();
            this.linkLabel3 = new System.Windows.Forms.LinkLabel();
            this.label5 = new System.Windows.Forms.Label();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.del_btn = new System.Windows.Forms.Button();
            this.ProgressBar1 = new System.Windows.Forms.ProgressBar();
            this.launcher_status = new System.Windows.Forms.Label();
            this.LogBox = new System.Windows.Forms.RichTextBox();
            this.db_convertor = new System.Windows.Forms.Button();
            this.btn_info = new System.Windows.Forms.Button();
            this.info_trans = new System.Windows.Forms.RichTextBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Updater = new System.Windows.Forms.Label();
            this.Update_app = new System.Windows.Forms.LinkLabel();
            this.steam_game = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // Install_btn
            // 
            this.Install_btn.Location = new System.Drawing.Point(21, 205);
            this.Install_btn.Name = "Install_btn";
            this.Install_btn.Size = new System.Drawing.Size(112, 43);
            this.Install_btn.TabIndex = 0;
            this.Install_btn.Text = "Установить";
            this.Install_btn.UseVisualStyleBackColor = true;
            this.Install_btn.Click += new System.EventHandler(this.Install_btn_Click);
            // 
            // updatedownload
            // 
            this.updatedownload.AutoSize = true;
            this.updatedownload.Location = new System.Drawing.Point(-2, 0);
            this.updatedownload.Name = "updatedownload";
            this.updatedownload.Size = new System.Drawing.Size(0, 13);
            this.updatedownload.TabIndex = 1;
            // 
            // vpo
            // 
            this.vpo.AutoSize = true;
            this.vpo.Location = new System.Drawing.Point(289, 81);
            this.vpo.Name = "vpo";
            this.vpo.Size = new System.Drawing.Size(106, 13);
            this.vpo.TabIndex = 3;
            this.vpo.Text = "Версия программы";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(174, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Версия Программы";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox6);
            this.groupBox1.Controls.Add(this.groupBox4);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.ChangePathButton);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.GamePathTextBox);
            this.groupBox1.Location = new System.Drawing.Point(298, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(385, 250);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Настройки";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.changes);
            this.groupBox6.Controls.Add(this.auto_translate);
            this.groupBox6.Controls.Add(this.recover);
            this.groupBox6.Controls.Add(this.upload_from_server);
            this.groupBox6.Controls.Add(this.upload_to_server);
            this.groupBox6.Controls.Add(this.editor_btn);
            this.groupBox6.Controls.Add(this.google_opt);
            this.groupBox6.Location = new System.Drawing.Point(136, 39);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(243, 206);
            this.groupBox6.TabIndex = 8;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Дополнительные опции";
            // 
            // changes
            // 
            this.changes.AutoSize = true;
            this.changes.Checked = true;
            this.changes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.changes.Location = new System.Drawing.Point(6, 46);
            this.changes.Name = "changes";
            this.changes.Size = new System.Drawing.Size(135, 17);
            this.changes.TabIndex = 21;
            this.changes.Text = "Проверка изменений";
            this.changes.UseVisualStyleBackColor = true;
            this.changes.CheckedChanged += new System.EventHandler(this.changes_CheckedChanged);
            // 
            // auto_translate
            // 
            this.auto_translate.AutoSize = true;
            this.auto_translate.Checked = true;
            this.auto_translate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.auto_translate.Enabled = false;
            this.auto_translate.Location = new System.Drawing.Point(6, 30);
            this.auto_translate.Name = "auto_translate";
            this.auto_translate.Size = new System.Drawing.Size(134, 17);
            this.auto_translate.TabIndex = 19;
            this.auto_translate.Text = "Авто-перевод Патчей";
            this.auto_translate.UseVisualStyleBackColor = true;
            this.auto_translate.CheckedChanged += new System.EventHandler(this.auto_translate_CheckedChanged);
            // 
            // recover
            // 
            this.recover.Location = new System.Drawing.Point(17, 133);
            this.recover.Name = "recover";
            this.recover.Size = new System.Drawing.Size(210, 29);
            this.recover.TabIndex = 17;
            this.recover.Text = "Восстановить БД";
            this.recover.UseVisualStyleBackColor = true;
            this.recover.Click += new System.EventHandler(this.recover_Click);
            // 
            // upload_from_server
            // 
            this.upload_from_server.Enabled = false;
            this.upload_from_server.Location = new System.Drawing.Point(17, 97);
            this.upload_from_server.Name = "upload_from_server";
            this.upload_from_server.Size = new System.Drawing.Size(210, 29);
            this.upload_from_server.TabIndex = 18;
            this.upload_from_server.Text = "Загрузить переводы";
            this.upload_from_server.UseVisualStyleBackColor = true;
            this.upload_from_server.Click += new System.EventHandler(this.upload_from_server_Click);
            // 
            // upload_to_server
            // 
            this.upload_to_server.Location = new System.Drawing.Point(17, 64);
            this.upload_to_server.Name = "upload_to_server";
            this.upload_to_server.Size = new System.Drawing.Size(210, 28);
            this.upload_to_server.TabIndex = 17;
            this.upload_to_server.Text = "Выгрузить переводы на сервер";
            this.upload_to_server.UseVisualStyleBackColor = true;
            this.upload_to_server.Click += new System.EventHandler(this.upload_to_server_Click);
            // 
            // editor_btn
            // 
            this.editor_btn.Location = new System.Drawing.Point(17, 168);
            this.editor_btn.Name = "editor_btn";
            this.editor_btn.Size = new System.Drawing.Size(210, 29);
            this.editor_btn.TabIndex = 1;
            this.editor_btn.Text = "Редактор Перевода";
            this.editor_btn.UseVisualStyleBackColor = true;
            this.editor_btn.Click += new System.EventHandler(this.editor_btn_Click_1);
            // 
            // google_opt
            // 
            this.google_opt.AutoSize = true;
            this.google_opt.Checked = true;
            this.google_opt.CheckState = System.Windows.Forms.CheckState.Checked;
            this.google_opt.Location = new System.Drawing.Point(6, 15);
            this.google_opt.Name = "google_opt";
            this.google_opt.Size = new System.Drawing.Size(152, 17);
            this.google_opt.TabIndex = 0;
            this.google_opt.Text = "Добавить перевод Deepl";
            this.google_opt.UseVisualStyleBackColor = true;
            this.google_opt.CheckedChanged += new System.EventHandler(this.google_opt_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.ChooseMen);
            this.groupBox4.Controls.Add(this.ChooseWomen);
            this.groupBox4.Location = new System.Drawing.Point(6, 102);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(124, 64);
            this.groupBox4.TabIndex = 7;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Пол персонажа";
            // 
            // ChooseMen
            // 
            this.ChooseMen.AutoSize = true;
            this.ChooseMen.Checked = true;
            this.ChooseMen.Location = new System.Drawing.Point(6, 19);
            this.ChooseMen.Name = "ChooseMen";
            this.ChooseMen.Size = new System.Drawing.Size(70, 17);
            this.ChooseMen.TabIndex = 8;
            this.ChooseMen.TabStop = true;
            this.ChooseMen.Text = "Мужчина";
            this.ChooseMen.UseVisualStyleBackColor = true;
            this.ChooseMen.CheckedChanged += new System.EventHandler(this.ChooseMen_CheckedChanged);
            // 
            // ChooseWomen
            // 
            this.ChooseWomen.AutoSize = true;
            this.ChooseWomen.Location = new System.Drawing.Point(6, 40);
            this.ChooseWomen.Name = "ChooseWomen";
            this.ChooseWomen.Size = new System.Drawing.Size(75, 17);
            this.ChooseWomen.TabIndex = 9;
            this.ChooseWomen.Text = "Женщина";
            this.ChooseWomen.UseVisualStyleBackColor = true;
            this.ChooseWomen.CheckedChanged += new System.EventHandler(this.ChooseWomen_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.dis_skills);
            this.groupBox3.Location = new System.Drawing.Point(6, 172);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(124, 43);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Отключить перевод";
            // 
            // dis_skills
            // 
            this.dis_skills.AutoSize = true;
            this.dis_skills.Location = new System.Drawing.Point(6, 16);
            this.dis_skills.Name = "dis_skills";
            this.dis_skills.Size = new System.Drawing.Size(66, 17);
            this.dis_skills.TabIndex = 0;
            this.dis_skills.Text = "Умений";
            this.dis_skills.UseVisualStyleBackColor = true;
            this.dis_skills.CheckedChanged += new System.EventHandler(this.dis_skills_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ChooseSit);
            this.groupBox2.Controls.Add(this.ChooseSith);
            this.groupBox2.Location = new System.Drawing.Point(6, 39);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(124, 63);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Варианты перевода";
            // 
            // ChooseSit
            // 
            this.ChooseSit.AutoSize = true;
            this.ChooseSit.Location = new System.Drawing.Point(6, 40);
            this.ChooseSit.Name = "ChooseSit";
            this.ChooseSit.Size = new System.Drawing.Size(43, 17);
            this.ChooseSit.TabIndex = 4;
            this.ChooseSit.Text = "Сит";
            this.ChooseSit.UseVisualStyleBackColor = true;
            this.ChooseSit.CheckedChanged += new System.EventHandler(this.ChooseSit_CheckedChanged);
            // 
            // ChooseSith
            // 
            this.ChooseSith.AutoSize = true;
            this.ChooseSith.Checked = true;
            this.ChooseSith.Location = new System.Drawing.Point(6, 19);
            this.ChooseSith.Name = "ChooseSith";
            this.ChooseSith.Size = new System.Drawing.Size(48, 17);
            this.ChooseSith.TabIndex = 3;
            this.ChooseSith.TabStop = true;
            this.ChooseSith.Text = "Ситх";
            this.ChooseSith.UseVisualStyleBackColor = true;
            this.ChooseSith.CheckedChanged += new System.EventHandler(this.ChooseSith_CheckedChanged);
            // 
            // ChangePathButton
            // 
            this.ChangePathButton.Location = new System.Drawing.Point(348, 13);
            this.ChangePathButton.Name = "ChangePathButton";
            this.ChangePathButton.Size = new System.Drawing.Size(31, 20);
            this.ChangePathButton.TabIndex = 2;
            this.ChangePathButton.Text = "...";
            this.ChangePathButton.UseVisualStyleBackColor = true;
            this.ChangePathButton.Click += new System.EventHandler(this.ChangePathButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Путь к игре";
            // 
            // GamePathTextBox
            // 
            this.GamePathTextBox.Enabled = false;
            this.GamePathTextBox.Location = new System.Drawing.Point(78, 13);
            this.GamePathTextBox.Name = "GamePathTextBox";
            this.GamePathTextBox.Size = new System.Drawing.Size(264, 20);
            this.GamePathTextBox.TabIndex = 0;
            this.GamePathTextBox.TextChanged += new System.EventHandler(this.GamePathTextBox_TextChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label6);
            this.groupBox5.Controls.Add(this.row_translated);
            this.groupBox5.Controls.Add(this.linkLabel3);
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.linkLabel2);
            this.groupBox5.Controls.Add(this.vpo);
            this.groupBox5.Controls.Add(this.linkLabel1);
            this.groupBox5.Controls.Add(this.label4);
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Location = new System.Drawing.Point(373, 411);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(401, 100);
            this.groupBox5.TabIndex = 8;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Информация о программе";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 16);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(162, 13);
            this.label6.TabIndex = 20;
            this.label6.Text = "Процент авторского перевода";
            // 
            // row_translated
            // 
            this.row_translated.AutoSize = true;
            this.row_translated.Location = new System.Drawing.Point(175, 16);
            this.row_translated.Name = "row_translated";
            this.row_translated.Size = new System.Drawing.Size(35, 13);
            this.row_translated.TabIndex = 21;
            this.row_translated.Text = "label7";
            // 
            // linkLabel3
            // 
            this.linkLabel3.AutoSize = true;
            this.linkLabel3.Location = new System.Drawing.Point(141, 60);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(105, 13);
            this.linkLabel3.TabIndex = 13;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "Гороховский Антон";
            this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 60);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(132, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Адаптация и разработка";
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(193, 37);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(26, 13);
            this.linkLabel2.TabIndex = 15;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "JKC";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(143, 37);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(44, 13);
            this.linkLabel1.TabIndex = 13;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Togruth";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 37);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(131, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Основано на переводах:";
            // 
            // del_btn
            // 
            this.del_btn.Location = new System.Drawing.Point(157, 205);
            this.del_btn.Name = "del_btn";
            this.del_btn.Size = new System.Drawing.Size(112, 43);
            this.del_btn.TabIndex = 7;
            this.del_btn.Text = "Удалить";
            this.del_btn.UseVisualStyleBackColor = true;
            this.del_btn.Click += new System.EventHandler(this.del_btn_Click);
            // 
            // ProgressBar1
            // 
            this.ProgressBar1.Location = new System.Drawing.Point(6, 263);
            this.ProgressBar1.Name = "ProgressBar1";
            this.ProgressBar1.Size = new System.Drawing.Size(808, 23);
            this.ProgressBar1.TabIndex = 9;
            // 
            // launcher_status
            // 
            this.launcher_status.AutoSize = true;
            this.launcher_status.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.launcher_status.Location = new System.Drawing.Point(41, 13);
            this.launcher_status.Name = "launcher_status";
            this.launcher_status.Size = new System.Drawing.Size(197, 20);
            this.launcher_status.TabIndex = 10;
            this.launcher_status.Text = "Статус лаунчера SWToR";
            // 
            // LogBox
            // 
            this.LogBox.Location = new System.Drawing.Point(6, 35);
            this.LogBox.Name = "LogBox";
            this.LogBox.ReadOnly = true;
            this.LogBox.Size = new System.Drawing.Size(286, 164);
            this.LogBox.TabIndex = 11;
            this.LogBox.Text = "";
            // 
            // db_convertor
            // 
            this.db_convertor.Location = new System.Drawing.Point(699, 232);
            this.db_convertor.Name = "db_convertor";
            this.db_convertor.Size = new System.Drawing.Size(75, 23);
            this.db_convertor.TabIndex = 12;
            this.db_convertor.Text = "button1";
            this.db_convertor.UseVisualStyleBackColor = true;
            this.db_convertor.Visible = false;
            this.db_convertor.Click += new System.EventHandler(this.db_convertor_Click);
            // 
            // btn_info
            // 
            this.btn_info.Location = new System.Drawing.Point(313, 288);
            this.btn_info.Name = "btn_info";
            this.btn_info.Size = new System.Drawing.Size(200, 28);
            this.btn_info.TabIndex = 13;
            this.btn_info.Text = "Информация";
            this.btn_info.UseVisualStyleBackColor = true;
            this.btn_info.Click += new System.EventHandler(this.btn_info_Click);
            // 
            // info_trans
            // 
            this.info_trans.Location = new System.Drawing.Point(12, 321);
            this.info_trans.Name = "info_trans";
            this.info_trans.ReadOnly = true;
            this.info_trans.Size = new System.Drawing.Size(342, 190);
            this.info_trans.TabIndex = 15;
            this.info_trans.Text = "";
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackgroundImage = global::SWToR_RUS.Properties.Resources.swtor;
            this.pictureBox2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox2.Location = new System.Drawing.Point(678, 68);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(149, 152);
            this.pictureBox2.TabIndex = 16;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(515, 341);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 24);
            this.label1.TabIndex = 17;
            this.label1.Text = "Инструкция";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // Updater
            // 
            this.Updater.AutoSize = true;
            this.Updater.Location = new System.Drawing.Point(580, 294);
            this.Updater.Name = "Updater";
            this.Updater.Size = new System.Drawing.Size(117, 13);
            this.Updater.TabIndex = 18;
            this.Updater.Text = "Вышла новая версия:";
            this.Updater.Visible = false;
            // 
            // Update_app
            // 
            this.Update_app.AutoSize = true;
            this.Update_app.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Update_app.Location = new System.Drawing.Point(693, 294);
            this.Update_app.Name = "Update_app";
            this.Update_app.Size = new System.Drawing.Size(121, 13);
            this.Update_app.TabIndex = 19;
            this.Update_app.TabStop = true;
            this.Update_app.Text = "Обновить приложение";
            this.Update_app.Visible = false;
            this.Update_app.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.Update_app_LinkClicked);
            // 
            // steam_game
            // 
            this.steam_game.AutoSize = true;
            this.steam_game.Enabled = false;
            this.steam_game.Location = new System.Drawing.Point(689, 27);
            this.steam_game.Name = "steam_game";
            this.steam_game.Size = new System.Drawing.Size(56, 17);
            this.steam_game.TabIndex = 20;
            this.steam_game.Text = "Steam";
            this.steam_game.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(824, 316);
            this.Controls.Add(this.steam_game);
            this.Controls.Add(this.Update_app);
            this.Controls.Add(this.Updater);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.info_trans);
            this.Controls.Add(this.btn_info);
            this.Controls.Add(this.db_convertor);
            this.Controls.Add(this.LogBox);
            this.Controls.Add(this.launcher_status);
            this.Controls.Add(this.ProgressBar1);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.del_btn);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.updatedownload);
            this.Controls.Add(this.Install_btn);
            this.Controls.Add(this.pictureBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Star Wars The Old Republic Русификатор";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
       
        

        #endregion

        private System.Windows.Forms.Button Install_btn;
        private System.Windows.Forms.Label updatedownload;
        private System.Windows.Forms.Label vpo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button ChangePathButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox GamePathTextBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton ChooseSit;
        private System.Windows.Forms.RadioButton ChooseSith;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton ChooseMen;
        private System.Windows.Forms.RadioButton ChooseWomen;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button del_btn;
        private System.Windows.Forms.ProgressBar ProgressBar1;
        private System.Windows.Forms.Label launcher_status;
        private System.Windows.Forms.RichTextBox LogBox;
        private System.Windows.Forms.Button db_convertor;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.LinkLabel linkLabel3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btn_info;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.CheckBox google_opt;
        private System.Windows.Forms.RichTextBox info_trans;
        private System.Windows.Forms.Button editor_btn;
        private System.Windows.Forms.CheckBox dis_skills;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button upload_to_server;
        private System.Windows.Forms.Button upload_from_server;
        private System.Windows.Forms.Button recover;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label Updater;
        private System.Windows.Forms.LinkLabel Update_app;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label row_translated;
        private System.Windows.Forms.CheckBox auto_translate;
        private System.Windows.Forms.CheckBox steam_game;
        private System.Windows.Forms.CheckBox changes;
    }
}

