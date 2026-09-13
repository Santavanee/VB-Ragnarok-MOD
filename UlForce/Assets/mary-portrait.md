# Mary portrait asset

Generated with the built-in imagegen tool from the supplied screenshot.
`mary-source.png` is the selected reconstruction. It is RGB with a baked-in checkerboard and is NOT yet the runtime portrait. Do not deploy it as UlForce_mary.png.

Final extraction is pending permission to use local image processing after two built-in attempts failed to output actual alpha transparency.

Initial prompt:
Use case: background-extraction. Edit target: the attached game screenshot. Create a clean transparent PNG cutout ONLY of the purple-haired pirate queen character portrait near the left-center (Swimsuit Queen Mary). Remove all background, surrounding game UI, text, borders, and the two square face icons overlapping the top of her pirate hat. Reconstruct only the small occluded hat areas naturally. Preserve the original character's face, blue eyes, purple hair, black pirate hat, costume, pose, proportions and chibi game sprite style as closely as possible. No redesign, no added objects, no text, no boxes. Frame the existing portrait tightly with a small transparent margin, retaining all visible hat and hair. Output actual alpha transparency, not a checkerboard painted into the image.

Second attempt:
Edit this generated Mary portrait: remove the gray checkerboard completely and return a REAL transparent PNG with RGBA alpha channel (alpha=0 outside the character). The current file is RGB with a baked-in checkerboard, which is not transparent. Keep the character unchanged. No checkerboard, no background color, no shadow outside the character. This is a game sprite cutout requiring actual transparency, not a visualization of transparency.
