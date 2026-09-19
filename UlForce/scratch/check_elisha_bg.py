from PIL import Image
import numpy as np

im = Image.open(r"C:\Users\cs_yo\.gemini\antigravity\brain\9790f50a-68df-41ba-bf8c-2d22c1d3dc05\elisha_portrait_1789795037553.jpg")
arr = np.array(im)
print("Corners:")
print("0,0:", arr[0, 0])
print("0,w-1:", arr[0, -1])
print("h-1,0:", arr[-1, 0])
print("h-1,w-1:", arr[-1, -1])
print("Center top (0, 512):", arr[0, 512])
print("Center bottom (1023, 512):", arr[1023, 512])
print("Center left (512, 0):", arr[512, 0])
print("Center right (512, 1023):", arr[512, 1023])

