import UnityPy

env = UnityPy.load('E:/GameH/VenusBlood RAGNAROK International (US)/VBRI_Data/level62')

for obj in env.objects:
    if obj.type.name == 'GameObject':
        data = obj.read()
        name = getattr(data, 'm_Name', '')
        if name.startswith('Toggle') and len(name) == 7 and name[6].isdigit():
            for c in data.m_Components:
                if c.type.name == 'RectTransform':
                    try:
                        c_obj = c.read()
                        raw = c_obj.get_raw_data()
                        tree = c_obj.read_typetree()
                        print(f"{name}: pos={tree.get('m_AnchoredPosition')} size={tree.get('m_SizeDelta')}")
                    except Exception as e:
                        pass
