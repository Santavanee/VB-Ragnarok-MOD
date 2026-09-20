import glob
import UnityPy

for path in glob.glob('E:/GameH/VenusBlood RAGNAROK International (US)/VBRI_Data/sharedassets*.assets'):
    try:
        env = UnityPy.load(path)
        for obj in env.objects:
            if obj.type.name == 'GameObject':
                data = obj.read()
                name = getattr(data, 'm_Name', '')
                if 'Research' in name:
                    print(f"File {path}: GameObject {name}")
    except Exception as e:
        pass

