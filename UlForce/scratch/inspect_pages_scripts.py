import UnityPy

env = UnityPy.load('E:/GameH/VenusBlood RAGNAROK International (US)/VBRI_Data/level62')

for obj in env.objects:
    if obj.type.name == 'GameObject':
        data = obj.read()
        name = getattr(data, 'm_Name', '')
        if name == 'pages':
            print("pages MonoBehaviour scripts:")
            for c in data.m_Components:
                if c.type.name == 'MonoBehaviour':
                    mb = c.read()
                    script = mb.m_Script.read()
                    print(" ", script.m_Name)

