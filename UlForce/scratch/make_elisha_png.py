from PIL import Image
import numpy as np
from collections import deque
from scipy.ndimage import binary_dilation

input_path = r"C:\Users\cs_yo\.gemini\antigravity\brain\9790f50a-68df-41ba-bf8c-2d22c1d3dc05\elisha_portrait_1789795037553.jpg"
im = Image.open(input_path).convert("RGBA")
arr = np.array(im, dtype=np.uint8)

h, w, _ = arr.shape
rgb = arr[:, :, :3].astype(np.float32)
r, g, b = rgb[:, :, 0], rgb[:, :, 1], rgb[:, :, 2]

max_c = np.maximum(np.maximum(r, g), b)
min_c = np.minimum(np.minimum(r, g), b)
diff_c = max_c - min_c

# Background is neutral and bright (white or light gray checkerboard)
is_bg = (min_c >= 190) & (diff_c <= 12)

# Flood fill from outer image borders
visited = np.zeros((h, w), dtype=bool)
queue = deque()

for x in range(w):
    if is_bg[0, x]:
        visited[0, x] = True
        queue.append((0, x))
    if is_bg[h - 1, x] and not visited[h - 1, x]:
        visited[h - 1, x] = True
        queue.append((h - 1, x))

for y in range(h):
    if is_bg[y, 0] and not visited[y, 0]:
        visited[y, 0] = True
        queue.append((y, 0))
    if is_bg[y, w - 1] and not visited[y, w - 1]:
        visited[y, w - 1] = True
        queue.append((y, w - 1))

while queue:
    cy, cx = queue.popleft()
    for dy, dx in [(-1, 0), (1, 0), (0, -1), (0, 1)]:
        ny, nx = cy + dy, cx + dx
        if 0 <= ny < h and 0 <= nx < w:
            if not visited[ny, nx] and is_bg[ny, nx]:
                visited[ny, nx] = True
                queue.append((ny, nx))

print(f"Flooded outer background: {np.sum(visited)} pixels")

# Also check for enclosed pockets of neutral background (e.g. between hair and staff, between horns, etc.)
# Any connected component of is_bg that does NOT contain skin (diff_c <= 10)
unvisited_bg = is_bg & (~visited)
print(f"Unvisited neutral pockets: {np.sum(unvisited_bg)} pixels")

# Let's label unvisited pockets and inspect them
visited_all_bg = visited.copy()

# A pocket is background if min_c >= 210 and diff_c <= 8 and size >= 50
p_visited = np.zeros((h, w), dtype=bool)
for y in range(h):
    for x in range(w):
        if unvisited_bg[y, x] and not p_visited[y, x]:
            comp = []
            q = deque([(y, x)])
            p_visited[y, x] = True
            while q:
                cy, cx = q.popleft()
                comp.append((cy, cx))
                for dy, dx in [(-1, 0), (1, 0), (0, -1), (0, 1)]:
                    ny, nx = cy + dy, cx + dx
                    if 0 <= ny < h and 0 <= nx < w and not p_visited[ny, nx] and unvisited_bg[ny, nx]:
                        p_visited[ny, nx] = True
                        q.append((ny, nx))
            # Check if this pocket is enclosed background
            ys = [p[0] for p in comp]
            xs = [p[1] for p in comp]
            min_y, max_y = min(ys), max(ys)
            min_x, max_x = min(xs), max(xs)
            # Face area is around y=400..600, x=450..580
            in_face = (min_y >= 350 and max_y <= 650 and min_x >= 420 and max_x <= 600)
            if not in_face and len(comp) >= 50:
                print(f"Adding pocket: size={len(comp)}, y=[{min_y}, {max_y}], x=[{min_x}, {max_x}]")
                for py, px in comp:
                    visited_all_bg[py, px] = True

alpha = np.ones((h, w), dtype=np.uint8) * 255
alpha[visited_all_bg] = 0

# Slight dilation into neutral boundary to remove white/gray fringe
dilated_bg = binary_dilation(visited_all_bg, iterations=1)
alpha[dilated_bg & (diff_c <= 15)] = 0

arr[:, :, 3] = alpha
out_path = r"c:\Users\cs_yo\source\repos\UlForce\UlForce\Assets\UlForce_elisha.png"
Image.fromarray(arr, "RGBA").save(out_path, "PNG")
print("Saved transparent PNG to", out_path)

