from PIL import Image
import numpy as np
from collections import deque

out_path = r"c:\Users\cs_yo\source\repos\UlForce\UlForce\Assets\UlForce_elisha.png"
im = Image.open(out_path).convert("RGBA")
arr = np.array(im, dtype=np.uint8)

alpha = arr[:, :, 3]
h, w = alpha.shape

visited = np.zeros((h, w), dtype=bool)
components = []

for y in range(h):
    for x in range(w):
        if alpha[y, x] > 0 and not visited[y, x]:
            comp = []
            q = deque([(y, x)])
            visited[y, x] = True
            while q:
                cy, cx = q.popleft()
                comp.append((cy, cx))
                for dy, dx in [(-1, 0), (1, 0), (0, -1), (0, 1)]:
                    ny, nx = cy + dy, cx + dx
                    if 0 <= ny < h and 0 <= nx < w and not visited[ny, nx] and alpha[ny, nx] > 0:
                        visited[ny, nx] = True
                        q.append((ny, nx))
            components.append(comp)

components.sort(key=len, reverse=True)
print(f"Non-zero alpha components: {len(components)}")
for i, c in enumerate(components[:5]):
    print(f"Comp {i}: size={len(c)}")

# Remove tiny speckles < 100 px
for comp in components:
    if len(comp) < 100:
        for py, px in comp:
            arr[py, px, 3] = 0

cleaned = Image.fromarray(arr, "RGBA")
cleaned.save(out_path, "PNG")
print("Cleaned speckles from Elisha PNG.")

