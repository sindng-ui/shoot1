import os
from PIL import Image

files = [
    "Assets/Resources/Characters/Warrior/warrior_front.png",
    "Assets/Resources/Characters/Warrior/warrior_side.png",
    "Assets/Resources/Characters/Ranger/ranger_front.png",
    "Assets/Resources/Characters/Ranger/ranger_side.png",
    "Assets/Resources/Characters/Wizard/wizard_front.png",
    "Assets/Resources/Characters/Wizard/wizard_side.png"
]

for f in files:
    if os.path.exists(f):
        im = Image.open(f)
        bbox = im.getbbox()
        w, h = im.size
        bw = bbox[2] - bbox[0]
        bh = bbox[3] - bbox[1]
        print(f"{f}: Image={w}x{h}, BBox={bbox}, CharSize={bw}x{bh}")
    else:
        print(f"File not found: {f}")
