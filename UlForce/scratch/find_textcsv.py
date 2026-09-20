import UnityPy

env = UnityPy.load('E:/GameH/VenusBlood RAGNAROK International (US)/VBRI_Data/resources.assets')

for obj in env.objects:
    if obj.type.name == 'TextAsset':
        data = obj.read()
        name = getattr(data, 'm_Name', '')
        if 'research' in name.lower() or 'textcsv' in name.lower():
            print(f"TextAsset: {name}")
            content = getattr(data, 'm_Script', '')
            if isinstance(content, bytes):
                content = content.decode('utf-8', errors='ignore')
            import sys
            sys.stdout.buffer.write(content.encode('utf-8'))
