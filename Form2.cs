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

        public string buffer = "";

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
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (ConfigurationManager.AppSettings["author"] != null)
            {
                new_author.Text = configuration.AppSettings.Settings["author"].Value;

                if (ConfigurationManager.AppSettings["email"] != null)
                {
                    using (MySqlConnection conn = new MySqlConnection(connStr_mysql))
                    {
                        conn.Open();
                        string sql = "SELECT id, email, name, pass FROM users WHERE email='" + configuration.AppSettings.Settings["email"].Value + "'";
                        MySqlCommand command = new MySqlCommand(sql, conn);
                        MySqlDataReader row = command.ExecuteReader();
                        if (row.HasRows)
                        {
                            while (row.Read())
                            {
                                if (row["pass"].ToString() == configuration.AppSettings.Settings["password"].Value)
                                {
                                    new_author.Enabled = false;
                                    auth.Enabled = false;
                                    authorization = true;
                                }
                            }
                            row.Close();
                        }
                        conn.Close();
                    }
                }
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
                        data_trans_file.Rows.Clear();
                        List<string> list_translators = new List<string>();
                        SQLiteConnection sqlite_conn;
                        sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; ");
                        sqlite_conn.Open();
                        int count_pgs = 0;
                        if (checkBox1.Checked)
                            sql_insert_part2 = " AND ((translator_m='Deepl' AND translator_w='Deepl') OR (translator_m='Deepl' AND translator_w is NULL) OR (translator_w='Deepl' AND translator_m is NULL))";
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
                export.Enabled = true;
            }
            else
                export.Enabled = false;
            if (fileinfo_user.Text != "")
                fileinfo_user.Enabled = true;

        }
        public void data_trans_file_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (buffer != Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[e.ColumnIndex].Value))
            {
                if (e.ColumnIndex == 2)
                {
                    data_trans_file.Rows[e.RowIndex].Cells["filesinfo"].Value = "1";
                }
                if (e.ColumnIndex == 3)
                {
                    data_trans_file.Rows[e.RowIndex].Cells["filesinfo_w"].Value = "1";
                }
            }
            
            fileinfo_user.Enabled = true;
            if (fileinfo_user.Text != "")
                upload_translate.Enabled = true;
        }

        public void data_trans_file_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if ((e.ColumnIndex == 2 || e.ColumnIndex == 3) && data_trans_file.Rows[e.RowIndex].Cells[4].Value.ToString() != "Deepl")
            {
                buffer = Convert.ToString(data_trans_file.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
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
            if (fileinfo_user.Text != "")
                upload_translate.Enabled = true;
            else
                upload_translate.Enabled = false;
        }

        private async void upload_translate_Click(object sender, EventArgs e)
        {
            await Task.Run(() => uploading());
        }
        public void uploading()
        {
            string new_authr = "5";
            string xml_text = "";
            List<string> list_keys = new List<string>();
            if (new_author.Text != "" && new_author.Text != "Напишите своё имя или оставьте как есть")
                new_authr = new_author.Text;
            if (new_authr != "5" && new_authr != "4" && new_authr != "deepl" && new_authr != "1" && new_authr != "2")
            {
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                configuration.AppSettings.Settings["author"].Value = new_authr;
                configuration.Save(ConfigurationSaveMode.Modified);
            }
            
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
                            row.Cells["filesinfo"].Value = "0";
                            row.Cells["filesinfo_w"].Value = "0";
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
                MessageBox.Show("Файл сохранён в папке user_translation! Выгружено " + num_edited_rows + " строк.");
            else
                MessageBox.Show("Выгружать нечего!");
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
                    data_trans_file.Rows.Clear();
                    string sql_insert_part3 = "";
                    string sql_insert_part2 = "";
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
                                }
                                else
                                {
                                    if (search_ru.Checked == true)
                                        sql_insert_part2 = "(text_ru_m like '%" + WebUtility.HtmlEncode(searchbox.Text) + "%' OR text_ru_w like '%" + WebUtility.HtmlEncode(searchbox.Text) + "%')";
                                    if (search_en.Checked == true)
                                        sql_insert_part2 = "text_en like '%" + WebUtility.HtmlEncode(searchbox.Text) + "%'";
                                }
                                if (checkBox1.Checked == true)
                                    sql_insert_part2 += " AND ((translator_m='Deepl' AND translator_w='Deepl') OR (translator_m='Deepl' AND translator_w is NULL) OR (translator_w='Deepl' AND translator_m is NULL))";
                            }
                            else if (checkBox1.Checked==true)
                                sql_insert_part2 = " AND ((translator_m='Deepl' AND translator_w='Deepl') OR (translator_m='Deepl' AND translator_w is NULL) OR (translator_w='Deepl' AND translator_m is NULL))";
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
            data_trans_file.Rows.Clear();
            string sql_insert;
            SQLiteConnection sqlite_conn;
            sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; ");
            sqlite_conn.Open();
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqlite_conn.CreateCommand();
            if (checkBox1.Checked)
                sql_insert = "SELECT fileinfo FROM Translated WHERE (translator_m='Deepl' AND translator_w='Deepl') OR (translator_m='Deepl' AND translator_w is NULL) OR (translator_w='Deepl' AND translator_m is NULL) GROUP by fileinfo;";
            else
                sql_insert = "SELECT fileinfo FROM Translated GROUP by fileinfo";
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
                        if (checkBox1.Checked == true)
                            sql_select = "SELECT key_unic,hash FROM Translated WHERE fileinfo='" + select_file + "' AND((translator_m = 'Deepl' AND translator_w = 'Deepl') OR(translator_m = 'Deepl' AND translator_w is NULL) OR(translator_w = 'Deepl' AND translator_m is NULL)) GROUP BY text_en ORDER by id";
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

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form ifrm = Application.OpenForms[0];
            ifrm.Show();
        }

        private void searchbox_TextChanged(object sender, EventArgs e)
        {
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

        private void export_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists("export"))
            {
                Directory.CreateDirectory("export");
            }

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
                string sql_insert = "SELECT key_unic,text_en,text_ru_m,text_ru_w,translator_m,translator_w FROM Translated WHERE fileinfo='" + file_to_trans.Text + "' ORDER BY ID";
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
            
            Form3 form3 = new Form3(new MyDelegate(GetData));
            form3.Show();
        }
        void GetData(string param)
        {
            new_author.Enabled = false;
            auth.Enabled = false;
            new_author.Text = param;
        }
    }
}
