import UnityPy

env = UnityPy.load('E:/GameH/VenusBlood RAGNAROK International (US)/VBRI_Data/level62')

for obj in env.objects:
    if obj.type.name == 'GameObject':
        data = obj.read()
        name = getattr(data, 'm_Name', '')
        if name == 'pages':
            print("Found 'pages' GameObject!")
            for c in data.m_Components:
                try:
                    c_data = c.read()
                    if c.type.name in ['Transform', 'RectTransform']:
                        children = getattr(c_data, 'm_Children', [])
                        print(f"Children count: {len(children)}")
                        for ch in children:
                            ch_trans = ch.read()
                            ch_go = ch_trans.m_GameObject.read()
                            print(f"  Child: {ch_go.m_Name} (active={ch_go.m_IsActive})")
                except Exception as ex:
                    print("Error:", ex)
