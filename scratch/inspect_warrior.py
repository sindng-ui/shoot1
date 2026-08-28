from PIL import Image
import numpy as np

img_path = "/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Warrior/warrior.png"
img = Image.open(img_path)
print(f"Format: {img.format}, Size: {img.size}, Mode: {img.mode}")

# Check corner pixels
rgb_img = img.convert('RGB')
corners = [
    rgb_img.getpixel((0, 0)),
    rgb_img.getpixel((img.size[0] - 1, 0)),
    rgb_img.getpixel((0, img.size[1] - 1)),
    rgb_img.getpixel((img.size[0] - 1, img.size[1] - 1)),
]
print(f"Corner colors: {corners}")
