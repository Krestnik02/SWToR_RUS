using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Configuration;

namespace SWToR_RUS
{
    public partial class Form3 : Form
    {
        public string connStr_mysql = "server=" + "195.234.5.250" + //Адрес сервера (для локальной базы пишите "localhost")
                    ";user=" + "swtor" + //Имя пользователя
                    ";database=" + "swtor_ru" + //Имя базы данных
                    ";port=" + "3306" + //Порт для подключения
                    ";password=" + "KHUS86!JHksds" + //Пароль для подключения
                    ";default command timeout=0;";

        public int appClosing;

        public Form3()
        {
            InitializeComponent();

            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            name.Text = configuration.AppSettings.Settings["author"].Value.ToString();
            email.Text = configuration.AppSettings.Settings["email"].Value.ToString();
            password.Text = configuration.AppSettings.Settings["password"].Value.ToString();
            if (configuration.AppSettings.Settings["translate_restrict"].Value.ToString() == "1")
                Translate_Restrict.Checked = true;
            Check_auth();
        }

        public static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }
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

        bool IsValid(string email)
        {
            string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
            Match isMatch = Regex.Match(email, pattern, RegexOptions.IgnoreCase);
            return isMatch.Success;
        }

        public void Auth_Click(object sender, EventArgs e)
        {
            string email_box = email.Text, name_box = name.Text, password_box = password.Text;
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string tr_restrict = "0";
            if (Translate_Restrict.Checked == true)
                tr_restrict = "1";
            if (IsValid(email_box))
            {
                if(name_box != "Deepl" && name_box != "deepl") {
                    using (MySqlConnection conn = new MySqlConnection(connStr_mysql))
                    {
                        conn.Open();
                        if (email_box != "" && name_box != "" && password_box != "")
                        {
                            string sql_select = "SELECT id, email, name, pass,status FROM users WHERE email='" + email_box + "' AND name = '" + name_box + "';";
                            MySqlCommand command = new MySqlCommand(sql_select, conn);
                            MySqlDataReader row = command.ExecuteReader();
                            if (row.HasRows)
                            {
                                while (row.Read())
                                {
                                    if (VerifyHashedPassword(row["pass"].ToString(), password_box))
                                    {
                                        if (row["status"].ToString() != tr_restrict)
                                        {
                                            string sql_update = "UPDATE users SET status='" + tr_restrict + "' WHERE id='" + row["id"].ToString() + "'";
                                            MySqlCommand update = new MySqlCommand(sql_update, conn);
                                            update.ExecuteNonQuery();
                                        }
                                        appClosing = 10;
                                        configuration.AppSettings.Settings["author"].Value = name_box;
                                        configuration.AppSettings.Settings["email"].Value = email_box;
                                        configuration.AppSettings.Settings["password"].Value = password_box;
                                        configuration.AppSettings.Settings["translate_restrict"].Value = tr_restrict;
                                        configuration.Save(ConfigurationSaveMode.Modified);
                                        ConfigurationManager.RefreshSection("appSettings");//Обновляем конфиг для приложения
                                        MessageBox.Show("Вы успешно авторизованы!", "Авторизация", MessageBoxButtons.OK);
                                        Close();
                                    } else
                                    {
                                        MessageBox.Show("Введен неверный пароль", "Авторизация", MessageBoxButtons.OK);
                                    }
                                }
                                row.Close();
                            }
                            else
                            {
                                row.Close();
                                string sql_select2 = "SELECT email FROM users WHERE email='" + email_box + "';";
                                MySqlCommand command2 = new MySqlCommand(sql_select2, conn);
                                MySqlDataReader row2 = command2.ExecuteReader();
                                if (row2.HasRows)
                                {
                                    MessageBox.Show("Пользователь с такой почтой уже зарегистрирован!", "Регистрация", MessageBoxButtons.OK);
                                    email.Clear();
                                    row2.Close();
                                } else
                                {
                                    row2.Close();
                                    DialogResult dialogResult = MessageBox.Show("Пользователь не найден, зарегистрироваться?", "Регистрация", MessageBoxButtons.YesNo);
                                    if (dialogResult == DialogResult.Yes)
                                    {
                                        string sql_insert = "INSERT INTO users(id,name,pass,email,status) VALUES ('0','" + name_box + "','" + HashPassword(password_box) + "','" + email_box + "','" + tr_restrict + "')";
                                        MySqlCommand insert = new MySqlCommand(sql_insert, conn);
                                        insert.ExecuteNonQuery();
                                        configuration.AppSettings.Settings["author"].Value = name_box;
                                        configuration.AppSettings.Settings["email"].Value = email_box;
                                        configuration.AppSettings.Settings["password"].Value = password_box;
                                        configuration.AppSettings.Settings["translate_restrict"].Value = tr_restrict;
                                        configuration.Save(ConfigurationSaveMode.Modified);
                                        ConfigurationManager.RefreshSection("appSettings");//Обновляем конфиг для приложения
                                        MessageBox.Show("Вы успешно зарегистрированны!", "Авторизация", MessageBoxButtons.OK);
                                        appClosing = 10;
                                        Close();
                                    }
                                }
                            }
                        }
                        conn.Close();
                    }
                } else
                {
                    MessageBox.Show("Вы не можете авторизоваться под этим именем!", "Авторизация", MessageBoxButtons.OK);
                    if (name.Enabled == false)
                        name.Enabled = true;
                }
            } else
            {
                MessageBox.Show("Проверьте правильность написания E-mail!", "Авторизация", MessageBoxButtons.OK);
            }
            
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (appClosing == 10)
            {
                Data.Trigger = 2;
                Data.Value = name.Text;
                Data.Title = "Редактор - Авторизирован: " + name.Text;
            } else
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    if (name.Text != "" || email.Text != "" || password.Text != "")
                    {
                        DialogResult dialogResult = MessageBox.Show("Закрывая окно вы потеряете внесенные изменения, закрыть окно?", "Авторизация", MessageBoxButtons.OKCancel);
                        if (dialogResult == DialogResult.OK)
                        {
                            Data.Trigger = 3;
                        }
                        else
                        {
                            e.Cancel = true;
                        }
                    }
                    else
                    {
                        Data.Trigger = 3;
                    }
                }
            }
            base.OnFormClosing(e);
        }

        private void Email_TextChanged(object sender, EventArgs e)
        {
            Check_auth();
        }
        private void Check_auth()
        {
            if (name.Text != "Deepl" && name.Text != "deepl" && name.Text != "" && email.Text != "" && password.Text != "")
            {
                auth.Enabled = true;
                Translate_Restrict.Enabled = true;
            }
            else
            {
                auth.Enabled = false;
                Translate_Restrict.Enabled = false;
            }
        }

        private void Password_TextChanged(object sender, EventArgs e)
        {
            Check_auth();
        }

        private void Name_TextChanged(object sender, EventArgs e)
        {
            Check_auth();
        }
    }
}
