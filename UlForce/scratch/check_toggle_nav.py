import UnityPy

env = UnityPy.load('E:/GameH/VenusBlood RAGNAROK International (US)/VBRI_Data/level62')

for obj in env.objects:
    if obj.type.name == 'GameObject':
        data = obj.read()
        name = getattr(data, 'm_Name', '')
        if name in ['Toggle6', 'Toggle7', 'Toggle8']:
            for c in data.m_Components:
                if c.type.name == 'MonoBehaviour':
                    try:
                        c_data = c.read()
                        script = c_data.m_Script.read()
                        if script.m_Name == 'Toggle':
                            print(f"{name} Toggle found")
                    except:
                        pass

