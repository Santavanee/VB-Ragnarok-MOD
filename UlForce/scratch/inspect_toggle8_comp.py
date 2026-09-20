import UnityPy

env = UnityPy.load('E:/GameH/VenusBlood RAGNAROK International (US)/VBRI_Data/level62')

for obj in env.objects:
    if obj.type.name == 'GameObject':
        data = obj.read()
        name = getattr(data, 'm_Name', '')
        if name == 'Toggle8':
            print("Found Toggle8!")
            for c in data.m_Components:
                try:
                    c_obj = c.read()
                    print("  Component type:", c.type.name)
                    # print some fields
                    for k in dir(c_obj):
                        if not k.startswith('_') and k in ['m_Name', 'm_Interactable', 'm_IsOn']:
                            print(f"    {k} = {getattr(c_obj, k, None)}")
                except Exception as ex:
                    print("  Component read error:", ex)

