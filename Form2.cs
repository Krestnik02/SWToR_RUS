﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Net;
using System.IO;
using System.Data.SQLite;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Net.Mime;

namespace SWToR_RUS
{
    public partial class Form2 : Form
    {
        public bool authorization = false;

        public ulong hash_g;

        public ulong vOut;

        public string selected_file_to_transl;

        public string page_transl;
        
        public int count_per_pages = 20;

        public string lst_font_change = "8";

        public string go_to_file = "0";

        public string searching_row_id = "";

        public string buffer_m = "";

        public string buffer_w = "";

        public string email = "";

        public List<string> data_name = new List<string>();

        public List<string> data_email = new List<string>();

        public List<string> source_list_m = new List<string>();

        public List<string> source_list_w = new List<string>();

        public List<string> source_list_m_author = new List<string>();

        public List<string> source_list_w_author = new List<string>();

        public string connStr_mysql = "server=" + "195.234.5.250" + //Адрес сервера (для локальной базы пишите "localhost")
                    ";user=" + "swtor" + //Имя пользователя
                    ";database=" + "swtor_ru" + //Имя базы данных
                    ";port=" + "3306" + //Порт для подключения
                    ";password=" + "KHUS86!JHksds" + //Пароль для подключения
                    ";default command timeout=0;";
        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            byte[] buffer4;
            if (hashedPassword == null)
            {
                return false;
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            byte[] src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 0x31) || (src[0] != 0))
            {
                return false;
            }
            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            byte[] buffer3 = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8))
            {
                buffer4 = bytes.GetBytes(0x20);
            }
            return ByteArraysEqual(buffer3, buffer4);
        }

        public static bool ByteArraysEqual(byte[] b1, byte[] b2)
        {
            if (b1 == b2) return true;
            if (b1 == null || b2 == null) return false;
            if (b1.Length != b2.Length) return false;
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i]) return false;
            }
            return true;
        }

        public Form2()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            file_name_search.Text = "";
            who_talk.Text = "";

            using (MySqlConnection conn = new MySqlConnection(connStr_mysql))
            {
                conn.Open();
                string sql = "SELECT DISTINCT name, email FROM users";
                MySqlCommand command = new MySqlCommand(sql, conn);
                MySqlDataReader row = command.ExecuteReader();

                if (row.HasRows)
                {
                    while (row.Read())
                    {
                        data_name.Add(row["name"].ToString());
                        data_email.Add(row["email"].ToString());
                    }
                    row.Close();
                }
                conn.Close();
            }

            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (ConfigurationManager.AppSettings["author"] != null && configuration.AppSettings.Settings["author"].Value != "")
            {
                new_author.Text = configuration.AppSettings.Settings["author"].Value;
                new_author.Enabled = false;
                author_ok.Enabled = false;
                file_to_trans.Enabled = true;
                searchbox.Enabled = true;

                if (ConfigurationManager.AppSettings["email"] != null && configuration.AppSettings.Settings["email"].Value != "")
                {
                    using (MySqlConnection conn = new MySqlConnection(connStr_mysql))
                    {
                        conn.Open();
                        string sql = "SELECT email, name, pass FROM users WHERE email='" + configuration.AppSettings.Settings["email"].Value + "' AND name ='" + configuration.AppSettings.Settings["author"].Value + "'";
                        MySqlCommand command = new MySqlCommand(sql, conn);
                        MySqlDataReader row = command.ExecuteReader();
                        if (row.HasRows)
                        {
                            while (row.Read())
                            {
                                if (row["pass"].ToString() == configuration.AppSettings.Settings["password"].Value)
                                {
                                    auth.Enabled = false;
                                    authorization = true;
                                    this.Text = "Редактор - Авторизирован: " + row["name"].ToString();

                                    //выводим список в комбобоксе
                                    SQLiteConnection sqlite_conn;
                                    sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; ");
                                    sqlite_conn.Open();
                                    SQLiteCommand sqlite_cmd;
                                    sqlite_cmd = sqlite_conn.CreateCommand();
                                    string sql_insert = "SELECT fileinfo FROM Translated GROUP by fileinfo";
                                    sqlite_cmd.CommandText = sql_insert;
                                    SQLiteDataReader r = sqlite_cmd.ExecuteReader();
                                    while (r.Read())
                                        file_to_trans.Items.Add(r["fileinfo"].ToString());
                                    r.Close();
                                    sqlite_conn.Close();
                                } else
                                {
                                    MessageBox.Show("Проверьте конфигурационный файл, очистите значения авторизационных данных и авторизируйтесь заново", "Ошибка авторизации", MessageBoxButtons.OK);
                                }
                            }
                            row.Close();
                        }
                        conn.Close();
                    }
                } else
                {
                    auth.Enabled = true;
                    //выводим список в комбобоксе
                    SQLiteConnection sqlite_conn;
                    sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; ");
                    sqlite_conn.Open();
                    SQLiteCommand sqlite_cmd;
                    sqlite_cmd = sqlite_conn.CreateCommand();
                    string sql_insert = "SELECT fileinfo FROM Translated WHERE ((translator_m='Deepl' OR translator_w='Deepl') OR (translator_m='" + new_author.Text + "' OR translator_w='" + new_author.Text + "')) GROUP by fileinfo";
                    sqlite_cmd.CommandText = sql_insert;
                    SQLiteDataReader r = sqlite_cmd.ExecuteReader();
                    while (r.Read())
                        file_to_trans.Items.Add(r["fileinfo"].ToString());
                    r.Close();
                    sqlite_conn.Close();
                }
            } else
            {
                MessageBox.Show("Для продолжения работы введите свой никнейм в поле \"Автор перевода\"", "Внимание", MessageBoxButtons.OK);
            }
        }

        private async void file_to_trans_SelectedIndexChanged(object sender, EventArgs e)
        {            
            if (file_to_trans.SelectedIndex != -1)
            {
                if (selected_file_to_transl != file_to_trans.SelectedItem.ToString())
                {
                    selected_file_to_transl = file_to_trans.SelectedItem.ToString();
                    if (file_to_trans.Text != "")
                    {
                        searchbox.Text = "";
                        string sql_insert = "";
                        string sql_insert_part2 = "";
                        source_list_m.Clear();
                        source_list_w.Clear();
                        source_list_m_author.Clear();
                        source_list_w_author.Clear();
                        data_trans_file.Rows.Clear();
                        /*List<string> list_translators = new List<string>();*/
                        SQLiteConnection sqlite_conn;
                        sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; ");
                        sqlite_conn.Open();
                        int count_pgs = 0;
                        if (checkBox1.Checked == true || authorization == false)
                            sql_insert_part2 += " AND ((translator_m='Deepl' OR translator_w='Deepl') OR (translator_m='" + new_author.Text + "' OR translator_w='" + new_author.Text + "'))";
                        sql_insert = "SELECT COUNT(DISTINCT text_en) FROM Translated WHERE fileinfo='" + file_to_trans.Text + "'" + sql_insert_part2;
                        SQLiteCommand oCmd = new SQLiteCommand(sql_insert, sqlite_conn);
                        count_pgs = Convert.ToInt32(oCmd.ExecuteScalar());
                        oCmd.Dispose();
                        sqlite_conn.Close();
                        if (colrowonpage.SelectedIndex != -1)
                            count_per_pages = int.Parse(colrowonpage.Text);
                        else
                            colrowonpage.SelectedIndex = colrowonpage.FindStringExact(count_per_pages.ToString());
                        page_lst.Items.Clear();
                        page_transl = "1";
                        for (int i = 1; i <= (count_pgs / count_per_pages) + 1; i++)
                        {
                            page_lst.Items.Add(i);
                        }
                        page_lst.SelectedIndex = page_lst.FindStringExact("1");
                        if (count_pgs / count_per_pages > 1)
                            page_lst.Enabled = true;
                        else
                            page_lst.Enabled = false;
                        await Task.Run(() => data_load());
                        colrowonpage.Enabled = true;
                        lst_font.Enabled = true;
                        fileinfo_user.Enabled = true;
                        gotofile.Enabled = false;
                        search_filename.Enabled = false;
                    }
                }
                if(authorization == true)
                    export.Enabled = true;
            }
            else
                export.Enabled = false;
            if (fileinfo_user.Text != "")
                fileinfo_user.Enabled = true;

        }

        public void data_upload_trans(DataGridViewCellEventArgs e, int status, string xml_text = "", string searchValue = "", string file = "")
        {
            List<string> list_keys = new List<string>();
            string sql_insert = "";
            if (File.Exists(file))
            {
                var lines = File.ReadAllLines(file);
                File.WriteAllLines(file, lines.Take(lines.Length - 1).ToArray(), encoding: Encoding.UTF8);
            }
            else
            {
                using (StreamWriter file_for_exam = new StreamWriter(file, true, encoding: Encoding.UTF8))
                {
                    if(status == 0)
                    {
                        xml_text = "<rezult_author>";
                    } else
                    {
                        xml_text = "<rezult>";
                    }
                    file_for_exam.WriteLine(xml_text);
                }
            }

            //теневая загрузка строки в отдельный файл
            using (SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; "))
            {
                sqlite_conn.Open();
                using (SQLiteCommand sqlite_cmd = new SQLiteCommand(sqlite_conn))
                {
                    loading_text.Invoke((MethodInvoker)(() => loading_text.Parent = progressBar_text));
                    loading_text.Invoke((MethodInvoker)(() => loading_text.BackColor = Color.Transparent));
                    loading_text.Invoke((MethodInvoker)(() => loading_text.Visible = true));
                    progressBar_text.Invoke((MethodInvoker)(() => progressBar_text.Visible = true));
                    progressBar_text.Invoke((MethodInvoker)(() => progressBar_text.Value = 0));
                    progressBar_text.Invoke((MethodInvoker)(() => progressBar_text.Maximum = count_per_pages));
                    foreach (DataGridViewRow row in data_trans_file.Rows)
                    {
                        if (row.Cells["filesinfo"].Value.ToString().Equals(searchValue) || row.Cells["filesinfo_w"].Value.ToString().Equals(searchValue))
                        {
                            string sql_select = "SELECT key_unic,translator_m FROM Translated WHERE text_en='" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "'";
                            sqlite_cmd.CommandText = sql_select;
                            SQLiteDataReader s = sqlite_cmd.ExecuteReader();
                            while (s.Read())
                            {
                                /*if (row.Cells["key"].Value.ToString() == s["key_unic"].ToString() || s["translator_m"].ToString() == "Deepl")
                                {
                                    if (!list_keys.Contains(s["key_unic"].ToString()))
                                        list_keys.Add(s["key_unic"].ToString());
                                }*/
                                if (!list_keys.Contains(s["key_unic"].ToString()))
                                    list_keys.Add(s["key_unic"].ToString());
                            }
                            s.Close();

                            list_keys.ForEach(delegate (string name)
                            {
                                //если пытается изменить авторский перевод
                                if (status == 0)
                                {
                                    //создание папки export, если ее нет
                                    if (!Directory.Exists("export"))
                                    {
                                        Directory.CreateDirectory("export");
                                    }

                                    if (row.Cells["filesinfo"].Value.ToString().Equals(searchValue))
                                    {
                                        xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(new_author.Text) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(row.Cells["translator_w"].Value.ToString()) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "</text_ru_w><soruce_m>" + WebUtility.HtmlEncode(source_list_m[e.RowIndex]) + "</soruce_m><soruce_w>" + WebUtility.HtmlEncode(source_list_w[e.RowIndex]) + "</soruce_w>";

                                        row.Cells["filesinfo"].Value = "0"; //сбрасываем состояние строки
                                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "0"; //сбрасываем состояние строки
                                    }
                                    else if (row.Cells["filesinfo_w"].Value.ToString().Equals(searchValue))
                                    {
                                        xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(row.Cells["translator_m"].Value.ToString()) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(new_author.Text) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "</text_ru_w><soruce_m>" + WebUtility.HtmlEncode(source_list_m[e.RowIndex]) + "</soruce_m><soruce_w>" + WebUtility.HtmlEncode(source_list_w[e.RowIndex]) + "</soruce_w>";

                                        row.Cells["filesinfo_w"].Value = "0"; //сбрасываем состояние строки
                                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "0"; //сбрасываем состояние строки
                                    }
                                }
                                else if (status == 1)
                                {
                                    if (Convert.ToString(row.Cells["text_ru_w"].Value) == "" && authorization == true)
                                    {
                                        if (row.Cells["filesinfo"].Value.ToString().Equals(searchValue))
                                        {
                                            sql_insert = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "',translator_m='" + WebUtility.HtmlEncode(new_author.Text) + "' WHERE key_unic ='" + name + "'";
                                            xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(new_author.Text) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"\"></text_ru_w>";

                                            row.Cells["filesinfo"].Value = "0"; //сбрасываем состояние строки
                                            data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "0"; //сбрасываем состояние строки
                                        }
                                        else if (row.Cells["filesinfo_w"].Value.ToString().Equals(searchValue))
                                        {
                                            sql_insert = "UPDATE Translated SET text_ru_w=NULL,translator_w=NULL WHERE key_unic ='" + name + "'";
                                            xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(row.Cells["translator_m"].Value.ToString()) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"\"></text_ru_w>";

                                            row.Cells["filesinfo_w"].Value = "0"; //сбрасываем состояние строки
                                            data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "0"; //сбрасываем состояние строки
                                        }
                                    }
                                    else
                                    {
                                        if (row.Cells["filesinfo"].Value.ToString().Equals(searchValue))
                                        {
                                            sql_insert = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "',translator_m='" + WebUtility.HtmlEncode(new_author.Text) + "' WHERE key_unic ='" + name + "'";
                                            xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(new_author.Text) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(row.Cells["translator_w"].Value.ToString()) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "</text_ru_w>";

                                            row.Cells["filesinfo"].Value = "0"; //сбрасываем состояние строки
                                            data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "0"; //сбрасываем состояние строки
                                        }
                                        else if (row.Cells["filesinfo_w"].Value.ToString().Equals(searchValue))
                                        {
                                            sql_insert = "UPDATE Translated SET text_ru_w='" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "',translator_w='" + WebUtility.HtmlEncode(new_author.Text) + "' WHERE key_unic ='" + name + "'";
                                            xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(row.Cells["translator_m"].Value.ToString()) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(new_author.Text) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "</text_ru_w>";

                                            row.Cells["filesinfo_w"].Value = "0"; //сбрасываем состояние строки
                                            data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "0"; //сбрасываем состояние строки
                                        }
                                    }
                                }

                                //редактируем строки, есть соседняя строка авторская
                                else if (status == 2)
                                {
                                    if (row.Cells["filesinfo"].Value.ToString().Equals(searchValue) && row.Cells["filesinfo_w"].Value.ToString() == "2")
                                    {
                                        sql_insert = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "', translator_m='" + WebUtility.HtmlEncode(new_author.Text) + "' WHERE key_unic ='" + name + "'";
                                        xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(new_author.Text) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(source_list_w_author[e.RowIndex]) + "\">" + WebUtility.HtmlEncode(source_list_w[e.RowIndex]) + "</text_ru_w>";

                                        row.Cells["filesinfo"].Value = "0"; //сбрасываем состояние строки
                                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "0"; //сбрасываем состояние строки
                                        row.Cells["filesinfo_w"].Value = "0"; //сбрасываем состояние строки
                                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "0"; //сбрасываем состояние строки
                                    }
                                    else if (row.Cells["filesinfo_w"].Value.ToString().Equals(searchValue) && row.Cells["filesinfo"].Value.ToString() == "2")
                                    {
                                        sql_insert = "UPDATE Translated SET text_ru_w='" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "', translator_w='" + WebUtility.HtmlEncode(new_author.Text) + "' WHERE key_unic ='" + name + "'";
                                        xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(source_list_m_author[e.RowIndex]) + "\">" + WebUtility.HtmlEncode(source_list_m[e.RowIndex]) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(new_author.Text) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "</text_ru_w>";

                                        row.Cells["filesinfo"].Value = "0"; //сбрасываем состояние строки
                                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "0"; //сбрасываем состояние строки
                                        row.Cells["filesinfo_w"].Value = "0"; //сбрасываем состояние строки
                                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "0"; //сбрасываем состояние строки
                                    }
                                }

                                using (StreamWriter file_for_exam = new StreamWriter(file, true, encoding: Encoding.UTF8))
                                {
                                    file_for_exam.WriteLine(xml_text);
                                }
                                sqlite_cmd.CommandText = sql_insert;
                                sqlite_cmd.ExecuteNonQuery();
                                xml_text = "";
                            });

                        }
                        progressBar_text.Invoke((MethodInvoker)(() => progressBar_text.Value += 1));
                        list_keys.Clear();
                    }
                }
                sqlite_conn.Close();
            }
            progressBar_text.Invoke((MethodInvoker)(() => progressBar_text.Visible = false));
            loading_text.Invoke((MethodInvoker)(() => loading_text.Visible = false));

            using (StreamWriter file_for_exam = new StreamWriter(file, true, encoding: Encoding.UTF8))
            {
                if (status == 0)
                {
                    file_for_exam.WriteLine("</rezult_author>");
                }
                else
                {
                    file_for_exam.WriteLine("</rezult>");
                }
            }
        }

        public void data_trans_file_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int alert_m = 0;
            int alert_w = 0;
            int alert_m_near = 0;
            int alert_w_near = 0;

            //проверяем на изменения с исходной строкой, если нет значит строка сделана им в этой сессии! Что бы не выводить ошибку, если он ввел текст, а потом удалил свой же
            if (Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[2].Value) == ""
                && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[2].Value) != buffer_m
                && source_list_m[e.RowIndex] != Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[2].Value)
                && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[4].Value) != new_author.Text)
            {
                MessageBox.Show("Только проверяющий может удать строки других авторов", "Внимание", MessageBoxButtons.OK); //если пытается удалить строку в м переводе, проверяем авторизован ли выводим мсбокс
                data_trans_file.Rows[e.RowIndex].Cells[2].Value = source_list_m[e.RowIndex]; //возвращаем во вьюгрид исходную строку
            }

            //проверяем на изменения с исходной строкой, если нет значит строка сделана им в этой сессии! Что бы не выводить ошибку, если он ввел текст, а потом удалил свой же
            else if (Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[3].Value) == ""
                && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[3].Value) != buffer_w
                && source_list_w[e.RowIndex] != Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[3].Value)
                && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[5].Value) != new_author.Text)
            {
                MessageBox.Show("Только проверяющий может удать строки других авторов", "Внимание", MessageBoxButtons.OK); //если пытается удалить строку в ж переводе, проверяем авторизован ли выводим мсбокс
                data_trans_file.Rows[e.RowIndex].Cells[3].Value = source_list_w[e.RowIndex]; //возвращаем во вьюгрид исходную строку
            }

            else if (source_list_m[e.RowIndex] == "" && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[2].Value) != "") //если строка изначально была пустая
            {
                data_trans_file.CurrentCell.Style.Font = new Font("Microsoft Sans Serif", Convert.ToInt32(lst_font.Text), FontStyle.Bold); //помечаем строки которые изменяли
                data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "1"; //помечаем строку, как любую другую
                data_upload_trans(e, 1, "", "1", "user_translation\\user_translation.xml");

            }
            else if (source_list_w[e.RowIndex] == "" && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[3].Value) != "") //если строка изначально была пустая
            {
                data_trans_file.CurrentCell.Style.Font = new Font("Microsoft Sans Serif", Convert.ToInt32(lst_font.Text), FontStyle.Bold); //помечаем строки которые изменяли
                data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "1"; //помечаем строку, как любую другую
                data_upload_trans(e, 1, "", "1", "user_translation\\user_translation.xml");
            }

            //проверяем есть ли изменения строк в м варианте
            else if (buffer_m != Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[2].Value) && buffer_m != "" && source_list_m[e.RowIndex] != "")
            {
                data_trans_file.CurrentCell.Style.Font = new Font("Microsoft Sans Serif", Convert.ToInt32(lst_font.Text), FontStyle.Bold); //помечаем строки которые изменяли

                for (int i = 0; i < data_name.Count; i++)
                {
                    if (Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[4].Value) == data_name[i].ToString()
                        && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[4].Value) != new_author.Text.ToString())
                    {
                        alert_m = 1;
                        email = data_email[i];
                    }
                }

                for (int i = 0; i < data_name.Count; i++)
                {
                    if (Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[5].Value) == data_name[i].ToString()
                        && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[5].Value) != new_author.Text.ToString())
                    {
                        alert_m_near = 1;
                    }
                }

                if (alert_m_near == 1 && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[4].Value) != "") //проверяем если автор соседней строки авторизован
                {
                    if (e.ColumnIndex == 2)
                    {
                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "1"; //помечаем строку, как любую другую
                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "2"; //помечаем соседнюю строку, как авторскую
                    }

                    data_upload_trans(e, 2, "", "1", "user_translation\\user_translation.xml");
                }

                else if (alert_m == 1 && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[4].Value) != "") //проверяем если автор этой м строки авторизован
                {
                    if (e.ColumnIndex == 2)
                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "2"; //помечаем строку, как строку которая внесена авторизированным автором

                    /* Заносим эти строки в отдельный файл */
                    DialogResult dialogResult = MessageBox.Show("Вы желаете изменить строку перевода авторизированного автора? Выберите \"Да\", если желате отправить автоматическое сообщение по завершению работы программы для уведомления его об этом", "Внимание", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        data_upload_trans(e, 0, "", "2", "export\\error_" + email + ".xml");
                    }
                    else
                    {
                        if (e.ColumnIndex == 2)
                        {
                            data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "0"; //сбрасываем состояние строки
                            data_trans_file.Rows[e.RowIndex].Cells[2].Value = source_list_m[e.RowIndex]; //возвращаем во вьюгрид исходную строку
                        }
                    }

                    alert_m = 0;
                }
                //проверяем наша ли это строка или Deepl
                else if (Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[4].Value) == new_author.Text.ToString()
                    || Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[4].Value) == "Deepl"
                    || Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[4].Value) == "")
                {
                    if (e.ColumnIndex == 2)
                    {
                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "1"; //помечаем строку, как любую другую
                    }

                    data_upload_trans(e, 1, "", "1", "user_translation\\user_translation.xml");
                }
                else 
                {
                    if (e.ColumnIndex == 2)
                    {
                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "1"; //помечаем строку, как любую другую
                    }

                    /* Заносим эти строки в отдельный файл */
                    data_upload_trans(e, 1, "", "1", "user_translation\\user_translation.xml");
                }
            }

            //проверяем есть ли изменения строк в ж варианте
            else if (buffer_w != Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[3].Value) && buffer_w != "" && source_list_w[e.RowIndex] != "")
            {
                data_trans_file.CurrentCell.Style.Font = new Font("Microsoft Sans Serif", Convert.ToInt32(lst_font.Text), FontStyle.Bold); //помечаем строки которые изменяли

                for (int i = 0; i < data_name.Count; i++)
                {
                    if (Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[5].Value) == data_name[i].ToString()
                        && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[5].Value) != new_author.Text.ToString())
                    {
                        alert_w = 1;
                        email = data_email[i];
                    }
                }

                for (int i = 0; i < data_name.Count; i++)
                {
                    if (Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[4].Value) == data_name[i].ToString()
                        && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[4].Value) != new_author.Text.ToString())
                    {
                        alert_w_near = 1;
                    }
                }

                if (alert_w_near == 1 && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[4].Value) != "") //проверяем если автор соседней строки авторизован
                {
                    if (e.ColumnIndex == 3)
                    {
                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "1"; //помечаем строку, как любую другую
                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "2"; //помечаем соседнюю строку, как авторскую
                    }
                    MessageBox.Show("test");
                    data_upload_trans(e, 2, "", "1", "user_translation\\user_translation.xml");
                }

                else if (alert_w == 1 && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[5].Value) != "") //проверяем если автор этой ж строки авторизован
                {
                    if (e.ColumnIndex == 3)
                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "2"; //помечаем строку, как строку которая внесена авторизированным автором

                    /* Заносим эти строки в отдельный файл */
                    DialogResult dialogResult = MessageBox.Show("Вы желаете изменить строку перевода авторизированного автора? Выберите \"Да\", если желате отправить автоматическое сообщение по завершению работы программы для уведомления его об этом", "Внимание", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        data_upload_trans(e, 0, "", "2", "export\\error_" + email + ".xml");
                    }
                    else
                    {
                        if (e.ColumnIndex == 3)
                        {
                            data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "0"; //сбрасываем состояние строки
                            data_trans_file.Rows[e.RowIndex].Cells[3].Value = source_list_w[e.RowIndex]; //возвращаем во вьюгрид исходную строку
                        }
                    }

                    alert_w = 0;
                }
                //проверяем наша ли это строка или Deepl
                else if (Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[5].Value) == new_author.Text.ToString()
                    || Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[5].Value) == "Deepl"
                    || Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[5].Value) == "")
                {
                    if (e.ColumnIndex == 3)
                    {
                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "1"; //помечаем строку, как любую другую
                    }

                    data_upload_trans(e, 1, "", "1", "user_translation\\user_translation.xml");
                }
                else 
                {
                    if (e.ColumnIndex == 3)
                    {
                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "1"; //помечаем строку, как любую другую
                    }

                    /* Заносим эти строки в отдельный файл */
                    data_upload_trans(e, 1, "", "1", "user_translation\\user_translation.xml");
                }
            }
        }

        private void data_trans_file_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            buffer_m = "";
            buffer_w = "";
            if (e.ColumnIndex == 2)
            {
                buffer_m = Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[2].Value);
            }
            if (e.ColumnIndex == 3)
            {
                buffer_w = Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[3].Value);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lst_font_change != lst_font.SelectedItem.ToString())
            {
                lst_font_change = lst_font.Text;
                int font = Convert.ToInt32(lst_font.Text);
                data_trans_file.DefaultCellStyle.Font = new Font("Microsoft Sans Serif", font);
            }            
        }

        private void fileinfo_user_TextChanged(object sender, EventArgs e)
        {
/*            if (fileinfo_user.Text != "")
                upload_translate.Enabled = true;
            else
                upload_translate.Enabled = false;
*/        }

        private /*async*/ void upload_translate_Click(object sender, EventArgs e)
        {
            /*await Task.Run(() => uploading());*/
        }
        public void uploading()
        {
/*            string new_authr = new_author.Text;
            string xml_text = "";
            List<string> list_keys = new List<string>();
            
            if (File.Exists("user_translation\\" + fileinfo_user.Text + ".xml"))
            {
                DialogResult dialogResult = MessageBox.Show("Файл с таким именем уже существует. Вы уверены что хотите продолжать записывать в него?", "Подтверждение", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    var lines = File.ReadAllLines("user_translation\\" + fileinfo_user.Text + ".xml");
                    File.WriteAllLines("user_translation\\" + fileinfo_user.Text + ".xml", lines.Take(lines.Length - 1).ToArray(), encoding: Encoding.UTF8);
                }
                else
                {
                    fileinfo_user.Text = fileinfo_user.Text + "1";
                    using (StreamWriter file_for_exam =
                     new StreamWriter("user_translation\\" + fileinfo_user.Text + ".xml", true, encoding: Encoding.UTF8))
                    {
                        xml_text = "<rezult>";
                        file_for_exam.WriteLine(xml_text);
                    }
                }
            }
            else
            {
                using (StreamWriter file_for_exam =
                     new StreamWriter("user_translation\\" + fileinfo_user.Text + ".xml", true, encoding: Encoding.UTF8))
                {
                    xml_text = "<rezult>";
                    file_for_exam.WriteLine(xml_text);
                }
            }
            string searchValue = "1";
            string sql_insert = "";
            int num_edited_rows = 0;
            string change_other_strings = "no";
            DialogResult dialogResult1 = MessageBox.Show("Заменять уже отредактированные строки?", "Подтверждение", MessageBoxButtons.YesNo);
            if (dialogResult1 == DialogResult.Yes)
            {
                change_other_strings = "yes";
            }
            using (SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; "))
            {
                sqlite_conn.Open();
                using (SQLiteCommand sqlite_cmd = new SQLiteCommand(sqlite_conn))
                {
                    loading_text.Invoke((MethodInvoker)(() => loading_text.Parent = progressBar_text));
                    loading_text.Invoke((MethodInvoker)(() => loading_text.BackColor = Color.Transparent));
                    loading_text.Invoke((MethodInvoker)(() => loading_text.Visible = true));
                    progressBar_text.Invoke((MethodInvoker)(() => progressBar_text.Visible = true));
                    progressBar_text.Invoke((MethodInvoker)(() => progressBar_text.Value = 0));
                    progressBar_text.Invoke((MethodInvoker)(() => progressBar_text.Maximum = count_per_pages));
                    foreach (DataGridViewRow row in data_trans_file.Rows)
                    {
                        if (row.Cells["filesinfo"].Value.ToString().Equals(searchValue) || row.Cells["filesinfo_w"].Value.ToString().Equals(searchValue))
                        {
                            string sql_select = "SELECT key_unic,translator_m FROM Translated WHERE text_en='" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "'";
                            sqlite_cmd.CommandText = sql_select;
                            SQLiteDataReader s = sqlite_cmd.ExecuteReader();
                            while (s.Read())
                            {
                                if (change_other_strings == "no")
                                {
                                    if (row.Cells["key"].Value.ToString() == s["key_unic"].ToString() || s["translator_m"].ToString() == "Deepl")
                                    {
                                        if (!list_keys.Contains(s["key_unic"].ToString()))
                                            list_keys.Add(s["key_unic"].ToString());
                                    }
                                }
                                else if (change_other_strings == "yes")
                                {
                                        if (!list_keys.Contains(s["key_unic"].ToString()))
                                            list_keys.Add(s["key_unic"].ToString());
                                }  
                            }
                            s.Close();
                            list_keys.ForEach(delegate (string name)
                            {
                                if (Convert.ToString(row.Cells["text_ru_w"].Value) == "" && authorization == true)
                                {
                                    if (row.Cells["filesinfo"].Value.ToString().Equals(searchValue) && row.Cells["filesinfo_w"].Value.ToString().Equals(searchValue))
                                    {
                                        sql_insert = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "',text_ru_w=NULL,translator_m='" + WebUtility.HtmlEncode(new_authr) + "',translator_w=NULL WHERE key_unic ='" + name + "'";
                                        xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(new_authr) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"\"></text_ru_w>";
                                    }
                                    else if (row.Cells["filesinfo"].Value.ToString().Equals(searchValue))
                                    {
                                        sql_insert = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "',translator_m='" + WebUtility.HtmlEncode(new_authr) + "' WHERE key_unic ='" + name + "'";
                                        xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(new_authr) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"\"></text_ru_w>";
                                    }
                                    else if (row.Cells["filesinfo_w"].Value.ToString().Equals(searchValue))
                                    {
                                        sql_insert = "UPDATE Translated SET text_ru_w=NULL,translator_w=NULL WHERE key_unic ='" + name + "'";
                                        xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(row.Cells["translator_m"].Value.ToString()) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"\"></text_ru_w>";
                                    }
                                }
                                else
                                {
                                    if (row.Cells["filesinfo"].Value.ToString().Equals(searchValue) && row.Cells["filesinfo_w"].Value.ToString().Equals(searchValue))
                                    {
                                        sql_insert = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "',text_ru_w='" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "',translator_m='" + WebUtility.HtmlEncode(new_authr) + "',translator_w='" + WebUtility.HtmlEncode(new_authr) + "' WHERE key_unic ='" + name + "'";
                                        xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(new_authr) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(new_authr) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "</text_ru_w>";
                                    }
                                    else if (row.Cells["filesinfo"].Value.ToString().Equals(searchValue))
                                    {
                                        sql_insert = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "',translator_m='" + WebUtility.HtmlEncode(new_authr) + "' WHERE key_unic ='" + name + "'";
                                        xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(new_authr) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(row.Cells["translator_w"].Value.ToString()) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "</text_ru_w>";
                                    }
                                    else if (row.Cells["filesinfo_w"].Value.ToString().Equals(searchValue))
                                    {
                                        sql_insert = "UPDATE Translated SET text_ru_w='" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "',translator_w='" + WebUtility.HtmlEncode(new_authr) + "' WHERE key_unic ='" + name + "'";
                                        xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(row.Cells["translator_m"].Value.ToString()) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(new_authr) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "</text_ru_w>";
                                    }
                                }

                                using (StreamWriter file_for_exam =
                         new StreamWriter("user_translation\\" + fileinfo_user.Text + ".xml", true, encoding: Encoding.UTF8))
                                {
                                    file_for_exam.WriteLine(xml_text);
                                }
                                sqlite_cmd.CommandText = sql_insert;
                                sqlite_cmd.ExecuteNonQuery();
                                num_edited_rows++;
                                xml_text = "";
                            });
                            if (row.Cells["filesinfo"].Value.ToString() == "2")
                            {
                                row.Cells["filesinfo"].Value = "0";
                            }

                            if (row.Cells["filesinfo_w"].Value.ToString() == "2")
                            {
                                row.Cells["filesinfo_w"].Value = "0";
                            }
                        }
                        progressBar_text.Invoke((MethodInvoker)(() => progressBar_text.Value += 1));
                        list_keys.Clear();
                    }
                }
                sqlite_conn.Close();
            }
            progressBar_text.Invoke((MethodInvoker)(() => progressBar_text.Visible = false));
            loading_text.Invoke((MethodInvoker)(() => loading_text.Visible = false));
            using (StreamWriter file_for_exam =
                                                new StreamWriter("user_translation\\" + fileinfo_user.Text + ".xml", true, encoding: Encoding.UTF8))
            {
                file_for_exam.WriteLine("</rezult>");
            }
            if (num_edited_rows != 0)
            {
                //buffer = "";
                MessageBox.Show("Файл сохранён в папке user_translation! Выгружено " + num_edited_rows + " строк.");
            }
            else
            {
                //buffer = "";
                MessageBox.Show("Выгружать нечего!");
            }
*/        }

        private async void page_lst_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (page_transl != page_lst.SelectedItem.ToString())
            {
                page_transl = page_lst.SelectedItem.ToString();
                await Task.Run(() => data_load());
            }   
        }

        private async void colrowonpage_SelectedIndexChanged(object sender, EventArgs e)
        {           
            if (count_per_pages != Int32.Parse(colrowonpage.SelectedItem.ToString()))
            {
                count_per_pages = Int32.Parse(colrowonpage.SelectedItem.ToString());
                await Task.Run(() => data_load());
            }
        }
        private void data_load()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    source_list_m.Clear();
                    source_list_w.Clear();
                    source_list_m_author.Clear();
                    source_list_w_author.Clear();
                    data_trans_file.Rows.Clear();
                    string sql_insert_part2 = "";
                    string sql_insert_part3 = "";
                    string sql_insert = "";
                    int count = 0;
                    int countpgs = 0;
                    if (colrowonpage.SelectedIndex != -1)
                        count_per_pages = Int32.Parse(colrowonpage.Text);
                    else
                        colrowonpage.SelectedIndex = colrowonpage.FindStringExact(count_per_pages.ToString());
                    using (SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; "))
                    {
                        sqlite_conn.Open();
                        using (SQLiteCommand sqlite_cmd = new SQLiteCommand(sqlite_conn))
                        {
                            if (searchbox.Text != "")
                            {
                                if (full_text.Checked == true)
                                {
                                    if (search_ru.Checked == true)
                                        sql_insert_part2 = "(text_ru_m = '" + WebUtility.HtmlEncode(searchbox.Text) + "' OR text_ru_w = '" + WebUtility.HtmlEncode(searchbox.Text) + "')";
                                    if (search_en.Checked == true)
                                        sql_insert_part2 = "text_en = '" + WebUtility.HtmlEncode(searchbox.Text) + "'";
                                }
                                else
                                {
                                    if (search_ru.Checked == true)
                                        sql_insert_part2 = "(text_ru_m like '%" + WebUtility.HtmlEncode(searchbox.Text) + "%' OR text_ru_w like '%" + WebUtility.HtmlEncode(searchbox.Text) + "%')";
                                    if (search_en.Checked == true)
                                        sql_insert_part2 = "text_en like '%" + WebUtility.HtmlEncode(searchbox.Text) + "%'";
                                }
                                if (checkBox1.Checked == true || authorization == false)
                                {
                                    sql_insert_part2 += " AND ((translator_m='Deepl' OR translator_w='Deepl') OR (translator_m='" + new_author.Text + "' OR translator_w='" + new_author.Text + "'))";
                                }
                            }
                            else if (checkBox1.Checked == true || authorization == false)
                            {
                                sql_insert_part2 = " AND ((translator_m='Deepl' OR translator_w='Deepl') OR (translator_m='" + new_author.Text + "' OR translator_w='" + new_author.Text + "'))";
                            }

                            if (searchbox.Text != "")
                                sql_insert = "SELECT COUNT(DISTINCT text_en) FROM Translated WHERE " + sql_insert_part2;
                            else
                                sql_insert = "SELECT COUNT(DISTINCT text_en) FROM Translated WHERE fileinfo='" + file_to_trans.Text + "'" + sql_insert_part2;
                            sqlite_cmd.CommandText = sql_insert;
                            SQLiteDataReader s = sqlite_cmd.ExecuteReader();
                            while (s.Read())
                            {
                                count = int.Parse(s["COUNT(DISTINCT text_en)"].ToString());
                            }
                            s.Close();
                            countpgs = count / count_per_pages;
                            countpgs += 1;
                            if (go_to_file == "1")
                            {
                                page_lst.Items.Clear();
                                for (int i = 1; i <= countpgs; i++)
                                {
                                    page_lst.Items.Add(i);
                                }                                
                                page_lst.SelectedIndex = page_lst.FindStringExact(page_transl);
                                sql_insert_part3 = " LIMIT " + (count_per_pages * int.Parse(page_transl) - count_per_pages) + "," + count_per_pages;
                            }
                            else if (page_lst.Items.Count != countpgs)
                            {
                                page_lst.Items.Clear();
                                for (int i = 1; i <= countpgs; i++)
                                {
                                    page_lst.Items.Add(i);
                                }
                                page_transl = "1";
                                page_lst.SelectedIndex = page_lst.FindStringExact("1");
                                sql_insert_part3 = " LIMIT " + count_per_pages;
                            }
                            else if (page_lst.Text == "")
                            {
                                sql_insert_part3 = " LIMIT " + count_per_pages;
                                page_transl = "1";
                                page_lst.SelectedIndex = page_lst.FindStringExact("1");
                            }
                            else
                            {
                                sql_insert_part3 = " LIMIT " + (count_per_pages * int.Parse(page_lst.Text) - count_per_pages) + "," + count_per_pages;
                            }

                            if (searchbox.Text != "")
                                sql_insert = "SELECT key_unic,text_en,text_ru_m,text_ru_w,translator_m,translator_w FROM Translated WHERE " + sql_insert_part2 + " GROUP BY text_en ORDER by ID" + sql_insert_part3;
                            else
                                sql_insert = "SELECT key_unic,text_en,text_ru_m,text_ru_w,translator_m,translator_w FROM Translated WHERE fileinfo='" + file_to_trans.Text + "'" + sql_insert_part2 + " GROUP BY text_en ORDER by ID" + sql_insert_part3 ;
                            sqlite_cmd.CommandText = sql_insert;
                            SQLiteDataReader r = sqlite_cmd.ExecuteReader();
                            loading_text.Parent = progressBar_text;
                            loading_text.BackColor = Color.Transparent;
                            loading_text.Visible = true;
                            progressBar_text.Visible = true;
                            progressBar_text.Value = 0;
                            progressBar_text.Maximum = count_per_pages;
                            while (r.Read())
                            {
                                int rowNumber = data_trans_file.Rows.Add();
                                data_trans_file.Rows[rowNumber].Cells["key"].Value = WebUtility.HtmlDecode(r["key_unic"].ToString());
                                data_trans_file.Rows[rowNumber].Cells["text_en"].Value = WebUtility.HtmlDecode(r["text_en"].ToString());
                                data_trans_file.Rows[rowNumber].Cells["text_ru_m"].Value = WebUtility.HtmlDecode(r["text_ru_m"].ToString());
                                data_trans_file.Rows[rowNumber].Cells["text_ru_w"].Value = WebUtility.HtmlDecode(r["text_ru_w"].ToString());
                                data_trans_file.Rows[rowNumber].Cells["translator_m"].Value = WebUtility.HtmlDecode(r["translator_m"].ToString());
                                data_trans_file.Rows[rowNumber].Cells["translator_w"].Value = WebUtility.HtmlDecode(r["translator_w"].ToString());
                                data_trans_file.Rows[rowNumber].Cells["filesinfo"].Value = "0";
                                data_trans_file.Rows[rowNumber].Cells["filesinfo_w"].Value = "0";
                                if (searching_row_id == WebUtility.HtmlDecode(r["key_unic"].ToString()))
                                {
                                    data_trans_file.Rows[0].Selected = false;
                                    data_trans_file.Rows[rowNumber].Selected = true;
                                }

                                source_list_m.Add(WebUtility.HtmlDecode(r["text_ru_m"].ToString()));
                                source_list_w.Add(WebUtility.HtmlDecode(r["text_ru_w"].ToString()));

                                source_list_m_author.Add(WebUtility.HtmlDecode(r["translator_m"].ToString()));
                                source_list_w_author.Add(WebUtility.HtmlDecode(r["translator_w"].ToString()));

                                /*if (!list_translators.Contains(WebUtility.HtmlDecode(r["translator_m"].ToString())))
                                    list_translators.Add(WebUtility.HtmlDecode(r["translator_m"].ToString()));
                                if (!list_translators.Contains(WebUtility.HtmlDecode(r["translator_w"].ToString())))
                                    list_translators.Add(WebUtility.HtmlDecode(r["translator_w"].ToString()));*/
                                progressBar_text.Value+= 1;
                            }
                            
                            r.Close();
                        }
                        sqlite_conn.Close();
                    }
                    /*string list_translator = "";
                    foreach (string list_trans in list_translators)
                    {
                        if (list_translator != "" && list_trans == "1")
                            list_translator += ",Togruth";
                        else if (list_trans == "1")
                            list_translator = "Togruth";
                        if (list_translator != "" && list_trans == "2")
                            list_translator += ",JKC";
                        else if (list_trans == "2")
                            list_translator = "JKC";
                        if (list_translator != "" && list_trans == "Deepl")
                            list_translator += ",Deepl";
                        else if (list_trans == "Deepl")
                            list_translator = "Deepl";
                        if (list_translator != "" && list_trans == "4")
                            list_translator += ",Krestnik02";
                        else if (list_trans == "4")
                            list_translator = "Krestnik02";
                        if (list_translator != "" && list_trans == "5")
                            list_translator += ",Другие переводчики";
                        else if (list_trans == "5")
                            list_translator = "Другие переводчики";
                        if (list_translator != "" && list_trans != "" && list_trans != "1" && list_trans != "2" && list_trans != "Deepl" && list_trans != "4" && list_trans != "5")
                            list_translator += "," + list_trans;
                        else if (list_trans != "" && list_trans != "1" && list_trans != "2" && list_trans != "Deepl" && list_trans != "4" && list_trans != "5")
                            list_translator = list_trans;
                    }*/
                    /*translated.Text = list_translator;*/
                    //set autosize mode
                    data_trans_file.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    data_trans_file.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    data_trans_file.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    //datagrid has calculated it's widths so we can store them
                    for (int i = 0; i <= data_trans_file.Columns.Count - 1; i++)
                    {
                        //store autosized widths
                        int colw = data_trans_file.Columns[i].Width;
                        //remove autosizing
                        data_trans_file.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                        //set width to calculated by autosize
                        data_trans_file.Columns[i].Width = colw;
                    }
                    upload_translate.Enabled = false;
                    if (countpgs > 1)
                        page_lst.Enabled = true;
                    else
                        page_lst.Enabled = false;
                    lst_font.Enabled = true;
                    lst_font.SelectedIndex = lst_font.FindStringExact(lst_font_change);
                    loading_text.Visible = false;
                    progressBar_text.Visible = false;
                    go_to_file = "0";
                }));
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            searchbox.Text = "";
            selected_file_to_transl = "";
            translated.Text = "Неизвестно";
            upload_translate.Enabled = false;
            lst_font.Enabled = false;
            colrowonpage.Enabled = false;
            page_lst.Items.Clear();
            page_lst.Enabled = false;
            file_to_trans.Items.Clear();
            source_list_m.Clear();
            source_list_w.Clear();
            source_list_m_author.Clear();
            source_list_w_author.Clear();
            data_trans_file.Rows.Clear();
            string sql_insert;
            SQLiteConnection sqlite_conn;
            sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; ");
            sqlite_conn.Open();
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqlite_conn.CreateCommand();
            sql_insert = "SELECT fileinfo FROM Translated WHERE ((translator_m='Deepl' OR translator_w='Deepl') OR (translator_m='" + new_author.Text + "' OR translator_w='" + new_author.Text + "')) GROUP by fileinfo;";
            sqlite_cmd.CommandText = sql_insert;
            SQLiteDataReader r = sqlite_cmd.ExecuteReader();
            while (r.Read())
                file_to_trans.Items.Add(r["fileinfo"].ToString());
            r.Close();
            sqlite_conn.Close();
            gotofile.Enabled = false;
            search_filename.Enabled = false;
        }

        private async void search_button_Click(object sender, EventArgs e)
        {
            if (searchbox.Text!="")
            {
                selected_file_to_transl = "";
                file_to_trans.SelectedIndex = -1;
                await Task.Run(() => data_load());
                if (data_trans_file.Rows.Count>0)
                {
                    gotofile.Enabled = true;
                    search_filename.Enabled = true;
                }                
            }
            if (fileinfo_user.Text != "")
                fileinfo_user.Enabled = true;
        }

        private void search_filename_Click(object sender, EventArgs e)
        {
            if (searchbox.Text!="")
            {
                int current_row = data_trans_file.CurrentCell.RowIndex;
                using (SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; "))
                {
                    sqlite_conn.Open();
                    using (SQLiteCommand sqlite_cmd = new SQLiteCommand(sqlite_conn))
                    {
                        string sql_select = "SELECT fileinfo FROM Translated WHERE key_unic='" + data_trans_file.Rows[current_row].Cells["key"].Value + "'";
                        sqlite_cmd.CommandText = sql_select;
                        SQLiteDataReader r = sqlite_cmd.ExecuteReader();
                        while (r.Read())
                        {
                            file_name_search.Text = "Строка находится в файле:" + r["fileinfo"].ToString();
                            break;
                        }
                        r.Close();
                    }
                    sqlite_conn.Close();
                }
            }
        }
        private async void gotofile_Click(object sender, EventArgs e)
        {
            string sql_select;
            int jks = 1;
            int page_num = 1;
            
            if (searchbox.Text != "")
            {
                int current_row = data_trans_file.CurrentCell.RowIndex;
                string select_file = "";
                using (SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; "))
                {
                    sqlite_conn.Open();
                    using (SQLiteCommand sqlite_cmd = new SQLiteCommand(sqlite_conn))
                    {
                        sql_select = "SELECT fileinfo FROM Translated WHERE key_unic='" + data_trans_file.Rows[current_row].Cells["key"].Value + "'";
                        sqlite_cmd.CommandText = sql_select;
                        SQLiteDataReader r = sqlite_cmd.ExecuteReader();
                        while (r.Read())
                        {
                            select_file = r["fileinfo"].ToString();
                            break;
                        }
                        r.Close();
                        if (checkBox1.Checked == true || authorization == false)
                            sql_select = "SELECT key_unic,hash FROM Translated WHERE fileinfo='" + select_file + "' AND ((translator_m='Deepl' OR translator_w='Deepl') OR (translator_m='" + new_author.Text + "' OR translator_w='" + new_author.Text + "')) GROUP BY text_en ORDER by id";
                        else
                            sql_select = "SELECT key_unic,hash FROM Translated WHERE fileinfo='" + select_file + "' GROUP BY text_en ORDER by id";
                        sqlite_cmd.CommandText = sql_select;
                        SQLiteDataReader s = sqlite_cmd.ExecuteReader();                        
                        while (s.Read())
                        {
                            if (s["key_unic"].ToString() == data_trans_file.Rows[current_row].Cells["key"].Value.ToString())
                                break;
                            if (jks % count_per_pages == 0)
                                page_num += 1;
                            jks++;
                        }
                        s.Close();
                    }
                    sqlite_conn.Close();
                }
                page_transl = page_num.ToString();
                searching_row_id = data_trans_file.Rows[current_row].Cells["key"].Value.ToString();
                go_to_file = "1";
                file_name_search.Text = "";
                searchbox.Text = "";
                selected_file_to_transl = select_file;
                file_to_trans.SelectedIndex = file_to_trans.Items.IndexOf(select_file);
                await Task.Run(() => data_load());
                gotofile.Enabled = false;
                search_filename.Enabled = false;
            }
        }
        private AlternateView Mail_Body()
        {
            LinkedResource Img = new LinkedResource("Resources\\swtor.jpg", MediaTypeNames.Image.Jpeg);
            Img.ContentId = "logo";
            string str = @"<body><img src=cid:logo width='150' height='150' alt='Logo'/>
                    <p>Здраствуйте участник проекта <b>SWToR RUS</b>! Это письмо отправлено Вам, так как ваш перевод хотят изменить.<br /><br />
                    Пожалуйста воспользуйтесь редактором перевода <b>SWToR RUS</b> для внесения изменений, если вы посчитаете их нужными.
                    <br /><br />Это письмо отправленно автоматически, пожалуйста не отвечайте на него.
                    <p>С теплыми пожеланиями, <b>SWTOR RUS COMMUNITY</b></p></p></body>";
            AlternateView AV = AlternateView.CreateAlternateViewFromString(str, null, MediaTypeNames.Text.Html);
            AV.LinkedResources.Add(Img);
            return AV;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {

            string[] allfiles = Directory.GetFiles("export\\", "*", SearchOption.AllDirectories);
            string[] stringSeparators = new string[] { "export\\error_", ".xml" };

            for (int i = 0; i < allfiles.Length; i++)
            {
                try
                {
                    SmtpClient client = new SmtpClient();
                    client.Host = "smtp.gmail.com";
                    client.Port = 587;
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("swtor2com@gmail.com", "sAq12w#$%");
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    MailMessage message = new MailMessage();
                    message.From = new MailAddress("swtor2com@gmail.com", "SWToR_RUS COMMUNITY");
                    message.To.Add(new MailAddress(string.Join("", allfiles[i].Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries))));
                    message.Subject = "Ваши строчки перевода хотят изменить";
                    message.Attachments.Add(new Attachment(allfiles[i].ToString()));
                    message.AlternateViews.Add(Mail_Body());
                    client.Send(message);

                }
                catch(SmtpFailedRecipientException ex)
                {
                    MessageBox.Show(ex + "\n" + "Пожалуйста предоставьте скриншот с данным сообщением администратору проекта", "Ошибка", MessageBoxButtons.OK);
                }
            }

            Form ifrm = Application.OpenForms[0];
            ifrm.Show();

            //закрываем форму авторизации, если открыта
            Form fc = Application.OpenForms["Form3"];
            if (fc != null)
            {
                fc.Hide();
            }
            Hide();
        }

        private void searchbox_TextChanged(object sender, EventArgs e)
        {
            source_list_m.Clear();
            source_list_w.Clear();
            source_list_m_author.Clear();
            source_list_w_author.Clear();
            data_trans_file.Rows.Clear();
            page_lst.Enabled = false;
            upload_translate.Enabled = false;
        }

        private async void data_trans_file_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            who_talk.Invoke((MethodInvoker)(() => who_talk.Text = "Загружаю информацию..."));
            await Task.Run(() => see_who_talk());
        }
        public void see_who_talk()
        {
            string sql_select;
            string quest_name_new="";
            List<string> quest_name_n = new List<string>();
            string who_talk_new = "";
            List<string> who_talk_n = new List<string>();
            string to_whom_talk_new = "";
            List<string> to_whom_talk_n = new List<string>();
            using (SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; "))
            {
                sqlite_conn.Open();
                using (SQLiteCommand sqlite_cmd = new SQLiteCommand(sqlite_conn))
                {                    
                    sql_select = "SELECT quest_name,who_talk,to_whom_talk FROM Conversations WHERE text_en_id='" + data_trans_file.Rows[data_trans_file.CurrentCell.RowIndex].Cells["key"].Value + "'";
                    sqlite_cmd.CommandText = sql_select;
                    SQLiteDataReader r = sqlite_cmd.ExecuteReader();
                    while (r.Read())
                    {
                        if (!quest_name_n.Contains(r["quest_name"].ToString()))
                            quest_name_n.Add(r["quest_name"].ToString());
                        if (!who_talk_n.Contains(r["who_talk"].ToString()))
                            who_talk_n.Add(r["who_talk"].ToString());
                        if (!to_whom_talk_n.Contains(r["to_whom_talk"].ToString()))
                            to_whom_talk_n.Add(r["to_whom_talk"].ToString());                       
                    }
                    foreach (var quest_name_na in quest_name_n)
                    {
                        if (quest_name_new == "")
                            quest_name_new += quest_name_na;
                        else
                            quest_name_new += "," + quest_name_na;
                    }
                    foreach (var who_talk_na in who_talk_n)
                    {
                        if (who_talk_new == "")
                            who_talk_new += who_talk_na;
                        else
                            who_talk_new += "," + who_talk_na;
                    }
                    foreach (var to_whom_talk_na in to_whom_talk_n)
                    {
                        if (to_whom_talk_new == "")
                            to_whom_talk_new += to_whom_talk_na;
                        else
                            to_whom_talk_new += "," + to_whom_talk_na;
                    }
                    r.Close();
                    string full_text="";
                    if (quest_name_new != "")
                        full_text += "Квест: " + quest_name_new + ";";
                    if (who_talk_new != "")
                        full_text += "Говорит: " + who_talk_new + ";";
                    if (to_whom_talk_new != "")
                        full_text += "Обращается к: " + to_whom_talk_new + ";";                   
                    who_talk.Invoke((MethodInvoker)(() => who_talk.Text = full_text));
                }
                sqlite_conn.Close();
            }                    
        }
        public void delete_xml_trash(XmlNode Key)
        {
            /*Доделать*/
            /*XmlElement KeyElement = (XmlElement)Key;
            KeyElement.InnerText = */
        }

        private void export_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists("export"))
            {
                Directory.CreateDirectory("export");
            }

            //формируем запрос
            string join = String.Join("' , '", data_name.Cast<string>()
                                 .Where(c => !string.IsNullOrWhiteSpace(c))
                                 .Distinct());

            MessageBox.Show(join.ToString(), "Внимание", MessageBoxButtons.OK);

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Title = "Выберите путь";
            saveFileDialog1.Filter = "XML-File | *.xml";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory() +"//export";
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {                
                SQLiteConnection sqlite_conn;
                sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; ");
                sqlite_conn.Open();
                SQLiteCommand sqlite_cmd;
                sqlite_cmd = sqlite_conn.CreateCommand();

                //выгружаем строки только неавторизированных авторов
                string sql_insert = "SELECT key_unic,text_en,text_ru_m,text_ru_w,translator_m,translator_w FROM Translated WHERE fileinfo='" + file_to_trans.Text + "' AND " +
                    "(translator_m NOT IN ('" + join + "') AND (translator_w NOT IN ('" + join + "') OR translator_w IS NULL)) ORDER BY ID";
                sqlite_cmd.CommandText = sql_insert;
                SQLiteDataReader r = sqlite_cmd.ExecuteReader();
                using (StreamWriter file_for_exam =
                             new StreamWriter(saveFileDialog1.FileName, true, encoding: Encoding.UTF8))
                {                    
                    file_for_exam.WriteLine("<rezult>");
                }
                while (r.Read())
                {
                    string xml_text = "<key>" + r["key_unic"].ToString() + "</key><text_en>" + r["text_en"].ToString() + "</text_en><text_ru_m transl=\"" + r["translator_m"].ToString() + "\">" + r["text_ru_m"].ToString() + "</text_ru_m><text_ru_w  transl=\"" + r["translator_w"].ToString() + "\">" + r["text_ru_w"].ToString() + "</text_ru_w>";
                    using (StreamWriter file_for_exam =
                                                 new StreamWriter(saveFileDialog1.FileName, true, encoding: Encoding.UTF8))
                    {
                        file_for_exam.WriteLine(xml_text);
                    }
                }
                using (StreamWriter file_for_exam =
                                             new StreamWriter(saveFileDialog1.FileName, true, encoding: Encoding.UTF8))
                {
                    file_for_exam.WriteLine("</rezult>");
                }
                r.Close();
                sqlite_conn.Close();                
            }                      
        }

        private void Wiki_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://starwars.fandom.com/ru/wiki/");
        }

        private void tor_site_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://torcommunity.com/");
        }

        private void html_spec_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://html5book.ru/specsimvoly-html/");
        }

        private void auth_Click(object sender, EventArgs e)
        {
            if(new_author.Text != "Напишите своё имя или оставьте как есть" && new_author.Text != "")
            {
                Data.Value = new_author.Text;
                auth.Enabled = false;
                Form3 form3 = new Form3();
                form3.Show();
            } else
            {
                auth.Enabled = false;
                Form3 form3 = new Form3();
                form3.Show();
            }
        }

        private void author_ok_Click(object sender, EventArgs e)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (ConfigurationManager.AppSettings["author"] != null && configuration.AppSettings.Settings["author"].Value != "" )
            {
                MessageBox.Show("Ваш конфигурационный файл поврежден, обратитесь к администратору", "Ошибка", MessageBoxButtons.OK);
            } 
            else if (new_author.Text.ToString() == "Напишите своё имя или оставьте как есть" || new_author.Text.ToString() == "")
            {
                MessageBox.Show("Заполните поле \"Автор перевода\"", "Ошибка", MessageBoxButtons.OK);
            }
            else
            {
                Regex regex = new Regex(@"^[A-Za-z0-9]+$");
                if(regex.IsMatch(new_author.Text)) {

                    using (MySqlConnection conn = new MySqlConnection(connStr_mysql))
                    {
                        conn.Open();
                        string sql = "SELECT name FROM users WHERE name='" + new_author.Text.ToString() + "'";
                        MySqlCommand command = new MySqlCommand(sql, conn);
                        MySqlDataReader row = command.ExecuteReader();
                        if (row.HasRows || new_author.Text == "deepl" || new_author.Text == "Deepl")
                        {
                            DialogResult dialogResult = MessageBox.Show("В поле \"Автор перевода\" введен существующий автор, вам необходимо авторизоваться под этим именем", "Внимание", MessageBoxButtons.YesNo);
                            if (dialogResult == DialogResult.Yes)
                            {
                                new_author.Enabled = false;
                                author_ok.Enabled = false;
                                Data.Value = new_author.Text;
                                Form3 form3 = new Form3();
                                form3.Show();
                            }
                            else
                            {
                                auth.Enabled = true;
                                Data.Trigger = 0;
                                new_author.Enabled = true;
                                author_ok.Enabled = true;
                                searchbox.Enabled = false;
                                file_to_trans.Enabled = false;
                                new_author.Clear();
                            }
                        } else
                        {
                            searchbox.Enabled = true;
                            file_to_trans.Enabled = true;
                            new_author.Enabled = false;
                            author_ok.Enabled = false;
                            configuration.AppSettings.Settings["author"].Value = new_author.Text.ToString();
                            configuration.Save(ConfigurationSaveMode.Modified);
                            auth.Enabled = true;
                            //выводим список в комбобоксе
                            SQLiteConnection sqlite_conn;
                            sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; ");
                            sqlite_conn.Open();
                            SQLiteCommand sqlite_cmd;
                            sqlite_cmd = sqlite_conn.CreateCommand();
                            string sql_insert = "SELECT fileinfo FROM Translated WHERE ((translator_m='Deepl' OR translator_w='Deepl') OR (translator_m='" + new_author.Text + "' OR translator_w='" + new_author.Text + "')) GROUP by fileinfo";
                            sqlite_cmd.CommandText = sql_insert;
                            SQLiteDataReader r = sqlite_cmd.ExecuteReader();
                            while (r.Read())
                                file_to_trans.Items.Add(r["fileinfo"].ToString());
                            r.Close();
                            sqlite_conn.Close();
                        }
                        conn.Close();
                    }
                } else
                {
                    MessageBox.Show("Поле \"Автор перевода\" содержит недопустимые символы", "Ошибка", MessageBoxButtons.OK);
                }
            }
        }

        private void new_author_Click(object sender, EventArgs e)
        {
            if (new_author.Text == "Напишите своё имя или оставьте как есть")
            {
                new_author.Clear();
            }
        }

        private void Form2_Activated(object sender, EventArgs e)
        {
            if(Data.Trigger == 3)
            {
                auth.Enabled = true;
                Data.Trigger = 0;
            }

            if (Data.Value != "" && Data.Value != null && Data.Trigger == 2)
            {
                new_author.Enabled = false;
                new_author.Text = Data.Value;
                this.Text = Data.Title;
                auth.Enabled = false;
                author_ok.Enabled = false;
                authorization = true;
                file_to_trans.Enabled = true;
                searchbox.Enabled = true;

                //выводим список в комбобоксе
                SQLiteConnection sqlite_conn;
                sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; ");
                sqlite_conn.Open();
                SQLiteCommand sqlite_cmd;
                sqlite_cmd = sqlite_conn.CreateCommand();
                string sql_insert = "SELECT fileinfo FROM Translated GROUP by fileinfo"; 
                sqlite_cmd.CommandText = sql_insert;
                SQLiteDataReader r = sqlite_cmd.ExecuteReader();
                while (r.Read())
                    file_to_trans.Items.Add(r["fileinfo"].ToString());
                r.Close();
                sqlite_conn.Close();
            }
        }
    }
}
