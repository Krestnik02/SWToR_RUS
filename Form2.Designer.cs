namespace SWToR_RUS
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            this.data_trans_file = new System.Windows.Forms.DataGridView();
            this.key = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.text_en = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.text_ru_m = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.text_ru_w = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.translator_m = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.translator_w = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.filesinfo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.filesinfo_w = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.file_to_trans = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.upload_translate = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.translated = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lst_font = new System.Windows.Forms.ComboBox();
            this.fileinfo_user = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.new_author = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.page_lst = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.colrowonpage = new System.Windows.Forms.ComboBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.searchbox = new System.Windows.Forms.TextBox();
            this.search_button = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.search_en = new System.Windows.Forms.RadioButton();
            this.search_ru = new System.Windows.Forms.RadioButton();
            this.search_filename = new System.Windows.Forms.Button();
            this.gotofile = new System.Windows.Forms.Button();
            this.file_name_search = new System.Windows.Forms.Label();
            this.full_text = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.progressBar_text = new System.Windows.Forms.ProgressBar();
            this.loading_text = new System.Windows.Forms.Label();
            this.who_talk = new System.Windows.Forms.Label();
            this.export = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.Wiki = new System.Windows.Forms.LinkLabel();
            this.tor_site = new System.Windows.Forms.LinkLabel();
            this.label9 = new System.Windows.Forms.Label();
            this.html_spec = new System.Windows.Forms.LinkLabel();
            this.My_translate = new System.Windows.Forms.CheckBox();
            this.auth = new System.Windows.Forms.Button();
            this.author_ok = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.data_trans_file)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // data_trans_file
            // 
            this.data_trans_file.AllowUserToAddRows = false;
            this.data_trans_file.AllowUserToDeleteRows = false;
            this.data_trans_file.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.data_trans_file.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.data_trans_file.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.data_trans_file.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.data_trans_file.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.key,
            this.text_en,
            this.text_ru_m,
            this.text_ru_w,
            this.translator_m,
            this.translator_w,
            this.filesinfo,
            this.filesinfo_w});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.data_trans_file.DefaultCellStyle = dataGridViewCellStyle1;
            this.data_trans_file.Location = new System.Drawing.Point(1, 159);
            this.data_trans_file.Name = "data_trans_file";
            this.data_trans_file.Size = new System.Drawing.Size(1450, 402);
            this.data_trans_file.TabIndex = 0;
            this.data_trans_file.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.data_trans_file_CellBeginEdit);
            this.data_trans_file.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.data_trans_file_CellEndEdit);
            this.data_trans_file.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.data_trans_file_CellEnter);
            // 
            // key
            // 
            this.key.HeaderText = "Ключ";
            this.key.Name = "key";
            this.key.ReadOnly = true;
            this.key.Visible = false;
            this.key.Width = 58;
            // 
            // text_en
            // 
            this.text_en.HeaderText = "Английский текст";
            this.text_en.Name = "text_en";
            this.text_en.ReadOnly = true;
            this.text_en.Width = 113;
            // 
            // text_ru_m
            // 
            this.text_ru_m.HeaderText = "Русский текст (М)";
            this.text_ru_m.Name = "text_ru_m";
            this.text_ru_m.Width = 99;
            // 
            // text_ru_w
            // 
            this.text_ru_w.HeaderText = "Русский текст (Ж)";
            this.text_ru_w.Name = "text_ru_w";
            this.text_ru_w.Width = 99;
            // 
            // translator_m
            // 
            this.translator_m.HeaderText = "Переводчик(М)";
            this.translator_m.Name = "translator_m";
            this.translator_m.ReadOnly = true;
            this.translator_m.Width = 108;
            // 
            // translator_w
            // 
            this.translator_w.HeaderText = "Переводчик(Ж)";
            this.translator_w.Name = "translator_w";
            this.translator_w.ReadOnly = true;
            this.translator_w.Width = 110;
            // 
            // filesinfo
            // 
            this.filesinfo.HeaderText = "Файл";
            this.filesinfo.Name = "filesinfo";
            this.filesinfo.ReadOnly = true;
            this.filesinfo.Visible = false;
            this.filesinfo.Width = 61;
            // 
            // filesinfo_w
            // 
            this.filesinfo_w.HeaderText = "Файл_ж";
            this.filesinfo_w.Name = "filesinfo_w";
            this.filesinfo_w.Visible = false;
            this.filesinfo_w.Width = 75;
            // 
            // file_to_trans
            // 
            this.file_to_trans.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.file_to_trans.Enabled = false;
            this.file_to_trans.FormattingEnabled = true;
            this.file_to_trans.Location = new System.Drawing.Point(199, 6);
            this.file_to_trans.Name = "file_to_trans";
            this.file_to_trans.Size = new System.Drawing.Size(328, 21);
            this.file_to_trans.TabIndex = 1;
            this.file_to_trans.SelectedIndexChanged += new System.EventHandler(this.file_to_trans_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(181, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Выбрать область редактирования";
            // 
            // upload_translate
            // 
            this.upload_translate.Enabled = false;
            this.upload_translate.Location = new System.Drawing.Point(397, 70);
            this.upload_translate.Name = "upload_translate";
            this.upload_translate.Size = new System.Drawing.Size(171, 47);
            this.upload_translate.TabIndex = 4;
            this.upload_translate.Text = "Сохранить\\Выгрузить Перевод";
            this.upload_translate.UseVisualStyleBackColor = true;
            this.upload_translate.Visible = false;
            this.upload_translate.Click += new System.EventHandler(this.upload_translate_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(232, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Файл переведён:";
            // 
            // translated
            // 
            this.translated.AutoSize = true;
            this.translated.Location = new System.Drawing.Point(325, 30);
            this.translated.Name = "translated";
            this.translated.Size = new System.Drawing.Size(68, 13);
            this.translated.TabIndex = 6;
            this.translated.Text = "Неизвестно";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(147, 126);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Размер шрифта";
            // 
            // lst_font
            // 
            this.lst_font.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.lst_font.Enabled = false;
            this.lst_font.FormattingEnabled = true;
            this.lst_font.Items.AddRange(new object[] {
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16"});
            this.lst_font.Location = new System.Drawing.Point(238, 123);
            this.lst_font.Name = "lst_font";
            this.lst_font.Size = new System.Drawing.Size(59, 21);
            this.lst_font.TabIndex = 9;
            this.lst_font.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // fileinfo_user
            // 
            this.fileinfo_user.Enabled = false;
            this.fileinfo_user.Location = new System.Drawing.Point(110, 72);
            this.fileinfo_user.Name = "fileinfo_user";
            this.fileinfo_user.Size = new System.Drawing.Size(230, 20);
            this.fileinfo_user.TabIndex = 10;
            this.fileinfo_user.Visible = false;
            this.fileinfo_user.TextChanged += new System.EventHandler(this.fileinfo_user_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 76);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Автор перевода";
            // 
            // new_author
            // 
            this.new_author.Location = new System.Drawing.Point(110, 72);
            this.new_author.MaxLength = 16;
            this.new_author.Name = "new_author";
            this.new_author.Size = new System.Drawing.Size(187, 20);
            this.new_author.TabIndex = 13;
            this.new_author.Text = "Напишите своё имя или оставьте как есть";
            this.new_author.Click += new System.EventHandler(this.new_author_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 102);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Страница";
            // 
            // page_lst
            // 
            this.page_lst.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.page_lst.Enabled = false;
            this.page_lst.FormattingEnabled = true;
            this.page_lst.Location = new System.Drawing.Point(67, 99);
            this.page_lst.Name = "page_lst";
            this.page_lst.Size = new System.Drawing.Size(64, 21);
            this.page_lst.TabIndex = 15;
            this.page_lst.SelectedIndexChanged += new System.EventHandler(this.page_lst_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(134, 102);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(101, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "Отображать строк";
            // 
            // colrowonpage
            // 
            this.colrowonpage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.colrowonpage.Enabled = false;
            this.colrowonpage.FormattingEnabled = true;
            this.colrowonpage.Items.AddRange(new object[] {
            "20",
            "50",
            "100",
            "500",
            "1000"});
            this.colrowonpage.Location = new System.Drawing.Point(238, 99);
            this.colrowonpage.Name = "colrowonpage";
            this.colrowonpage.Size = new System.Drawing.Size(59, 21);
            this.colrowonpage.TabIndex = 17;
            this.colrowonpage.SelectedIndexChanged += new System.EventHandler(this.colrowonpage_SelectedIndexChanged);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(15, 33);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(205, 17);
            this.checkBox1.TabIndex = 18;
            this.checkBox1.Text = "Показывать только Перевод Deepl";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // searchbox
            // 
            this.searchbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.searchbox.Enabled = false;
            this.searchbox.Location = new System.Drawing.Point(6, 14);
            this.searchbox.Name = "searchbox";
            this.searchbox.Size = new System.Drawing.Size(230, 20);
            this.searchbox.TabIndex = 20;
            this.searchbox.TextChanged += new System.EventHandler(this.searchbox_TextChanged);
            // 
            // search_button
            // 
            this.search_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.search_button.Location = new System.Drawing.Point(284, 38);
            this.search_button.Name = "search_button";
            this.search_button.Size = new System.Drawing.Size(77, 59);
            this.search_button.TabIndex = 21;
            this.search_button.Text = "Найти";
            this.search_button.UseVisualStyleBackColor = true;
            this.search_button.Click += new System.EventHandler(this.search_button_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.search_en);
            this.groupBox1.Controls.Add(this.search_ru);
            this.groupBox1.Location = new System.Drawing.Point(6, 41);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(99, 62);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Искать в";
            // 
            // search_en
            // 
            this.search_en.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.search_en.AutoSize = true;
            this.search_en.Location = new System.Drawing.Point(6, 39);
            this.search_en.Name = "search_en";
            this.search_en.Size = new System.Drawing.Size(78, 17);
            this.search_en.TabIndex = 1;
            this.search_en.Text = "оригинале";
            this.search_en.UseVisualStyleBackColor = true;
            // 
            // search_ru
            // 
            this.search_ru.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.search_ru.AutoSize = true;
            this.search_ru.Checked = true;
            this.search_ru.Location = new System.Drawing.Point(6, 19);
            this.search_ru.Name = "search_ru";
            this.search_ru.Size = new System.Drawing.Size(73, 17);
            this.search_ru.TabIndex = 0;
            this.search_ru.TabStop = true;
            this.search_ru.Text = "переводе";
            this.search_ru.UseVisualStyleBackColor = true;
            // 
            // search_filename
            // 
            this.search_filename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.search_filename.Enabled = false;
            this.search_filename.Location = new System.Drawing.Point(111, 80);
            this.search_filename.Name = "search_filename";
            this.search_filename.Size = new System.Drawing.Size(156, 23);
            this.search_filename.TabIndex = 23;
            this.search_filename.Text = "В каком файле эта строка?";
            this.search_filename.UseVisualStyleBackColor = true;
            this.search_filename.Click += new System.EventHandler(this.search_filename_Click);
            // 
            // gotofile
            // 
            this.gotofile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.gotofile.Enabled = false;
            this.gotofile.Location = new System.Drawing.Point(111, 57);
            this.gotofile.Name = "gotofile";
            this.gotofile.Size = new System.Drawing.Size(109, 23);
            this.gotofile.TabIndex = 24;
            this.gotofile.Text = "Перейти к файлу";
            this.gotofile.UseVisualStyleBackColor = true;
            this.gotofile.Click += new System.EventHandler(this.gotofile_Click);
            // 
            // file_name_search
            // 
            this.file_name_search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.file_name_search.AutoSize = true;
            this.file_name_search.Location = new System.Drawing.Point(9, 106);
            this.file_name_search.Name = "file_name_search";
            this.file_name_search.Size = new System.Drawing.Size(36, 13);
            this.file_name_search.TabIndex = 25;
            this.file_name_search.Text = "Файл";
            // 
            // full_text
            // 
            this.full_text.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.full_text.AutoSize = true;
            this.full_text.Location = new System.Drawing.Point(111, 38);
            this.full_text.Name = "full_text";
            this.full_text.Size = new System.Drawing.Size(125, 17);
            this.full_text.TabIndex = 26;
            this.full_text.Text = "Точное совпадение";
            this.full_text.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.file_name_search);
            this.groupBox2.Controls.Add(this.gotofile);
            this.groupBox2.Controls.Add(this.groupBox1);
            this.groupBox2.Controls.Add(this.full_text);
            this.groupBox2.Controls.Add(this.search_filename);
            this.groupBox2.Controls.Add(this.searchbox);
            this.groupBox2.Controls.Add(this.search_button);
            this.groupBox2.Location = new System.Drawing.Point(1078, 9);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(373, 128);
            this.groupBox2.TabIndex = 27;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Поиск";
            // 
            // progressBar_text
            // 
            this.progressBar_text.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar_text.Location = new System.Drawing.Point(1333, 567);
            this.progressBar_text.Name = "progressBar_text";
            this.progressBar_text.Size = new System.Drawing.Size(118, 27);
            this.progressBar_text.TabIndex = 27;
            this.progressBar_text.Visible = false;
            // 
            // loading_text
            // 
            this.loading_text.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.loading_text.AutoSize = true;
            this.loading_text.Location = new System.Drawing.Point(1360, 574);
            this.loading_text.Name = "loading_text";
            this.loading_text.Size = new System.Drawing.Size(67, 13);
            this.loading_text.TabIndex = 28;
            this.loading_text.Text = "Загружаю...";
            this.loading_text.Visible = false;
            // 
            // who_talk
            // 
            this.who_talk.AutoSize = true;
            this.who_talk.Location = new System.Drawing.Point(12, 143);
            this.who_talk.Name = "who_talk";
            this.who_talk.Size = new System.Drawing.Size(35, 13);
            this.who_talk.TabIndex = 29;
            this.who_talk.Text = "label8";
            // 
            // export
            // 
            this.export.Enabled = false;
            this.export.Location = new System.Drawing.Point(533, 5);
            this.export.Name = "export";
            this.export.Size = new System.Drawing.Size(97, 23);
            this.export.TabIndex = 30;
            this.export.Text = "Экспорт в xml";
            this.export.UseVisualStyleBackColor = true;
            this.export.Click += new System.EventHandler(this.export_Click);
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(5, 564);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(723, 13);
            this.label8.TabIndex = 31;
            this.label8.Text = "Символы для перевода:  \"\\n\" - новый абзац ; \"<br>\" - новая строка; кавычки, двойн" +
    "ые кавычки преобразуются редактором автоматически. ";
            // 
            // Wiki
            // 
            this.Wiki.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Wiki.AutoSize = true;
            this.Wiki.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Wiki.Location = new System.Drawing.Point(111, 580);
            this.Wiki.Name = "Wiki";
            this.Wiki.Size = new System.Drawing.Size(61, 13);
            this.Wiki.TabIndex = 32;
            this.Wiki.TabStop = true;
            this.Wiki.Text = "Вукипедия";
            this.Wiki.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.Wiki_LinkClicked);
            // 
            // tor_site
            // 
            this.tor_site.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tor_site.AutoSize = true;
            this.tor_site.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tor_site.Location = new System.Drawing.Point(178, 580);
            this.tor_site.Name = "tor_site";
            this.tor_site.Size = new System.Drawing.Size(72, 13);
            this.tor_site.TabIndex = 33;
            this.tor_site.TabStop = true;
            this.tor_site.Text = "База данных";
            this.tor_site.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.tor_site_LinkClicked);
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(5, 580);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(100, 13);
            this.label9.TabIndex = 34;
            this.label9.Text = "Полезные ссылки";
            // 
            // html_spec
            // 
            this.html_spec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.html_spec.AutoSize = true;
            this.html_spec.Cursor = System.Windows.Forms.Cursors.Hand;
            this.html_spec.Location = new System.Drawing.Point(256, 580);
            this.html_spec.Name = "html_spec";
            this.html_spec.Size = new System.Drawing.Size(100, 13);
            this.html_spec.TabIndex = 35;
            this.html_spec.TabStop = true;
            this.html_spec.Text = "Спецсимволы html";
            this.html_spec.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.html_spec_LinkClicked);
            // 
            // My_translate
            // 
            this.My_translate.AutoSize = true;
            this.My_translate.Location = new System.Drawing.Point(15, 52);
            this.My_translate.Name = "My_translate";
            this.My_translate.Size = new System.Drawing.Size(195, 17);
            this.My_translate.TabIndex = 36;
            this.My_translate.Text = "Показывать только мой перевод";
            this.My_translate.UseVisualStyleBackColor = true;
            // 
            // auth
            // 
            this.auth.Location = new System.Drawing.Point(356, 70);
            this.auth.Name = "auth";
            this.auth.Size = new System.Drawing.Size(171, 47);
            this.auth.TabIndex = 37;
            this.auth.Text = "Авторизация";
            this.auth.UseVisualStyleBackColor = true;
            this.auth.Click += new System.EventHandler(this.auth_Click);
            // 
            // author_ok
            // 
            this.author_ok.Location = new System.Drawing.Point(303, 71);
            this.author_ok.Name = "author_ok";
            this.author_ok.Size = new System.Drawing.Size(47, 21);
            this.author_ok.TabIndex = 38;
            this.author_ok.Text = "ОК";
            this.author_ok.UseVisualStyleBackColor = true;
            this.author_ok.Click += new System.EventHandler(this.author_ok_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 75);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(92, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Название файла";
            this.label4.Visible = false;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1452, 595);
            this.Controls.Add(this.author_ok);
            this.Controls.Add(this.auth);
            this.Controls.Add(this.My_translate);
            this.Controls.Add(this.html_spec);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.tor_site);
            this.Controls.Add(this.Wiki);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.export);
            this.Controls.Add(this.who_talk);
            this.Controls.Add(this.loading_text);
            this.Controls.Add(this.progressBar_text);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.colrowonpage);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.page_lst);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.new_author);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.fileinfo_user);
            this.Controls.Add(this.lst_font);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.translated);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.upload_translate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.file_to_trans);
            this.Controls.Add(this.data_trans_file);
            this.Controls.Add(this.groupBox2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(1100, 600);
            this.Name = "Form2";
            this.Text = "Редактор - Не Авторизирован";
            this.Activated += new System.EventHandler(this.Form2_Activated);
            ((System.ComponentModel.ISupportInitialize)(this.data_trans_file)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView data_trans_file;
        private System.Windows.Forms.ComboBox file_to_trans;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button upload_translate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label translated;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox lst_font;
        private System.Windows.Forms.TextBox fileinfo_user;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox new_author;
        private System.Windows.Forms.DataGridViewTextBoxColumn key;
        private System.Windows.Forms.DataGridViewTextBoxColumn text_en;
        private System.Windows.Forms.DataGridViewTextBoxColumn text_ru_m;
        private System.Windows.Forms.DataGridViewTextBoxColumn text_ru_w;
        private System.Windows.Forms.DataGridViewTextBoxColumn translator_m;
        private System.Windows.Forms.DataGridViewTextBoxColumn translator_w;
        private System.Windows.Forms.DataGridViewTextBoxColumn filesinfo;
        private System.Windows.Forms.DataGridViewTextBoxColumn filesinfo_w;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox page_lst;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox colrowonpage;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TextBox searchbox;
        private System.Windows.Forms.Button search_button;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton search_en;
        private System.Windows.Forms.RadioButton search_ru;
        private System.Windows.Forms.Button search_filename;
        private System.Windows.Forms.Button gotofile;
        private System.Windows.Forms.Label file_name_search;
        private System.Windows.Forms.CheckBox full_text;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ProgressBar progressBar_text;
        private System.Windows.Forms.Label loading_text;
        private System.Windows.Forms.Label who_talk;
        private System.Windows.Forms.Button export;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.LinkLabel Wiki;
        private System.Windows.Forms.LinkLabel tor_site;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.LinkLabel html_spec;
        private System.Windows.Forms.CheckBox My_translate;
        private System.Windows.Forms.Button auth;
        private System.Windows.Forms.Button author_ok;
        private System.Windows.Forms.Label label4;
    }
}