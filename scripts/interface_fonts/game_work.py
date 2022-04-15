# -*- coding: utf-8 -*-
import zlib
import os
from xml.dom import minidom
import xml.etree.cElementTree as ET


def read_manifest():
    file_path = "stb.manifest"
    paths = []
    xmldoc = minidom.parse(file_path)
    text_list = xmldoc.getElementsByTagName('file')

    for text_el in text_list:
        paths.append("/resources/en-us/" + text_el.attributes["val"].value.replace(".", "/") + ".stb")

    return paths


def get_dir(string):
    res = ""
    dirs = string.split("/")[:-1]
    for dir in dirs:
        res += dir + "/"
    return res


def stb_to_xml(stb_strings, path):
    root = ET.Element("stb_file")

    head = ET.SubElement(root, "file_info")
    strs_num = ET.SubElement(head, "strs")
    strs_num.text = str(len(stb_strings))
    file_name = ET.SubElement(head, "file_name")
    file_name.text = path

    body = ET.SubElement(root, "file_content")
    for stb_string in stb_strings:
        stb_text = ET.SubElement(body, "text")
        stb_text.set("id1", str(stb_string.id))
        stb_text.set("id2", str(stb_string.id2))
        stb_text.set("bitflag", str(stb_string.bitflag))
        stb_text.set("version", str(stb_string.version))
        stb_text.text = stb_string.text

    dom = minidom.parseString(ET.tostring(root))
    file_cont = dom.toprettyxml(indent='\t', encoding="utf-8")
    return file_cont.replace(b"\n", b"\r\n")


def stb_to_txt(stb_strings):
    file_txt_cont = ""
    for stb_string in stb_strings:
        # file_txt_cont += stb_string.text.replace(".", "(point)").replace(":", "(colon)") + "\r\n"
        file_txt_cont += stb_string.text.replace("\r\n", "(\\r\\n)").replace("\n", "(\\n)") + "\r\n\r\n"
    return file_txt_cont.encode("utf-8")


class Hash():
    def mix(self, a, b, c):  # functions for turning the path into hash
        a &= 0xffffffff;
        b &= 0xffffffff;
        c &= 0xffffffff
        a -= c;
        a &= 0xffffffff;
        a ^= self.rot(c, 4);
        a &= 0xffffffff;
        c += b;
        c &= 0xffffffff
        b -= a;
        b &= 0xffffffff;
        b ^= self.rot(a, 6);
        b &= 0xffffffff;
        a += c;
        a &= 0xffffffff
        c -= b;
        c &= 0xffffffff;
        c ^= self.rot(b, 8);
        c &= 0xffffffff;
        b += a;
        b &= 0xffffffff
        a -= c;
        a &= 0xffffffff;
        a ^= self.rot(c, 16);
        a &= 0xffffffff;
        c += b;
        c &= 0xffffffff
        b -= a;
        b &= 0xffffffff;
        b ^= self.rot(a, 19);
        b &= 0xffffffff;
        a += c;
        a &= 0xffffffff
        c -= b;
        c &= 0xffffffff;
        c ^= self.rot(b, 4);
        c &= 0xffffffff;
        b += a;
        b &= 0xffffffff
        return a, b, c

    def rot(self, x, k):
        return (((x) << (k)) | ((x) >> (32 - (k))))

    def final(self, a, b, c):
        a &= 0xffffffff;
        b &= 0xffffffff;
        c &= 0xffffffff
        c ^= b;
        c &= 0xffffffff;
        c -= self.rot(b, 14);
        c &= 0xffffffff
        a ^= c;
        a &= 0xffffffff;
        a -= self.rot(c, 11);
        a &= 0xffffffff
        b ^= a;
        b &= 0xffffffff;
        b -= self.rot(a, 25);
        b &= 0xffffffff
        c ^= b;
        c &= 0xffffffff;
        c -= self.rot(b, 16);
        c &= 0xffffffff
        a ^= c;
        a &= 0xffffffff;
        a -= self.rot(c, 4);
        a &= 0xffffffff
        b ^= a;
        b &= 0xffffffff;
        b -= self.rot(a, 14);
        b &= 0xffffffff
        c ^= b;
        c &= 0xffffffff;
        c -= self.rot(b, 24);
        c &= 0xffffffff
        return a, b, c

    def hashlittle2(self, data, initval=0, initval2=0):
        length = lenpos = len(data)

        a = b = c = (0xdeadbeef + (length) + initval)

        c += initval2;
        c &= 0xffffffff

        p = 0  # string offset
        while lenpos > 12:
            a += (ord(data[p + 0]) + (ord(data[p + 1]) << 8) + (ord(data[p + 2]) << 16) + (ord(data[p + 3]) << 24));
            a &= 0xffffffff
            b += (ord(data[p + 4]) + (ord(data[p + 5]) << 8) + (ord(data[p + 6]) << 16) + (ord(data[p + 7]) << 24));
            b &= 0xffffffff
            c += (ord(data[p + 8]) + (ord(data[p + 9]) << 8) + (ord(data[p + 10]) << 16) + (ord(data[p + 11]) << 24));
            c &= 0xffffffff
            a, b, c = self.mix(a, b, c)
            p += 12
            lenpos -= 12

        if lenpos == 12: c += (
                ord(data[p + 8]) + (ord(data[p + 9]) << 8) + (ord(data[p + 10]) << 16) + (
                    ord(data[p + 11]) << 24)); b += (
                ord(data[p + 4]) + (ord(data[p + 5]) << 8) + (ord(data[p + 6]) << 16) + (
                    ord(data[p + 7]) << 24)); a += (
                ord(data[p + 0]) + (ord(data[p + 1]) << 8) + (ord(data[p + 2]) << 16) + (ord(data[p + 3]) << 24));
        if lenpos == 11: c += (ord(data[p + 8]) + (ord(data[p + 9]) << 8) + (ord(data[p + 10]) << 16)); b += (
                ord(data[p + 4]) + (ord(data[p + 5]) << 8) + (ord(data[p + 6]) << 16) + (
                    ord(data[p + 7]) << 24)); a += (
                ord(data[p + 0]) + (ord(data[p + 1]) << 8) + (ord(data[p + 2]) << 16) + (ord(data[p + 3]) << 24));
        if lenpos == 10: c += (ord(data[p + 8]) + (ord(data[p + 9]) << 8)); b += (
                ord(data[p + 4]) + (ord(data[p + 5]) << 8) + (ord(data[p + 6]) << 16) + (
                    ord(data[p + 7]) << 24)); a += (
                ord(data[p + 0]) + (ord(data[p + 1]) << 8) + (ord(data[p + 2]) << 16) + (ord(data[p + 3]) << 24));
        if lenpos == 9:  c += (ord(data[p + 8])); b += (
                ord(data[p + 4]) + (ord(data[p + 5]) << 8) + (ord(data[p + 6]) << 16) + (
                    ord(data[p + 7]) << 24)); a += (
                ord(data[p + 0]) + (ord(data[p + 1]) << 8) + (ord(data[p + 2]) << 16) + (ord(data[p + 3]) << 24));
        if lenpos == 8:  b += (
                ord(data[p + 4]) + (ord(data[p + 5]) << 8) + (ord(data[p + 6]) << 16) + (
                    ord(data[p + 7]) << 24)); a += (
                ord(data[p + 0]) + (ord(data[p + 1]) << 8) + (ord(data[p + 2]) << 16) + (ord(data[p + 3]) << 24));
        if lenpos == 7:  b += (ord(data[p + 4]) + (ord(data[p + 5]) << 8) + (ord(data[p + 6]) << 16)); a += (
                ord(data[p + 0]) + (ord(data[p + 1]) << 8) + (ord(data[p + 2]) << 16) + (ord(data[p + 3]) << 24));
        if lenpos == 6:  b += ((ord(data[p + 5]) << 8) + ord(data[p + 4])); a += (
                ord(data[p + 0]) + (ord(data[p + 1]) << 8) + (ord(data[p + 2]) << 16) + (ord(data[p + 3]) << 24))
        if lenpos == 5:  b += (ord(data[p + 4])); a += (
                ord(data[p + 0]) + (ord(data[p + 1]) << 8) + (ord(data[p + 2]) << 16) + (ord(data[p + 3]) << 24));
        if lenpos == 4:  a += (
                ord(data[p + 0]) + (ord(data[p + 1]) << 8) + (ord(data[p + 2]) << 16) + (ord(data[p + 3]) << 24))
        if lenpos == 3:  a += (ord(data[p + 0]) + (ord(data[p + 1]) << 8) + (ord(data[p + 2]) << 16))
        if lenpos == 2:  a += (ord(data[p + 0]) + (ord(data[p + 1]) << 8))
        if lenpos == 1:  a += ord(data[p + 0])
        a &= 0xffffffff;
        b &= 0xffffffff;
        c &= 0xffffffff
        if lenpos == 0: return c, b

        a, b, c = self.final(a, b, c)

        return c, b


class TorEditor:
    def __init__(self, dirgame):
        self.dirgame = dirgame
        self.dirassets = self.dirgame + "Assets/"
        self.hash = Hash()
        self.stb_wotk = None

    def restore(self, arc):
        with open("backup/" + arc, "rb") as torbu:
            cont = torbu.read()
        with open(self.dirassets + arc, "wb") as tor1:
            tor1.write(cont)

    def backup(self, arc):
        with open(self.dirassets + arc, "rb") as tor1:
            cont = tor1.read()
        with open("backup/" + arc, "wb") as torbu:
            torbu.write(cont)
        return 1

    def get_uint_from_tor_object(self, i, show=False):  # the function to read reversed (LE?) integers
        cont = self.temp_tor_obj.read(i)
        if show:
            print(cont)
        return self.get_uint(cont)

    def get_uint(self, cont, i=None):
        temp = False
        if i is None:
            i = len(cont)
        sum = 0
        for d in range(i):
            a = cont[d]
            sum += a * (256 ** d)
        if temp:
            print()
        return sum

    def tole(self, d, i):  # turning an integer into an array of bytes in LE
        p = []
        num = d
        for c in range(i):
            a = num % 256
            p.append(a)
            num = num // 256
        return p

    def analize_temp_entry(self, entry):
        sh = self.get_uint(entry[20:24])
        ph = self.get_uint(entry[24:28])

        if sh == self.path[0] and ph == self.path[1]:
            file_offset = self.get_uint(entry[0:8])
            meta_data_size = self.get_uint(entry[8:12])
            compr_size = self.get_uint(entry[12:16])
            un_compr_size = self.get_uint(entry[16:20])
            crc = self.get_uint(entry[28:32])
            is_compressed = self.get_uint(entry[32:34])

            characs = {
                "file_offset": file_offset,
                "meta_data_size": meta_data_size,
                "compr_size": compr_size,
                "un_compr_size": un_compr_size,
                "crc": crc,
                "is_compressed": is_compressed,
                "hash1": sh,
                "hash2": ph
            }
            return characs
        else:
            return None

    def look_for_tor_path(self, tor_file_obj, tor_path, for_extraction=False):
        n = 0
        self.temp_tor_obj = tor_file_obj
        if isinstance(tor_path, str):
            # object of file with tor extension
            self.path = self.hash.hashlittle2(tor_path)
        else:
            self.path = tor_path
        # swtor path (e.g. /recources/gfx/...) in hash (tuple with 2 elements)
        foundnum = 0
        # number of found files
        need_empty_entry = False
        # it becomes True when the entry we are looking for is found: only then we need to find empty entry
        empty_entry_offset = None
        result = None
        # if found, it's a dictionary with characteristics

        magic = self.get_uint_from_tor_object(4)
        # checking whether it's .tor file
        if magic != 0x50594d:
            return None

        version = self.get_uint_from_tor_object(4)
        if version != 5:
            return None

        byteorder = self.get_uint_from_tor_object(4)
        if byteorder != 0xFD23EC43:
            return None
        ftoffset = self.get_uint_from_tor_object(8)
        # current offset we are working with
        self.temp_tor_obj.seek(ftoffset)
        while ftoffset > 0:
            # reading entries
            ftcapacity = self.get_uint_from_tor_object(4)
            # number of entries in the table
            old_ftoffset = ftoffset
            ftoffset = self.get_uint_from_tor_object(8)
            for c in range(ftcapacity):
                temp_entry = self.temp_tor_obj.read(34)
                if temp_entry.replace(b"\x00", b""):
                    temp_result = self.analize_temp_entry(temp_entry)
                    if temp_result is not None:
                        foundnum += 1
                        if for_extraction:
                            return temp_result
                        result = temp_result
                        # saving result; it's not mutable object
                        result["entry_offset"] = old_ftoffset + 4 + 8 + 34 * c
                        # we should know where our entry is located
                        need_empty_entry = True
                        # now we should look for an empty entry
                else:
                    n += 1
                    if need_empty_entry:
                        # we will need to know where is an empty entry if the new file is bigger than the original one
                        empty_entry_offset = old_ftoffset + 4 + 8 + 34 * c
                        need_empty_entry = False
            self.temp_tor_obj.seek(ftoffset)
        if for_extraction and foundnum == 0:
            return None
        # here extraction ends

        if foundnum == 1:
            if empty_entry_offset:
                result["empty_entry_offset"] = empty_entry_offset
                return result
        else:
            return None

        del self.path, self.temp_tor_obj

    def extract_file(self, tor_path, ext="default", save=True, archive=None):
        if archive is None:
            arcs = os.listdir(self.dirassets)
            for arc_name in arcs:
                print(arc_name)
                with open(self.dirassets + arc_name, "rb") as arc:
                    result = self.look_for_tor_path(arc, tor_path, for_extraction=True)
                    if result:
                        required_archive_name = arc_name
                        archive_path = self.dirassets + required_archive_name
                        break
        else:
            with open(archive, "rb") as arc:
                result = self.look_for_tor_path(arc, tor_path, for_extraction=True)
                if result:
                    required_archive_name = archive
                    if "/" in archive or "\\" in archive:
                        archive_path = archive
                    else:
                        archive_path = self.dirassets + required_archive_name

        if result is not None:
            with open(archive_path, "rb") as tor:
                file_beginning = result["file_offset"]
                tor.seek(file_beginning)
                metadata_content = tor.read(result["meta_data_size"])
                file_content = tor.read(result["compr_size"])

            if result["is_compressed"]:
                file_content = zlib.decompress(file_content)

            path = ".{}".format(tor_path)
            dir = get_dir(path)

            if save:
                if not os.path.exists(dir):
                    os.makedirs(dir)

            if ext == "default":
                content = file_content
                filename = path
            elif ext == "xml":
                stb_strings = self.stb_wotk.read_stb(file_content)
                content = stb_to_xml(stb_strings, tor_path)
                filename = path.replace(".stb", ".xml")
            elif ext == "txt":
                stb_strings = self.stb_wotk.read_stb(file_content)
                content = stb_to_txt(stb_strings)
                filename = path.replace(".stb", ".txt")
            else:
                raise ValueError

            if save:
                with open(filename, "wb") as file:
                    file.write(content)
            else:
                return metadata_content, content, result
        else:
            pass

    def extract_everything(self, tor_paths, format="default"):
        i = 0
        l = len(tor_paths)
        for tor_path in tor_paths:
            if i % 20 == 0:
                print(round(i / l * 100, 2))
            i += 1
            try:
                self.extract_file(tor_path.strip(), ext=format)
            except Exception as e:
                print(type(e), e)
                print("Failed:", tor_path)
                continue

    def create_entry(self, file_offset, meta_data_size, file_compr_size, file_un_compr_size, path1, path2, crc,
                     is_compressed):
        tablecont = []
        tablecont += self.tole(file_offset, 8)  # creating new characteristics
        tablecont += self.tole(meta_data_size, 4)
        tablecont += self.tole(file_compr_size, 4)
        tablecont += self.tole(file_un_compr_size, 4)
        tablecont += self.tole(path1, 4)
        tablecont += self.tole(path2, 4)
        tablecont += self.tole(crc, 4)
        tablecont += self.tole(is_compressed, 2)
        return bytes(tablecont)

    def change_file_in_tor(self, new_file_content, tor_path, tor_archive_name=None):
        result = None
        if tor_archive_name:
            if "/" in tor_archive_name or "\\" in tor_archive_name:
                tor_archive_path = tor_archive_name
            else:
                tor_archive_path = self.dirassets + tor_archive_name

            with open(tor_archive_path, "rb") as tor_archive:
                result = self.look_for_tor_path(tor_archive, tor_path)
        else:
            tor_archive_names = os.listdir(self.dirassets)
            for temp_tor_archive_name in tor_archive_names:
                print(tor_archive_name)
                with open(self.dirassets + temp_tor_archive_name, "rb") as temp_tor_archive:
                    result = self.look_for_tor_path(temp_tor_archive, tor_path)
                    if result:
                        tor_archive_name = temp_tor_archive_name
                        break
        if not result:
            print("Not found")
            return 0
        # now result is a dictionary

        new_un_compr_size = len(new_file_content)
        if result["is_compressed"]:
            new_file_content = zlib.compress(new_file_content)
        new_compr_size = len(new_file_content)

        tor_archive_len = os.path.getsize(tor_archive_path)
        new_file_offset = new_compr_size > result["compr_size"] and tor_archive_len or result["file_offset"]
        # if bigger, than offset is len of archive, else - old offset
        entry_compr_size = max([new_compr_size, result["compr_size"]])
        # it's compr_size which we are going to write into the entry

        new_entry = self.create_entry(new_file_offset,
                                      result["meta_data_size"],
                                      entry_compr_size,
                                      new_un_compr_size,
                                      self.path[0],
                                      self.path[1],
                                      result["crc"],
                                      result["is_compressed"]
                                      )
        # bytes object

        if new_compr_size > result["compr_size"]:
            print("More")
            with open(tor_archive_path, "rb+") as tor_archive:
                tor_archive.seek(result["file_offset"])
                meta_data = tor_archive.read(result["meta_data_size"])
                # reading meta data
                tor_archive.seek(result["empty_entry_offset"])
                tor_archive.write(new_entry)
                # writing new entry into the file
            with open(tor_archive_path, "ab") as tor_archive:
                tor_archive.write(meta_data + new_file_content)
        else:
            print("Less")
            new_file_content += b"\x00" * (result["compr_size"] - new_compr_size)
            # making new_file_content with similar length as old
            with open(tor_archive_path, "rb+") as tor_archive:
                tor_archive.seek(result["entry_offset"])
                tor_archive.write(new_entry)
                tor_archive.seek(result["file_offset"] + result["meta_data_size"])
                tor_archive.write(new_file_content)
        return 1
