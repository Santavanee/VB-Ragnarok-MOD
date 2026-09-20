import UnityPy

env = UnityPy.load('E:/GameH/VenusBlood RAGNAROK International (US)/VBRI_Data/level62')

for obj in env.objects:
    if obj.type.name == 'GameObject':
        data = obj.read()
        name = getattr(data, 'm_Name', '')
        if name in ['Toggle6', 'Toggle7', 'Toggle8']:
            for c in data.m_Components:
                try:
                    c_data = c.read()
                    if c.type.name in ['Transform', 'RectTransform']:
                        for ch in getattr(c_data, 'm_Children', []):
                            ch_go = ch.read().m_GameObject.read()
                            for cc in ch_go.m_Components:
                                try:
                                    cc_data = cc.read()
                                    txt = getattr(cc_data, 'm_Text', None)
                                    if txt is not None:
                                        print(f"{name} -> {ch_go.m_Name} text: {repr(txt)}")
                                except:
                                    pass
                except:
                    pass

