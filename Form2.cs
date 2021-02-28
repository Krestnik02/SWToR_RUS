using System;
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
using System.Threading;

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

        public bool author_edit_start = false;

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
                                    source_editor.Visible = true;

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
                        } else
                        {
                            MessageBox.Show("Пользователь не найден", "Ошибка авторизации", MessageBoxButtons.OK);
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
                        List<string> list_translators = new List<string>();
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
                        gotofile.Enabled = false;
                        search_filename.Enabled = false;
                    }
                }
                if(authorization == true)
                    export.Enabled = true;
            }
            else
                export.Enabled = false;
        }

        public void data_upload_trans(DataGridViewCellEventArgs e, int status, string xml_text = "", string searchValue_m = "", string searchValue_w = "", string file = "")
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
                        if (row.Cells["filesinfo"].Value.ToString().Equals(searchValue_m) || row.Cells["filesinfo_w"].Value.ToString().Equals(searchValue_w))
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

                                    if (row.Cells["filesinfo"].Value.ToString().Equals(searchValue_m))
                                    {
                                        if (Convert.ToString(row.Cells["text_ru_m"].Value) == "")
                                        {
                                            sql_insert = "UPDATE Translated SET text_ru_m=NULL, translator_m=NULL WHERE key_unic ='" + name + "'";
                                            xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"\"></text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(row.Cells["translator_w"].Value.ToString()) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "</text_ru_w><source_m transl=\"" + WebUtility.HtmlEncode(source_list_m_author[e.RowIndex]) + "\">" + WebUtility.HtmlEncode(source_list_m[e.RowIndex]) + "</source_m><source_w transl=\"" + WebUtility.HtmlEncode(source_list_w_author[e.RowIndex]) + "\">" + WebUtility.HtmlEncode(source_list_w[e.RowIndex]) + "</source_w>";
                                        } else
                                        {
                                            xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(new_author.Text) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(row.Cells["translator_w"].Value.ToString()) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "</text_ru_w><source_m transl=\"" + WebUtility.HtmlEncode(source_list_m_author[e.RowIndex]) + "\">" + WebUtility.HtmlEncode(source_list_m[e.RowIndex]) + "</source_m><source_w transl=\"" + WebUtility.HtmlEncode(source_list_w_author[e.RowIndex]) + "\">" + WebUtility.HtmlEncode(source_list_w[e.RowIndex]) + "</source_w>";
                                        }
                                    }
                                    else if (row.Cells["filesinfo_w"].Value.ToString().Equals(searchValue_w))
                                    {
                                        if (Convert.ToString(row.Cells["text_ru_w"].Value) == "")
                                        {
                                            xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(row.Cells["translator_m"].Value.ToString()) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"\"></text_ru_w><source_m transl=\"" + WebUtility.HtmlEncode(source_list_m_author[e.RowIndex]) + "\">" + WebUtility.HtmlEncode(source_list_m[e.RowIndex]) + "</source_m><source_w transl=\"" + WebUtility.HtmlEncode(source_list_w_author[e.RowIndex]) + "\">" + WebUtility.HtmlEncode(source_list_w[e.RowIndex]) + "</source_w>";
                                        } else
                                        {
                                            xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(row.Cells["translator_m"].Value.ToString()) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(new_author.Text) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "</text_ru_w><source_m transl=\"" + WebUtility.HtmlEncode(source_list_m_author[e.RowIndex]) + "\">" + WebUtility.HtmlEncode(source_list_m[e.RowIndex]) + "</source_m><source_w transl=\"" + WebUtility.HtmlEncode(source_list_w_author[e.RowIndex]) + "\">" + WebUtility.HtmlEncode(source_list_w[e.RowIndex]) + "</source_w>";
                                        }
                                    }
                                }
                                else if (status == 1)
                                {
                                    if (Convert.ToString(row.Cells["text_ru_w"].Value) == "" && authorization == true)
                                    {
                                        if (row.Cells["filesinfo"].Value.ToString().Equals(searchValue_m) && row.Cells["filesinfo_w"].Value.ToString().Equals(searchValue_w))
                                        {
                                            sql_insert = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "',text_ru_w='" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "',translator_m='" + WebUtility.HtmlEncode(new_author.Text) + "',translator_w='" + WebUtility.HtmlEncode(new_author.Text) + "' WHERE key_unic ='" + name + "'";
                                            xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(new_author.Text) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(new_author.Text) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "</text_ru_w>";
                                        }
                                        else if (row.Cells["filesinfo"].Value.ToString().Equals(searchValue_m))
                                        {
                                            sql_insert = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "',translator_m='" + WebUtility.HtmlEncode(new_author.Text) + "' WHERE key_unic ='" + name + "'";
                                            xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(new_author.Text) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"\"></text_ru_w>";
                                        }
                                        else if (row.Cells["filesinfo_w"].Value.ToString().Equals(searchValue_w))
                                        {
                                            sql_insert = "UPDATE Translated SET text_ru_w=NULL,translator_w=NULL WHERE key_unic ='" + name + "'";
                                            xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(row.Cells["translator_m"].Value.ToString()) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"\"></text_ru_w>";
                                        }
                                    }
                                    else
                                    {
                                        if (row.Cells["filesinfo"].Value.ToString().Equals(searchValue_m) && row.Cells["filesinfo_w"].Value.ToString().Equals(searchValue_w))
                                        {
                                            sql_insert = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "',text_ru_w='" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "',translator_m='" + WebUtility.HtmlEncode(new_author.Text) + "',translator_w='" + WebUtility.HtmlEncode(new_author.Text) + "' WHERE key_unic ='" + name + "'";
                                            xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(new_author.Text) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(new_author.Text) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "</text_ru_w>";
                                        }
                                        else if (row.Cells["filesinfo"].Value.ToString().Equals(searchValue_m))
                                        {
                                            sql_insert = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "',translator_m='" + WebUtility.HtmlEncode(new_author.Text) + "' WHERE key_unic ='" + name + "'";
                                            xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(new_author.Text) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(row.Cells["translator_w"].Value.ToString()) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "</text_ru_w>";
                                        }
                                        else if (row.Cells["filesinfo_w"].Value.ToString().Equals(searchValue_w))
                                        {
                                            sql_insert = "UPDATE Translated SET text_ru_w='" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "',translator_w='" + WebUtility.HtmlEncode(new_author.Text) + "' WHERE key_unic ='" + name + "'";
                                            xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(row.Cells["translator_m"].Value.ToString()) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(new_author.Text) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "</text_ru_w>";
                                        }
                                    }
                                }

                                //редактируем строки, если соседняя строка авторская
                                else if (status == 2)
                                {
                                    //если авторская м
                                    if(row.Cells["filesinfo"].Value.ToString() == "2")
                                    {
                                        if (Convert.ToString(row.Cells["text_ru_w"].Value) == "")
                                        {
                                            sql_insert = "UPDATE Translated SET text_ru_w=NULL, translator_w=NULL WHERE key_unic ='" + name + "'";
                                            xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(source_list_m_author[e.RowIndex]) + "\">" + WebUtility.HtmlEncode(source_list_m[e.RowIndex]) + "</text_ru_m><text_ru_w transl=\"\"></text_ru_w>";
                                        }
                                        else if (Convert.ToString(row.Cells["text_ru_w"].Value) != "" && row.Cells["filesinfo_w"].Value.ToString().Equals(searchValue_w))
                                        {
                                            sql_insert = "UPDATE Translated SET text_ru_w='" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "', translator_w='" + WebUtility.HtmlEncode(new_author.Text) + "' WHERE key_unic ='" + name + "'";
                                            xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(source_list_m_author[e.RowIndex]) + "\">" + WebUtility.HtmlEncode(source_list_m[e.RowIndex]) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(new_author.Text) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_w"].Value.ToString()) + "</text_ru_w>";
                                        }
                                    }
                                    //если авторская ж
                                    if (row.Cells["filesinfo_w"].Value.ToString() == "2")
                                    {
                                        if (Convert.ToString(row.Cells["text_ru_m"].Value) == "")
                                        {
                                            MessageBox.Show("Вы не можете удалить эту строку", "Внимание", MessageBoxButtons.OK); //если пытается удалить строку
                                            data_trans_file.Rows[e.RowIndex].Cells[2].Value = source_list_m[e.RowIndex]; //возвращаем во вьюгрид исходную строку
                                            row.Cells["text_ru_m"].Value = source_list_m[e.RowIndex];
                                            xml_text = "";
                                            sql_insert = "";
                                        }
                                        else if (Convert.ToString(row.Cells["text_ru_m"].Value) != "" && row.Cells["filesinfo"].Value.ToString().Equals(searchValue_m))
                                        {
                                            sql_insert = "UPDATE Translated SET text_ru_m='" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "', translator_m='" + WebUtility.HtmlEncode(new_author.Text) + "' WHERE key_unic ='" + name + "'";
                                            xml_text = "<key>" + WebUtility.HtmlEncode(name) + "</key><text_en>" + WebUtility.HtmlEncode(row.Cells["text_en"].Value.ToString()) + "</text_en><text_ru_m transl=\"" + WebUtility.HtmlEncode(new_author.Text) + "\">" + WebUtility.HtmlEncode(row.Cells["text_ru_m"].Value.ToString()) + "</text_ru_m><text_ru_w transl=\"" + WebUtility.HtmlEncode(source_list_w_author[e.RowIndex]) + "\">" + WebUtility.HtmlEncode(source_list_w[e.RowIndex]) + "</text_ru_w>";
                                        }
                                    }
                                }

                                if (xml_text != "")
                                {
                                    using (StreamWriter file_for_exam = new StreamWriter(file, true, encoding: Encoding.UTF8))
                                    {
                                        file_for_exam.WriteLine(xml_text);
                                    }
                                }
                                if (sql_insert != "")
                                {
                                    sqlite_cmd.CommandText = sql_insert;
                                    sqlite_cmd.ExecuteNonQuery();
                                }
                                xml_text = "";
                            });
                            //сбрасываем состояние строки
                            row.Cells["filesinfo"].Value = "0";
                            row.Cells["filesinfo_w"].Value = "0";
                            data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "0"; 
                            data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "0";
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
            if(author_edit_start == false) { //выключаем обработку для обычных строк, когда редактируем авторские строки
                int alert_m = 0;
                int alert_w = 0;
                int alert_m_near = 0;
                int alert_w_near = 0;

                //проверяем на изменения с исходной строкой, если нет значит строка сделана им в этой сессии! Что бы не выводить ошибку, если он ввел текст, а потом удалил свой же
                if (Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[2].Value) == ""
                    && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[2].Value) != buffer_m
                    && source_list_m[e.RowIndex] != Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[2].Value))
                {
                    MessageBox.Show("Вы не можете удалить эту строку", "Внимание", MessageBoxButtons.OK); //если пытается удалить строку в м переводе, проверяем авторизован ли выводим мсбокс
                    data_trans_file.Rows[e.RowIndex].Cells[2].Value = source_list_m[e.RowIndex]; //возвращаем во вьюгрид исходную строку
                }

                //проверяем на изменения с исходной строкой, если нет значит строка сделана им в этой сессии! Что бы не выводить ошибку, если он ввел текст, а потом удалил свой же
                else if (Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[3].Value) == ""
                    && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[3].Value) != buffer_w
                    && source_list_w[e.RowIndex] != Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[3].Value)
                    && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[5].Value) != new_author.Text)
                {
                    MessageBox.Show("Только проверяющий может удалять строки других авторов", "Внимание", MessageBoxButtons.OK); //если пытается удалить строку в ж переводе, проверяем авторизован ли выводим мсбокс
                    data_trans_file.Rows[e.RowIndex].Cells[3].Value = source_list_w[e.RowIndex]; //возвращаем во вьюгрид исходную строку
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

                        data_upload_trans(e, 2, "", "1", "2", "user_translation\\user_translation.xml");
                    }

                    else if (alert_m == 1 && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[4].Value) != "") //проверяем если автор этой м строки авторизован
                    {
                        if (e.ColumnIndex == 2)
                            data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "2"; //помечаем строку, как строку которая внесена авторизированным автором

                        /* Заносим эти строки в отдельный файл */
                        DialogResult dialogResult = MessageBox.Show("Вы желаете изменить строку перевода авторизированного автора? Выберите \"Да\", если желате отправить автоматическое сообщение по завершению работы программы для уведомления его об этом", "Внимание", MessageBoxButtons.YesNo);
                        if (dialogResult == DialogResult.Yes)
                        {
                            data_upload_trans(e, 0, "", "2", "null", "export\\error_" + email + ".xml");

                            data_trans_file.Rows[e.RowIndex].Cells[2].Value = source_list_m[e.RowIndex]; //возвращаем во вьюгрид исходную строку
                            data_trans_file.CurrentCell.Style.Font = new Font("Microsoft Sans Serif", Convert.ToInt32(lst_font.Text), FontStyle.Regular); //помечаем строки которые изменяли

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
                    else 
                    {
                        if (e.ColumnIndex == 2)
                        {
                            //если изменены две строки сразу
                            if (Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[2].Value) != source_list_m[e.RowIndex]
                             && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[3].Value) != source_list_w[e.RowIndex])
                            {
                                data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "1"; //помечаем строку, как любую другую
                                data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "1"; //помечаем строку, как любую другую

                                data_upload_trans(e, 1, "", "1", "1", "user_translation\\user_translation.xml");

                            }
                            else
                            {
                                data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "1"; //помечаем строку, как любую другую

                                data_upload_trans(e, 1, "", "1", "null", "user_translation\\user_translation.xml");
                            }
                        }
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

                        data_upload_trans(e, 2, "", "2", "1", "user_translation\\user_translation.xml");
                    }

                    else if (alert_w == 1 && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[5].Value) != "") //проверяем если автор этой ж строки авторизован
                    {
                        if (e.ColumnIndex == 3)
                            data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "2"; //помечаем строку, как строку которая внесена авторизированным автором

                        /* Заносим эти строки в отдельный файл */
                        DialogResult dialogResult = MessageBox.Show("Вы желаете изменить строку перевода авторизированного автора? Выберите \"Да\", если желате отправить автоматическое сообщение по завершению работы программы для уведомления его об этом", "Внимание", MessageBoxButtons.YesNo);
                        if (dialogResult == DialogResult.Yes)
                        {
                            data_upload_trans(e, 0, "", "null", "2", "export\\error_" + email + ".xml");

                            data_trans_file.Rows[e.RowIndex].Cells[3].Value = source_list_m[e.RowIndex]; //возвращаем во вьюгрид исходную строку
                            data_trans_file.CurrentCell.Style.Font = new Font("Microsoft Sans Serif", Convert.ToInt32(lst_font.Text), FontStyle.Regular); //помечаем строки которые изменяли

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
                    else 
                    {
                        if (e.ColumnIndex == 3)
                        {
                            //если изменены две строки сразу
                            if (Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[2].Value) != source_list_m[e.RowIndex]
                             && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[3].Value) != source_list_w[e.RowIndex])
                            {
                                data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "1"; //помечаем строку, как любую другую
                                data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "1"; //помечаем строку, как любую другую

                                data_upload_trans(e, 1, "", "1", "1", "user_translation\\user_translation.xml");

                            }
                            else
                            {
                                data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "1"; //помечаем строку, как любую другую

                                data_upload_trans(e, 1, "", "null", "1", "user_translation\\user_translation.xml");
                            }
                        }
                    }
                }
                else if (source_list_m[e.RowIndex] == "" && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[2].Value) != "") //если строка изначально была пустая
                {
                    data_trans_file.CurrentCell.Style.Font = new Font("Microsoft Sans Serif", Convert.ToInt32(lst_font.Text), FontStyle.Bold); //помечаем строки которые изменяли
                    //если изменены две строки сразу
                    if (Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[2].Value) != source_list_m[e.RowIndex]
                     && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[3].Value) != source_list_w[e.RowIndex])
                    {
                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "1"; //помечаем строку, как любую другую
                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "1"; //помечаем строку, как любую другую

                        data_upload_trans(e, 1, "", "1", "1", "user_translation\\user_translation.xml");
                    }
                    else
                    {
                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "1"; //помечаем строку, как любую другую

                        data_upload_trans(e, 1, "", "1", "null", "user_translation\\user_translation.xml");
                    }
                }
                else if (source_list_w[e.RowIndex] == "" && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[3].Value) != "") //если строка изначально была пустая
                {
                    data_trans_file.CurrentCell.Style.Font = new Font("Microsoft Sans Serif", Convert.ToInt32(lst_font.Text), FontStyle.Bold); //помечаем строки которые изменяли
                    //если изменены две строки сразу
                    if (Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[2].Value) != source_list_m[e.RowIndex]
                     && Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[3].Value) != source_list_w[e.RowIndex])
                    {
                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "1"; //помечаем строку, как любую другую
                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "1"; //помечаем строку, как любую другую

                        data_upload_trans(e, 1, "", "1", "1", "user_translation\\user_translation.xml");

                    }
                    else
                    {
                        data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "1"; //помечаем строку, как любую другую

                        data_upload_trans(e, 1, "", "null", "1", "user_translation\\user_translation.xml");
                    }
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
                    List<string> list_translators = new List<string>();
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
                                    if (search_author.Checked == true)
                                        sql_insert_part2 = "translator_m = '" + WebUtility.HtmlEncode(searchbox.Text) + "' OR translator_w = '" + WebUtility.HtmlEncode(searchbox.Text) + "'";
                                }
                                else
                                {
                                    if (search_ru.Checked == true)
                                        sql_insert_part2 = "(text_ru_m like '%" + WebUtility.HtmlEncode(searchbox.Text) + "%' OR text_ru_w like '%" + WebUtility.HtmlEncode(searchbox.Text) + "%')";
                                    if (search_en.Checked == true)
                                        sql_insert_part2 = "text_en like '%" + WebUtility.HtmlEncode(searchbox.Text) + "%'";
                                    if (search_author.Checked == true)
                                        sql_insert_part2 = "translator_m like '%" + WebUtility.HtmlEncode(searchbox.Text) + "%' OR translator_w like '%" + WebUtility.HtmlEncode(searchbox.Text) + "%'";
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

                                if (!list_translators.Contains(WebUtility.HtmlDecode(r["translator_m"].ToString())))
                                    list_translators.Add(WebUtility.HtmlDecode(r["translator_m"].ToString()));
                                if (!list_translators.Contains(WebUtility.HtmlDecode(r["translator_w"].ToString())))
                                    list_translators.Add(WebUtility.HtmlDecode(r["translator_w"].ToString()));
                                progressBar_text.Value+= 1;
                            }
                            
                            r.Close();
                        }
                        sqlite_conn.Close();
                    }
                    string list_translator = "";
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
                    }
                    translated.Text = list_translator;
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
                    MessageBox.Show(ex.Message + "\n" + "Пожалуйста предоставьте скриншот с данным сообщением администратору проекта", "Ошибка", MessageBoxButtons.OK);
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
        public void delete_xml_trash()
        {
            //ДОДЕЛАТЬ РАБОТА ПО УДАЛЕНИЮ ДУБЛИРОВАННЫХ СТРОК
            /*string key_import = "";
            string[] allfiles = Directory.GetFiles("user_translation\\", "*", SearchOption.AllDirectories);
            foreach (string filename in allfiles)
            {
                //ProgressBar1.Invoke((MethodInvoker)(() => ProgressBar1.Value = 0));
                //int lineCount = File.ReadLines(filename).Count();
                //ProgressBar1.Invoke((MethodInvoker)(() => ProgressBar1.Maximum = lineCount - 2));

                XmlDocument xDoc1 = new XmlDocument();
                xDoc1.Load(filename);

                XmlElement xRoot1 = xDoc1.DocumentElement;
                XmlNodeList list = xDoc1.GetElementsByTagName("Key");

                if (list.Count > 0)
                {
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        XmlElement key = (XmlElement)list[i];
                        

                    }
                }
            }*/
        }
        private void source_editor_Click(object sender, EventArgs e)
        {
            //загружаем строки авторского перевода
            //создаем таблицу в БД если ее нет
            //СДЕЛАТЬ ПАКЕТНУЮ ЗАГРУЗКУ ФАЙЛОВ, ЧЕРЕЗ ДИАЛОГ
            using (SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; "))
            {
                sqlite_conn.Open();
                using (SQLiteCommand sqlite_cmd = new SQLiteCommand(sqlite_conn))
                {
                    string sql_create_table = "CREATE TABLE IF NOT EXISTS author_edit (" +
                        "ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "key_unic TEXT NOT NULL," +
                        "text_en TEXT NOT NULL," +
                        "text_ru_m TEXT NOT NULL," +
                        "text_ru_w TEXT," +
                        "translator_m TEXT NOT NULL," +
                        "translator_w TEXT," +
                        "source_text_ru_m TEXT NOT NULL," +
                        "source_text_ru_w TEXT," +
                        "source_transl_m TEXT NOT NULL," +
                        "source_transl_w TEXT" +
                        ")";
                    sqlite_cmd.CommandText = sql_create_table;
                    sqlite_cmd.ExecuteNonQuery();
                }
                sqlite_conn.Close();
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Загрузка строк авторского перевода";
            openFileDialog.Filter = "XML-File | *.xml";
            openFileDialog.FilterIndex = 1;
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string sql_insert = "";
                string sql_update = "";
                int num_edited_rows = 0;
                string key_import = "";
                string text_en_import = "";
                string text_ru_m_import = "";
                string text_ru_w_import = "";
                string source_m_import = "";
                string source_w_import = "";
                string translator_m_import = "";
                string translator_w_import = "";
                string source_transl_m_import = "";
                string source_transl_w_import = "";
                author_edit_start = true;

                source_list_m.Clear();
                source_list_w.Clear();
                source_list_m_author.Clear();
                source_list_w_author.Clear();
                data_trans_file.Rows.Clear();

                data_trans_file.Columns["source_text_ru_m"].Visible = true;
                data_trans_file.Columns["source_text_ru_w"].Visible = true;
                data_trans_file.Columns["source_transl_m"].Visible = true;
                data_trans_file.Columns["source_transl_w"].Visible = true;

                int lineCount = File.ReadLines(openFileDialog.FileName).Count();
                loading_text.Invoke((MethodInvoker)(() => loading_text.Parent = progressBar_text));
                loading_text.Invoke((MethodInvoker)(() => loading_text.BackColor = Color.Transparent));
                loading_text.Invoke((MethodInvoker)(() => loading_text.Visible = true));
                progressBar_text.Invoke((MethodInvoker)(() => progressBar_text.Visible = true));
                progressBar_text.Invoke((MethodInvoker)(() => progressBar_text.Value = 0));
                progressBar_text.Invoke((MethodInvoker)(() => progressBar_text.Maximum = lineCount - 2));

                XmlDocument xDoc1 = new XmlDocument();
                xDoc1.Load(openFileDialog.FileName);
                XmlElement xRoot1 = xDoc1.DocumentElement;
                int jks = 1;
                if (xRoot1.Name.ToString() == "rezult_author")
                {
                    //сначало загружаем строки в БД
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
                        if (childnode.Name == "source_m")
                        {
                            source_m_import = WebUtility.HtmlDecode(childnode.InnerText);
                            source_transl_m_import = WebUtility.HtmlDecode(childnode.Attributes.GetNamedItem("transl").Value);
                        }
                        if (childnode.Name == "source_w")
                        {
                            source_w_import = WebUtility.HtmlDecode(childnode.InnerText);
                            source_transl_w_import = WebUtility.HtmlDecode(childnode.Attributes.GetNamedItem("transl").Value);
                        }
                        if (jks % 6 == 0)
                        {
                            using (SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; "))
                            {
                                sqlite_conn.Open();

                                if (text_ru_w_import == "") //
                                {
                                    sql_update = "UPDATE author_edit SET text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "', text_ru_w=NULL, translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "', translator_w=NULL, source_text_ru_m='" + WebUtility.HtmlEncode(source_m_import) + "', source_text_ru_w='" + WebUtility.HtmlEncode(source_w_import) + "', source_transl_m='" + WebUtility.HtmlEncode(source_transl_m_import) + "', source_transl_w='" + WebUtility.HtmlEncode(source_transl_w_import) + "' WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "' AND text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "'";
                                    sql_insert = "INSERT INTO author_edit(id,key_unic,text_en,text_ru_m,text_ru_w,translator_m,translator_w,source_text_ru_m,source_text_ru_w,source_transl_m,source_transl_w) VALUES ('0','" + WebUtility.HtmlEncode(key_import) + "','" + WebUtility.HtmlEncode(text_en_import) + "','" + WebUtility.HtmlEncode(text_ru_m_import) + "',NULL,'" + WebUtility.HtmlEncode(translator_m_import) + "',NULL,'" + WebUtility.HtmlEncode(source_m_import) + "','" + WebUtility.HtmlEncode(source_w_import) + "','" + WebUtility.HtmlEncode(source_transl_m_import) + "','" + WebUtility.HtmlEncode(source_transl_w_import) + "')";
                                }
                                else if (source_w_import == "") //
                                {
                                    sql_update = "UPDATE author_edit SET text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "', text_ru_w='" + WebUtility.HtmlEncode(text_ru_w_import) + "', translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "', translator_w='" + WebUtility.HtmlEncode(translator_w_import) + "', source_text_ru_m='" + WebUtility.HtmlEncode(source_m_import) + "', source_text_ru_w=NULL, source_transl_m='" + WebUtility.HtmlEncode(source_transl_m_import) + "', source_transl_w=NULL WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "' AND text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "'";
                                    sql_insert = "INSERT INTO author_edit(id,key_unic,text_en,text_ru_m,text_ru_w,translator_m,translator_w,source_text_ru_m,source_text_ru_w,source_transl_m,source_transl_w) VALUES ('0','" + WebUtility.HtmlEncode(key_import) + "','" + WebUtility.HtmlEncode(text_en_import) + "','" + WebUtility.HtmlEncode(text_ru_m_import) + "','" + WebUtility.HtmlEncode(text_ru_w_import) + "','" + WebUtility.HtmlEncode(translator_m_import) + "','" + WebUtility.HtmlEncode(translator_w_import) + "','" + WebUtility.HtmlEncode(source_m_import) + "',NULL,'" + WebUtility.HtmlEncode(source_transl_m_import) + "',NULL)";
                                }
                                else if (text_ru_w_import == "" && source_w_import == "") //
                                {
                                    sql_update = "UPDATE author_edit SET text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "', text_ru_w=NULL, translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "', translator_w=NULL, source_text_ru_m='" + WebUtility.HtmlEncode(source_m_import) + "', source_text_ru_w=NULL, source_transl_m='" + WebUtility.HtmlEncode(source_transl_m_import) + "', source_transl_w=NULL WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "' AND text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "'";
                                    sql_insert = "INSERT INTO author_edit(id,key_unic,text_en,text_ru_m,text_ru_w,translator_m,translator_w,source_text_ru_m,source_text_ru_w,source_transl_m,source_transl_w) VALUES ('0','" + WebUtility.HtmlEncode(key_import) + "','" + WebUtility.HtmlEncode(text_en_import) + "','" + WebUtility.HtmlEncode(text_ru_m_import) + "',NULL,'" + WebUtility.HtmlEncode(translator_m_import) + "',NULL,'" + WebUtility.HtmlEncode(source_m_import) + "',NULL,'" + WebUtility.HtmlEncode(source_transl_m_import) + "',NULL)";
                                }
                                else
                                {
                                    sql_update = "UPDATE author_edit SET text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "', text_ru_w='" + WebUtility.HtmlEncode(text_ru_w_import) + "', translator_m='" + WebUtility.HtmlEncode(translator_m_import) + "', translator_w='" + WebUtility.HtmlEncode(translator_w_import) + "', source_text_ru_m='" + WebUtility.HtmlEncode(source_m_import) + "', source_text_ru_w='" + WebUtility.HtmlEncode(source_w_import) + "', source_transl_m='" + WebUtility.HtmlEncode(source_transl_m_import) + "', source_transl_w='" + WebUtility.HtmlEncode(source_transl_w_import) + "' WHERE key_unic ='" + WebUtility.HtmlEncode(key_import) + "' AND text_ru_m='" + WebUtility.HtmlEncode(text_ru_m_import) + "'";
                                    sql_insert = "INSERT INTO author_edit(id,key_unic,text_en,text_ru_m,text_ru_w,translator_m,translator_w,source_text_ru_m,source_text_ru_w,source_transl_m,source_transl_w) VALUES ('0','" + WebUtility.HtmlEncode(key_import) + "','" + WebUtility.HtmlEncode(text_en_import) + "','" + WebUtility.HtmlEncode(text_ru_m_import) + "','" + WebUtility.HtmlEncode(text_ru_w_import) + "','" + WebUtility.HtmlEncode(translator_m_import) + "','" + WebUtility.HtmlEncode(translator_w_import) + "','" + WebUtility.HtmlEncode(source_m_import) + "','" + WebUtility.HtmlEncode(source_w_import) + "','" + WebUtility.HtmlEncode(source_transl_m_import) + "','" + WebUtility.HtmlEncode(source_transl_w_import) + "')";
                                }
                                SQLiteCommand update = new SQLiteCommand(sql_update, sqlite_conn);
                                int numRowsUpdated = update.ExecuteNonQuery();
                                if (numRowsUpdated == 0)
                                {
                                    SQLiteCommand insert = new SQLiteCommand(sql_insert, sqlite_conn);
                                    insert.ExecuteNonQuery();
                                }
                                sqlite_conn.Close();
                            }
                            num_edited_rows++;
                            progressBar_text.Invoke((MethodInvoker)(() => progressBar_text.Value += 1));
                        }
                        jks++;
                    }

                    //затем показываем строки во вьюгриде
                    //СДЕЛАТЬ ПАГИНАЦИЮ

                    using (SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; "))
                    {
                        sqlite_conn.Open();
                        using (SQLiteCommand sqlite_cmd = new SQLiteCommand(sqlite_conn))
                        {
                            sql_insert = "SELECT key_unic,text_en,text_ru_m,text_ru_w,translator_m,translator_w,source_text_ru_m,source_text_ru_w,source_transl_m,source_transl_w FROM author_edit GROUP BY key_unic ORDER by ID";
                            
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
                                data_trans_file.Rows[rowNumber].Cells["source_text_ru_m"].Value = WebUtility.HtmlDecode(r["source_text_ru_m"].ToString());
                                data_trans_file.Rows[rowNumber].Cells["source_text_ru_w"].Value = WebUtility.HtmlDecode(r["source_text_ru_w"].ToString());
                                data_trans_file.Rows[rowNumber].Cells["source_transl_m"].Value = WebUtility.HtmlDecode(r["source_transl_m"].ToString());
                                data_trans_file.Rows[rowNumber].Cells["source_transl_w"].Value = WebUtility.HtmlDecode(r["source_transl_w"].ToString());
                                data_trans_file.Rows[rowNumber].Cells["filesinfo"].Value = "0";
                                data_trans_file.Rows[rowNumber].Cells["filesinfo_w"].Value = "0";
                                if (searching_row_id == WebUtility.HtmlDecode(r["key_unic"].ToString()))
                                {
                                    data_trans_file.Rows[0].Selected = false;
                                    data_trans_file.Rows[rowNumber].Selected = true;
                                }
                                progressBar_text.Value += 1;
                            }
                            r.Close();
                        }
                        sqlite_conn.Close();
                    }
                    loading_text.Visible = false;
                    progressBar_text.Visible = false;
                } else
                {
                    MessageBox.Show("Файл не соответсвует шаблону, пожалуйста убедитесь, что загружаете файл полученный из письма", "Ошибка загрузки", MessageBoxButtons.OK);
                    data_trans_file.Columns["source_text_ru_m"].Visible = false;
                    data_trans_file.Columns["source_text_ru_w"].Visible = false;
                    data_trans_file.Columns["source_transl_m"].Visible = false;
                    data_trans_file.Columns["source_transl_w"].Visible = false;
                }
                progressBar_text.Invoke((MethodInvoker)(() => progressBar_text.Visible = false));
                loading_text.Invoke((MethodInvoker)(() => loading_text.Visible = false));
                MessageBox.Show("Выгружено в БД: " + num_edited_rows + " строк");
            }
        }
        private void export_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists("export_translation"))
            {
                Directory.CreateDirectory("export_translation");
            }

            //формируем список пользователей
            string join = String.Join("' , '", data_name.Cast<string>()
                                 .Where(c => !string.IsNullOrWhiteSpace(c))
                                 .Distinct());

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Title = "Экспорт строк перевода";
            saveFileDialog1.Filter = "XML-File | *.xml";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory() + "//export_translation";
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
                    using (StreamWriter file_for_exam = new StreamWriter(saveFileDialog1.FileName, true, encoding: Encoding.UTF8))
                    {
                        file_for_exam.WriteLine(xml_text);
                    }
                }
                using (StreamWriter file_for_exam = new StreamWriter(saveFileDialog1.FileName, true, encoding: Encoding.UTF8))
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
