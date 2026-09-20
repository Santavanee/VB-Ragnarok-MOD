import UnityPy

env = UnityPy.load('E:/GameH/VenusBlood RAGNAROK International (US)/VBRI_Data/level62')

for obj in env.objects:
    if obj.type.name == 'GameObject':
        data = obj.read()
        name = getattr(data, 'm_Name', '')
        if name in ['Toggle6', 'Toggle7', 'Toggle8']:
            print(f"=== {name} ===")
            for c in data.m_Components:
                try:
                    c_data = c.read()
                    if c.type.name in ['Transform', 'RectTransform']:
                        children = getattr(c_data, 'm_Children', [])
                        for ch in children:
                            ch_go = ch.read().m_GameObject.read()
                            print(f"  Child: {ch_go.m_Name}")
                            # check components of child
                            for cc in ch_go.m_Components:
                                try:
                                    cc_data = cc.read()
                                    if cc.type.name == 'Text':
                                        print(f"    Text: {getattr(cc_data, 'm_Text', '')}")
                                except:
                                    pass
                except Exception as ex:
                    pass

