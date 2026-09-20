import UnityPy

env = UnityPy.load('E:/GameH/VenusBlood RAGNAROK International (US)/VBRI_Data/level62')

for obj in env.objects:
    if obj.type.name == 'GameObject':
        data = obj.read()
        name = getattr(data, 'm_Name', '')
        components = []
        for c in data.m_Components:
            try:
                c_data = c.read()
                components.append(c_data.type.name)
            except:
                pass
        print(f"GameObject: {name} | Components: {', '.join(components)}")

