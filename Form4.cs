using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Net;

namespace SWToR_RUS
{
    public partial class Form4 : Form
    {
        public Configuration Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None); //Доступ к конфигурации
        public Form4()
        {
            InitializeComponent();
        }

        private void Form4_Load(object sender, EventArgs e)
        {

            using (SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; "))
            {
                using (SQLiteCommand sqlite_cmd = new SQLiteCommand(sqlite_conn))
                {
                    sqlite_conn.Open();
                    string sqllite_select = "SELECT translator_m, COUNT(translator_m) AS count_m FROM Translated WHERE translator_m != 'Deepl' GROUP BY translator_m  ORDER BY COUNT(translator_m) DESC";
                    sqlite_cmd.CommandText = sqllite_select;
                    SQLiteDataReader r = sqlite_cmd.ExecuteReader();
                    while (r.Read())
                    {
                        int rowNumber = data_bans.Rows.Add();
                        data_bans.Rows[rowNumber].Cells["translator"].Value = WebUtility.HtmlDecode(r["translator_m"].ToString());
                        data_bans.Rows[rowNumber].Cells["count"].Value = WebUtility.HtmlDecode(r["count_m"].ToString());
                    }
                    r.Close();
                    sqlite_cmd.Dispose();
                    sqlite_conn.Close();
                }
            }

            if (Config.AppSettings.Settings["banlist"].Value != "0" && Config.AppSettings.Settings["banlist"].Value != "")
            {
                string list = Config.AppSettings.Settings["banlist"].Value.ToString();

                List<string> current_list_bans = list.Split(',').ToList();

                foreach (DataGridViewRow Row in data_bans.Rows)
                {
                    foreach (var value in current_list_bans)
                    {
                        if(value == WebUtility.HtmlEncode(Row.Cells["translator"].Value.ToString()))
                        {
                            Row.Cells["checker"].Value = "T";
                        }
                    }
                }
            }
        }

        private void ok_Click(object sender, EventArgs e)
        {
            List<string> list_bans = new List<string>();

            foreach (DataGridViewRow Row in data_bans.Rows)
            {
                if (Row.Cells["checker"].Value != null && Row.Cells["checker"].Value.ToString() == "T")
                {
                    list_bans.Add(WebUtility.HtmlEncode(Row.Cells["translator"].Value.ToString()));
                }
            }

            var result = String.Join(",", list_bans.ToArray());
            
            Config.AppSettings.Settings["banlist"].Value = result;
            Config.Save(ConfigurationSaveMode.Modified);
            list_bans.Clear();

            Close();
        }
    }
}