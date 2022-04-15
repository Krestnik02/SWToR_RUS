import os
import json
from game_work import TorEditor

with open("fonts/font_data.json", "r") as f:
    fonts_data = json.load(f)

t = TorEditor("D:/GO/SWTOR/")

for font_data in fonts_data:
    with open(os.path.join("fonts/", font_data["filename"]), "rb") as f:
        swtor_hash = font_data["hash1"], font_data["hash2"]
        t.change_file_in_tor(f.read(), swtor_hash, "D:/Games/Star Wars - The Old Republic/swtor/retailclient/main_gfx_1.tor")
