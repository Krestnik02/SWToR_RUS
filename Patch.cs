using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System.Configuration;
using System.IO;
using System.Net;
using System.Data.SQLite;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System.Threading;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using System.Windows.Forms;

namespace SWToR_RUS
{
    internal class Patch
    {
        public IWebDriver driver;

        public int endtable;

        public int lastoffes;

        public int cou;

        public uint filescount;

        public ulong vOut;

        uint hash_g;

        public string transl_a;

        public Dictionary<ulong, string> dictionary_en = new Dictionary<ulong, string>();

        public Dictionary<ulong, string> dictionary_xml_m = new Dictionary<ulong, string>();

        public Dictionary<ulong, string> dictionary_xml_translator_m = new Dictionary<ulong, string>();

        public Dictionary<ulong, string> dictionary_xml_w = new Dictionary<ulong, string>();

        public Dictionary<ulong, string> dictionary_xml_translator_w = new Dictionary<ulong, string>();

        public int Deepl_First_time = 1;

        public List<string> File_Name_List = new List<string>();

        public string changes;

        public ProgressBar ProgressBar_F1 = Application.OpenForms["Form1"].Controls["ProgressBar1"] as ProgressBar;

        public void ConnectDB()
        {
            byte[] array = File.ReadAllBytes("db\\fonts.gfx");
            string a = "0";
            string a2 = "0";
            string dis_items = "0";
            string non_dialoge="0";
            int z = 290505;
            int f = 465311;
            List<uint> list = new List<uint>();
            if (ConfigurationManager.AppSettings["sith"] != null)
                a = ConfigurationManager.AppSettings["sith"];
            if (ConfigurationManager.AppSettings["skill"] != null)
                a2 = ConfigurationManager.AppSettings["skill"];
            if (ConfigurationManager.AppSettings["items"] != null)
                dis_items = ConfigurationManager.AppSettings["items"];
            if (ConfigurationManager.AppSettings["non_dialoge"] != null)
                non_dialoge = ConfigurationManager.AppSettings["non_dialoge"];
            string text_en;
            string text_ru_m;
            string text_ru_w;
            string translator_m;
            string translator_w;
            string sql_insert;
            SQLiteConnection sqlite_conn;
            sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True;");
            sqlite_conn.Open();
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqlite_conn.CreateCommand();
            sql_insert = "SELECT hash,fileinfo,key_unic,text_ru_m,text_ru_w,translator_m,translator_w FROM Translated WHERE ";
            if (ConfigurationManager.AppSettings["google"] != "1")
            {
                if(a2 == "2" && dis_items == "1")
                    sql_insert += "fileinfo!='itm.stb' AND fileinfo!='abl.stb' AND fileinfo!='tal.stb' AND fileinfo!='gui/amplifiers.stb' AND fileinfo!='gui/abl/player/skill_trees.stb' AND (translator_m!='Deepl' OR translator_w!='Deepl')";
                else if (a2 == "2")
                    sql_insert += "fileinfo!='abl.stb' AND fileinfo!='tal.stb' AND fileinfo!='gui/amplifiers.stb' AND fileinfo!='gui/abl/player/skill_trees.stb' AND (translator_m!='Deepl' OR translator_w!='Deepl')";
                else if (dis_items == "1")
                    sql_insert += "fileinfo!='itm.stb' AND (translator_m!='Deepl' OR translator_w!='Deepl')";
                else if (non_dialoge=="1")
                    sql_insert += "fileinfo like 'cnv%' AND (translator_m!='Deepl' OR translator_w!='Deepl')";
                else
                    sql_insert += "translator_m!='Deepl' OR translator_w!='Deepl'";
            }
            else
            {
                if (a2 == "2" && dis_items == "1")
                    sql_insert += "fileinfo!='itm.stb' AND fileinfo!='abl.stb' AND fileinfo!='tal.stb' AND fileinfo!='gui/amplifiers.stb' AND fileinfo!='gui/abl/player/skill_trees.stb'";
                else if (a2 == "2")
                    sql_insert += "fileinfo!='abl.stb' AND fileinfo!='tal.stb' AND fileinfo!='gui/amplifiers.stb' AND fileinfo!='gui/abl/player/skill_trees.stb'";
                else if (dis_items == "1")
                    sql_insert += "fileinfo!='itm.stb'";
                else if (non_dialoge == "1")
                    sql_insert += "fileinfo like 'cnv%'";
                else
                    sql_insert = "SELECT hash,fileinfo,key_unic,text_en,text_ru_m,text_ru_w,translator_m,translator_w FROM Translated";
            }
            sqlite_cmd.CommandText = sql_insert;
            SQLiteDataReader r = sqlite_cmd.ExecuteReader();
            while (r.Read())
            {
                vOut = UInt64.Parse(r["key_unic"].ToString());
                hash_g = UInt32.Parse(r["hash"].ToString());
                translator_m = r["translator_m"].ToString();
                translator_w = r["translator_w"].ToString();
                text_ru_m = WebUtility.HtmlDecode(r["text_ru_m"].ToString()).Replace("ё", "е").Replace("Ё", "Е");
                text_ru_w = WebUtility.HtmlDecode(r["text_ru_w"].ToString()).Replace("ё", "е").Replace("Ё", "Е");
                if (text_ru_w == "")
                    text_ru_w = text_ru_m;
                if (a == "1")
                {
                    text_ru_m = Sith(text_ru_m);
                    text_ru_w = Sith(text_ru_w);
                }
                if (!dictionary_xml_m.ContainsKey(vOut))
                {
                    dictionary_xml_m.Add(vOut, text_ru_m);
                    dictionary_xml_translator_m.Add(vOut, translator_m);
                }
                if (!dictionary_xml_w.ContainsKey(vOut))
                {
                    dictionary_xml_w.Add(vOut, text_ru_w);
                    if (translator_w != "")
                        dictionary_xml_translator_w.Add(vOut, translator_w);
                }
                if (r["fileinfo"].ToString().IndexOf("\\") == -1 && !File_Name_List.Contains(r["fileinfo"].ToString()))
                    File_Name_List.Add(r["fileinfo"].ToString());
                if (ConfigurationManager.AppSettings["changes"] == "1")
                {
                    text_en = WebUtility.HtmlDecode(r["text_en"].ToString());
                    changes = "1";
                    if (!dictionary_en.ContainsKey(vOut))
                        dictionary_en.Add(vOut, text_en);
                }
                else
                    changes = "0";
                if (!list.Contains(hash_g))
                    list.Add(hash_g);
            }
            r.Close();
            sqlite_cmd.Dispose();
            sqlite_conn.Close();
            ProgressBar_F1.Invoke((MethodInvoker)(() => ProgressBar_F1.Maximum = list.Count));
            ProgressBar_F1.Invoke((MethodInvoker)(() => ProgressBar_F1.Value = 0));
            string str = ConfigurationManager.AppSettings["gamepath"];
            PatchMain(str + "\\Assets\\swtor_maln_global_1.tor", "maln_global_1.tor");
            PatchMain(str + "\\Assets\\swtor_ru-wm_global_1.tor", "ru-wm_global_1.tor");
            PatchMain(str + "\\Assets\\swtor_ru-ww_global_1.tor", "ru-ww_global_1.tor");
            Patchen(str + "\\Assets\\swtor_ru-wm_global_1.tor");
            Patchen(str + "\\Assets\\swtor_ru-ww_global_1.tor");
            cou = 0;
            if (ConfigurationManager.AppSettings["gender"] == "1")
            {
                PatchParse(str + "\\Assets\\swtor_ru-wm_global_1.tor", dictionary_xml_m, list, 1);
                cou = 0;
                PatchParse(str + "\\Assets\\swtor_ru-ww_global_1.tor", dictionary_xml_w, list, 0);
            }
            else
            {
                PatchParse(str + "\\Assets\\swtor_ru-ww_global_1.tor", dictionary_xml_w, list, 1);
                cou = 0;
                PatchParse(str + "\\Assets\\swtor_ru-wm_global_1.tor", dictionary_xml_m, list, 0);
            }
            if (Deepl_First_time == 0)
            {
                driver.Close();
                driver.Quit();
            }
            PatchGfx(str + "\\Assets\\swtor_maln_gfx_assets_1.tor", array, z, f);
        }

        public string Sith(string s)//Ситх или Сит
        {
            StringBuilder stringBuilder = new StringBuilder(s);
            stringBuilder.Replace("итхи", "иты");
            stringBuilder.Replace("Ситх", "Сит");
            stringBuilder.Replace("ситх", "сит");
            stringBuilder.Replace("СИТХ", "СИТ");
            return stringBuilder.ToString();
        }

        public void PatchGfx(string filename, byte[] dd, int z, int f)//Добавляет русский FONT
        {
            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
            int num = EndOff(fileStream);
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
            binaryWriter.Write(dd);
            binaryWriter.Close();
            fileStream.Close();
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

        public void Patchen(string filename)
        {
            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            BinaryWriter binaryWriter = new BinaryWriter(fileStream);
            uint num = (uint)fileStream.Length;
            int num2 = EndOff(fileStream);
            num2 -= 136;
            binaryReader.BaseStream.Position = num2;
            uint num3 = binaryReader.ReadUInt32();
            binaryReader.ReadUInt32();
            binaryReader.ReadUInt32();
            uint num4 = binaryReader.ReadUInt32();
            binaryReader.ReadUInt32();
            binaryReader.ReadUInt32();
            uint num5 = binaryReader.ReadUInt32();
            binaryReader.ReadUInt32();
            binaryReader.ReadUInt16();
            if (num5 == 3837354259u)
            {
                binaryReader.BaseStream.Seek(num3, SeekOrigin.Begin);
                byte[] buffer = binaryReader.ReadBytes((int)(num4 + 36));
                binaryWriter.BaseStream.Position = num;
                binaryWriter.Write(buffer);
                binaryReader.BaseStream.Position = num2;
                binaryWriter.Write(num);
                binaryWriter.Close();
            }
            binaryReader.Close();
            fileStream.Close();
        }

        public void PatchMain(string filename, string replace)//Подменяет ссылки оригинальных файлов на изменённые
        {
            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            int num = EndOff(fileStream);
            num -= 68;
            binaryReader.BaseStream.Position = num;
            uint num2 = binaryReader.ReadUInt32();
            binaryReader.ReadUInt32();
            binaryReader.ReadUInt32();
            binaryReader.ReadUInt32();
            uint count = binaryReader.ReadUInt32();
            binaryReader.ReadUInt32();
            uint num3 = binaryReader.ReadUInt32();
            binaryReader.ReadUInt32();
            binaryReader.ReadUInt16();
            if (num3 == 4034388578u)
            {
                binaryReader.BaseStream.Seek(num2 + 36, SeekOrigin.Begin);
                byte[] bytes = binaryReader.ReadBytes((int)count);
                string s = Encoding.UTF8.GetString(bytes).Replace("main_global_1.tor", replace).Replace("main_gfx_assets_1.tor", "maln_gfx_assets_1.tor")
                    .Replace("en-us_global_1.tor", replace);
                byte[] bytes2 = Encoding.UTF8.GetBytes(s);
                BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                binaryWriter.BaseStream.Position = num2 + 36;
                binaryWriter.Write(bytes2);
                binaryWriter.Close();
            }
            binaryReader.Close();
            fileStream.Close();
        }
        private ulong UniqueId(long left, short right)
        {
            return (ulong)((long)((ulong)((left + right) * (left + right + 1)) / 2uL) + left);
        }

        public void PatchParse(string filename, Dictionary<ulong, string> dbru, List<uint> fileidcount, int somework)
        {
            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
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
                    if (fileidcount.Contains(num5))
                    { //Console.WriteLine(num4+"---"+ zsize + "---" + size + "---" + hash + "---" + num5.ToString("X")+ "---"+ num5); 
                        PackText(fileStream, num4, zsize, size, dbru, hash, num5, endtable, somework);
                        if (ProgressBar_F1.Value + 1<=ProgressBar_F1.Maximum)
                            ProgressBar_F1.Invoke((MethodInvoker)(() => ProgressBar_F1.Value += 1));
                    }
                    else
                    {
                        try
                        {
                            PackText(fileStream, num4, zsize, size, dbru, hash, num5, endtable, somework);
                        }
                        catch
                        {

                        }
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
        }

      public void PackText(FileStream fileStream, uint off, uint zsize, uint size, Dictionary<ulong, string> dbd, uint hash2, uint hash1, int endtable,int somework)
        {
            BinaryReader binaryReader = new BinaryReader(fileStream);
            binaryReader.BaseStream.Position = off;
            binaryReader.ReadBytes(36);
            byte[] input = binaryReader.ReadBytes((int)zsize);
            byte[] buffer = new byte[size];
            Inflater inflater = new Inflater();
            inflater.SetInput(input);
            inflater.Inflate(buffer);

            BinaryReader binaryReader2 = new BinaryReader(new MemoryStream(buffer));
            binaryReader2.ReadBytes(3);
            int num = binaryReader2.ReadInt32();
            int num2 = num * 26 + 7;
            Dictionary<ulong, string> dictionary = new Dictionary<ulong, string>();
            int num3 = 0;
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write((byte)1);
            binaryWriter.Write((byte)0);
            binaryWriter.Write((byte)0);
            binaryWriter.Write(num);
            while (num3 < num)
            {
                long num4 = binaryReader2.ReadInt64();
                byte b = binaryReader2.ReadByte();
                byte value = binaryReader2.ReadByte();
                float value2 = binaryReader2.ReadSingle();
                int num5 = binaryReader2.ReadInt32();
                int num6 = binaryReader2.ReadInt32();
                binaryReader2.ReadInt32();
                long position = binaryReader2.BaseStream.Position;
                string change_value = "0";
                if (num5 > 0)
                {
                    binaryReader2.BaseStream.Seek(num6, SeekOrigin.Begin);
                    byte[] bytes = binaryReader2.ReadBytes(num5);
                    string text = Encoding.UTF8.GetString(bytes).Replace("\n", "\\n");
                    ulong key = UniqueId(num4, b);
                    if (changes == "1")
                    {
                        if (dictionary_en.TryGetValue(key, out string _))
                        {
                            if (dictionary_en[key] != text)
                            {
                                dbd.Remove(key);
                                dictionary_xml_m.Remove(key);
                                dictionary_xml_w.Remove(key);
                                dictionary_xml_translator_m.Remove(key);
                                dictionary_xml_translator_w.Remove(key);
                                change_value = "1";
                            }
                            else
                                change_value = "0";
                        }
                    }
                    if (dbd.TryGetValue(key, out string _))
                    {
                        dictionary.Add(key, dbd[key]);                       
                        _ = dbd[key];
                        num5 = Encoding.UTF8.GetBytes(dbd[key].Replace("\\n", "\n")).Length;
                    }
                    else
                    {
                        transl_a = "";
                        if (ConfigurationManager.AppSettings["a_translate"] == "1" && somework == 1)
                        {
                            if (File.Exists(@"C:\\Program Files\\Mozilla Firefox\\firefox.exe") || File.Exists(@"C:\\Program Files (x86)\\Mozilla Firefox\\firefox.exe"))
                            {
                                
                                transl_a = Translator(text, "Deepl");
                                /*string instrrr = "INSERT INTO Translated (fileinfo,hash,key_unic,text_en,text_ru_m,translator_m) VALUES ('" + hash1 + "','" + hash1 + "','" + key + "','" + WebUtility.HtmlEncode(text) + "','" + WebUtility.HtmlEncode(transl_a) + "','Deepl');";
                                using (StreamWriter file_for_exam = new StreamWriter("db\\deepl_trans.txt", true))
                                {
                                    file_for_exam.WriteLine(instrrr);
                                }
                                transl_a = "";*/
                            }
                            else
                                transl_a = Translator(text, "Promt");
                        }                        
                        if (transl_a == "")
                        {
                            dictionary.Add(key, text);
                            dictionary_xml_m.Add(key, text);
                            dictionary_xml_w.Add(key, text);
                            dictionary_xml_translator_m.Add(key, "Deepl");
                            dictionary_xml_translator_w.Add(key, "Deepl");
                            num5 = Encoding.UTF8.GetBytes(text.Replace("\\n", "\n")).Length;
                        }
                        else
                        {
                            dictionary.Add(key, transl_a);
                            dictionary_xml_m.Add(key, transl_a);
                            dictionary_xml_w.Add(key, transl_a);
                            dictionary_xml_translator_m.Add(key, "Deepl");
                            dictionary_xml_translator_w.Add(key, "Deepl");
                            num5 = Encoding.UTF8.GetBytes(transl_a.Replace("\\n", "\n")).Length;
                            using (SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; "))
                            {
                                sqlite_conn.Open();
                                using (SQLiteCommand sqlite_cmd = new SQLiteCommand(sqlite_conn))
                                {
                                    if (change_value == "1")
                                        sqlite_cmd.CommandText = "UPDATE Translated SET text_en='" + WebUtility.HtmlEncode(text) + "', text_ru_m='" + WebUtility.HtmlEncode(transl_a) + "',translator_m='Deepl',text_ru_w=NULL,translator_w=NULL WHERE key_unic ='" + key + "'";
                                    else
                                        sqlite_cmd.CommandText = "INSERT INTO Translated (fileinfo,hash,key_unic,text_en,text_ru_m,translator_m) VALUES ('" + hash1 + "','" + hash1 + "','" + key + "','" + WebUtility.HtmlEncode(text) + "','" + WebUtility.HtmlEncode(transl_a) + "','Deepl')";
                                    sqlite_cmd.ExecuteNonQuery();
                                }
                            }

                        }
                        /*string xml_text = "<hash>" + hash1 + "</hash><key>" + key + "</key><text_en>" + WebUtility.HtmlEncode(text) + "</text_en>";
                        using (StreamWriter file_for_exam =
                        new StreamWriter("db\\new_eng.txt", true))
                        {
                            file_for_exam.WriteLine(xml_text);
                        }
                        
                        string xml_text = "<filesinfo></filesinfo><hash>" + hash1 + "</hash><key>" + key + "</key><text_en>" + WebUtility.HtmlEncode(text) + "</text_en><text_ru_m transl=\"3\">" + WebUtility.HtmlEncode(dbd[key]) + "</text_ru_m><text_ru_w  transl=\"3\"></text_ru_w>";
                                using (StreamWriter file_for_exam =
                                new StreamWriter(@"C:\Users\Tidus\source\repos\SWToR_RUS\bin\Debug\db\all2.txt", true, encoding: Encoding.UTF8))
                                {
                                    file_for_exam.WriteLine(xml_text);
                                }
                                SQLiteConnection sqlite_conn;
                                sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; ");
                                sqlite_conn.Open();
                                SQLiteCommand sqlite_cmd;
                                sqlite_cmd = sqlite_conn.CreateCommand();
                                string sql_insert = "INSERT INTO Translated (hash,key_unic,text_en,text_ru_m,translator_m) VALUES ('" + hash1.ToString() + "','" + key.ToString() + "','" + WebUtility.HtmlEncode(text) + "','" + WebUtility.HtmlEncode(dbd[key]) + "','3')";
                                sqlite_cmd.CommandText = sql_insert;
                                sqlite_cmd.ExecuteNonQuery();
                                sqlite_conn.Close();*/
                    }
                    /*if (somework == 1)
                    {
                        string tmp_base;
                        if (dictionary_xml_translator_w.TryGetValue(key, out string sdfg) && dictionary_xml_m[key] != dictionary_xml_w[key])
                        {
                            tmp_base = "INSERT INTO Translated (fileinfo,hash,key_unic,text_en,text_ru_m,translator_m,text_ru_w,translator_w) VALUES ('" + hash1 + "','" + hash1 + "','" + key + "','" + WebUtility.HtmlEncode(text) + "','" + WebUtility.HtmlEncode(dictionary_xml_m[key]).Replace("\n \n", "\n\n").Replace("\n", "\\n") + "','" + dictionary_xml_translator_m[key] + "','" + WebUtility.HtmlEncode(dictionary_xml_w[key]).Replace("\n \n", "\n\n").Replace("\n", "\\n") + "','" + dictionary_xml_translator_w[key] + "');";
                        }
                        else
                        {
                            tmp_base = "INSERT INTO Translated (fileinfo,hash,key_unic,text_en,text_ru_m,translator_m) VALUES ('" + hash1 + "','" + hash1 + "','" + key + "','" + WebUtility.HtmlEncode(text) + "','" + WebUtility.HtmlEncode(dictionary_xml_m[key]).Replace("\n \n", "\n\n").Replace("\n", "\\n") + "','" + dictionary_xml_translator_m[key] + "');";
                        }
                    tmp_base = "'" + nu88 + "','" + num4 + "','" + b + "','" + value + "','" + value2 + "','" + num6 + "','" + hash1 + "','" + key + "','" + WebUtility.HtmlEncode(text) + "','" + WebUtility.HtmlEncode(dictionary_xml_m[key]).Replace("\n \n", "\n\n").Replace("\n", "\\n") + "','" + dictionary_xml_translator_m[key] + "'";
                        using (StreamWriter file_for_exam = new StreamWriter("db\\allbase.txt", true))
                        {
                            file_for_exam.WriteLine(tmp_base);
                        }
                    }*/
                }
                binaryReader2.BaseStream.Position = position;
                num3++;
                binaryWriter.Write(num4);
                binaryWriter.Write(b);
                binaryWriter.Write(value);
                binaryWriter.Write(value2);
                binaryWriter.Write(num5);
                binaryWriter.Write(num2);
                num2 += num5;
                binaryWriter.Write(num5);
            }
            foreach (KeyValuePair<ulong, string> item in dictionary)
            {
                Encoding.UTF8.GetBytes(item.Value.Replace("\\n", "\n"));
                binaryWriter.Write(Encoding.UTF8.GetBytes(item.Value.Replace("\\n", "\n")));
            }
            MemoryStream memoryStream2 = new MemoryStream();
            Deflater deflater = new Deflater();
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

            if (somework == 1)
                using (StreamReader Hashes_Names = new StreamReader("db\\hashes_filename.txt", Encoding.Default))
                {
                    while (!Hashes_Names.EndOfStream)
                    {
                        string line = Hashes_Names.ReadLine();
                        if (line.IndexOf(hash1.ToString("X")) != -1)
                        {
                            string rez = line.Substring(line.IndexOf("/resources/en-us/str/") + 21);
                            string rez2 = rez.Substring(0, rez.IndexOf("#"));
                            if (File_Name_List.Contains(hash1.ToString()))
                            {
                                using (SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=db\\translate.db3; Version = 3; New = True; Compress = True; "))
                                {
                                    sqlite_conn.Open();
                                    using (SQLiteCommand sqlite_cmd = new SQLiteCommand(sqlite_conn))
                                    {
                                        sqlite_cmd.CommandText = "UPDATE Translated SET fileinfo='" + WebUtility.HtmlEncode(rez2) + "' WHERE hash ='" + hash1 + "'";
                                        sqlite_cmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                    }
                }
            Hhh(fileStream, array3, newzsize, newsize, hash2, hash1, endtable);
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
        public string Translator(string text,string translator_system)
        {
            if (translator_system == "Deepl")
            {
                var outputElements = "";
                if (Deepl_First_time == 1)
                {
                    Deepl_First_time = 0;
                    FirefoxOptions options = new FirefoxOptions();
                    //options.AddArguments("--headless");
                    driver = new FirefoxDriver(options)
                    {
                        Url = "https://deepl.com/translator"
                    };

                    driver.FindElement(By.XPath("//*[@dl-test=\"translator-source-lang-btn\"]")).Click();
                    Thread.Sleep(1000);
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    wait.Until(d => d.FindElements(By.XPath("//*[@dl-test=\"translator-source-lang-list\"]/div/div/button[@dl-test=\"translator-lang-option-en\"]")).Count > 0);
                    Actions actions = new Actions(driver);
                    actions.MoveToElement(driver.FindElement(By.XPath("//*[@dl-test=\"translator-source-lang-list\"]/div/div/button[@dl-test=\"translator-lang-option-en\"]")));
                    actions.Perform();
                    driver.FindElement(By.XPath("//*[@dl-test=\"translator-source-lang-list\"]/div/div/button[@dl-test=\"translator-lang-option-en\"]")).Click();

                    driver.FindElement(By.XPath("//*[@dl-test=\"translator-target-lang-btn\"]")).Click();
                    Thread.Sleep(1000);
                    wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    wait.Until(d => d.FindElements(By.XPath("//*[@dl-test=\"translator-target-lang-list\"]/div/div/button[@dl-test=\"translator-lang-option-ru-RU\"]")).Count > 0);
                    Actions actionss = new Actions(driver);
                    actionss.MoveToElement(driver.FindElement(By.XPath("//*[@dl-test=\"translator-target-lang-list\"]/div/div/button[@dl-test=\"translator-lang-option-ru-RU\"]")));
                    actionss.Perform();
                    driver.FindElement(By.XPath("//*[@dl-test=\"translator-target-lang-list\"]/div/div/button[@dl-test=\"translator-lang-option-ru-RU\"]")).Click();
                }
                if (driver.FindElement(By.XPath("//*[@dl-test=\"translator-source-lang\"]/button/span/strong")).Text != "английский")
                {
                    driver.FindElement(By.XPath("//*[@dl-test=\"translator-source-lang-btn\"]")).Click();
                    Thread.Sleep(1000);
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    wait.Until(d => d.FindElements(By.XPath("//*[@dl-test=\"translator-source-lang-list\"]/div/div/button[@dl-test=\"translator-lang-option-en\"]")).Count > 0);
                    Actions actions = new Actions(driver);
                    actions.MoveToElement(driver.FindElement(By.XPath("//*[@dl-test=\"translator-source-lang-list\"]/div/div/button[@dl-test=\"translator-lang-option-en\"]")));
                    actions.Perform();
                    driver.FindElement(By.XPath("//*[@dl-test=\"translator-source-lang-list\"]/div/div/button[@dl-test=\"translator-lang-option-en\"]")).Click();

                    driver.FindElement(By.XPath("//*[@dl-test=\"translator-target-lang-btn\"]")).Click();
                    Thread.Sleep(1000);
                    wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    wait.Until(d => d.FindElements(By.XPath("//*[@dl-test=\"translator-target-lang-list\"]/div/div/button[@dl-test=\"translator-lang-option-ru-RU\"]")).Count > 0);
                    Actions actionss = new Actions(driver);
                    actionss.MoveToElement(driver.FindElement(By.XPath("//*[@dl-test=\"translator-target-lang-list\"]/div/div/button[@dl-test=\"translator-lang-option-ru-RU\"]")));
                    actionss.Perform();
                    driver.FindElement(By.XPath("//*[@dl-test=\"translator-target-lang-list\"]/div/div/button[@dl-test=\"translator-lang-option-ru-RU\"]")).Click();
                }
                Thread.Sleep(500);
                try
                {
                    Actions actionss = new Actions(driver);
                    actionss.MoveToElement(driver.FindElement(By.XPath("//*[@dl-test=\"translator-source-clear-button\"]")));
                    actionss.Perform();
                    driver.FindElement(By.XPath("//*[@dl-test=\"translator-source-clear-button\"]")).Click();
                }
                catch
                {
                    
                }
                try
                {
                    var textInputElement = driver.FindElement(By.XPath("//*[@dl-test=\"translator-source-input\"]"));
                    textInputElement.SendKeys(text);
                    Thread.Sleep(1000);
                    //var copyElement = driver.FindElement(By.XPath("//*[@dl-test=\"translator-target-toolbar-copy\"]/button"));
                    while (driver.FindElements(By.XPath(".//div[@class='lmt__mobile_share_container lmt--mobile-hidden']")).Count <= 0)
                    {
                        Thread.Sleep(500);
                    }
                    Thread.Sleep(1000);
                    if (driver.FindElements(By.XPath(".//div[@dl-test=\"translator-target-result-as-text-container\"]/p[@class=\"lmt__translations_as_text__item lmt__translations_as_text__main_translation\"]/button[@class=\"lmt__translations_as_text__text_btn\"]/span")).Count <= 0)
                        outputElements = driver.FindElement(By.XPath(".//div[@dl-test=\"translator-target-result-as-text-container\"]/p[@class=\"lmt__translations_as_text__item lmt__translations_as_text__main_translation\"]/button[@class=\"lmt__translations_as_text__text_btn\"]")).GetAttribute("textContent");
                    else
                        outputElements = driver.FindElement(By.XPath(".//div[@dl-test=\"translator-target-result-as-text-container\"]/p[@class=\"lmt__translations_as_text__item lmt__translations_as_text__main_translation\"]/button[@class=\"lmt__translations_as_text__text_btn\"]/span")).GetAttribute("textContent");
                    if (outputElements.Length <= 1)
                    {
                        Thread.Sleep(1000);
                        if (driver.FindElements(By.XPath(".//div[@dl-test=\"translator-target-result-as-text-container\"]/p[@class=\"lmt__translations_as_text__item lmt__translations_as_text__main_translation\"]/button[@class=\"lmt__translations_as_text__text_btn\"]/span")).Count <= 0)
                            outputElements = driver.FindElement(By.XPath(".//div[@dl-test=\"translator-target-result-as-text-container\"]/p[@class=\"lmt__translations_as_text__item lmt__translations_as_text__main_translation\"]/button[@class=\"lmt__translations_as_text__text_btn\"]")).GetAttribute("textContent");
                        else
                            outputElements = driver.FindElement(By.XPath(".//div[@dl-test=\"translator-target-result-as-text-container\"]/p[@class=\"lmt__translations_as_text__item lmt__translations_as_text__main_translation\"]/button[@class=\"lmt__translations_as_text__text_btn\"]/span")).GetAttribute("textContent");

                    }

                }
                catch {
                    //Console.WriteLine("Error");
                    Deepl_First_time = 1;
                    driver.Close();
                    driver.Quit();
                    return text;
                    /*Deepl_First_time = 1;   
                    Thread.Sleep(10000);
                    driver.Close();
                    driver.Quit();
                    return text;*/
                }
            if (outputElements == "")
                    return text;
                else
                    return outputElements;
            }
            /*else if (translator_system == "Promt")
            {
                Thread.Sleep(1000);
                var baseAddress = "https://www.translate.ru/api/soap/getTranslation";
                var http = (HttpWebRequest)WebRequest.Create(new Uri(baseAddress));
                http.Accept = "application/json";
                http.ContentType = "application/json";
                http.Method = "POST";

                string parsedContent = "{ dirCode:'en-ru', topic:'General', text:'" + WebUtility.HtmlEncode(text) + "', lang:'ru', limit:'3000',useAutoDetect:true, key:'123', ts:'MainSite',tid:'', IsMobile:false}";
                UTF8Encoding encoding = new UTF8Encoding();
                Byte[] bytessss = encoding.GetBytes(parsedContent);

                Stream newStream = http.GetRequestStream();
                newStream.Write(bytessss, 0, bytessss.Length);
                newStream.Close();

                var response = http.GetResponse();

                var stream = response.GetResponseStream();
                var sr = new StreamReader(stream);
                var result = sr.ReadToEnd();
                var details = JObject.Parse(result);
                transl_a = details["d"]["result"].ToString();
                if (transl_a.Contains("<div class=\"sourceTxt\">"))
                {
                    int posi = transl_a.IndexOf("<div class=\"sourceTxt\">");
                    transl_a = transl_a.Substring(posi + 23);
                    posi = transl_a.IndexOf("</div>");
                    transl_a = transl_a.Substring(0, posi);
                }
                return transl_a;
            }*/
            else
                return text;

        }
    }
}
