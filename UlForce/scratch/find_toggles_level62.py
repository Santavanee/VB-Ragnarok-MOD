import UnityPy

env = UnityPy.load('E:/GameH/VenusBlood RAGNAROK International (US)/VBRI_Data/level62')

for obj in env.objects:
    if obj.type.name == 'GameObject':
        data = obj.read()
        name = getattr(data, 'm_Name', '')
        if 'toggle' in name.lower() or 'page' in name.lower():
            print(f"GameObject: {name} (active: {getattr(data, 'm_IsActive', True)})")
            for c in data.m_Components:
                try:
                    c_data = c.read()
                    print(f"   Component: {c_data.type.name}")
                except Exception as e:
                    print(f"   Component error: {e}")

