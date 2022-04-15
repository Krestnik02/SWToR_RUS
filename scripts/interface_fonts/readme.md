
# SWTOR new interface fonts

They are located in GameDir/swtor/retailclient/main_gfx_1.tor. There were more than 500 files in the .tor archive in 7.0.1 patch.

To work on fonts, extract all files from the archive (first, with known hashes, then everything left). Then we can remove dds files (DDS header) and leave only .gfx (CFX header) and .swf files (CWS header). We can edit them with [JPEXS decompiler](https://github.com/jindrapetrik/jpexs-decompiler). If we see fonts directory in this program while opening file and english fonts with them, we should embed Russian.