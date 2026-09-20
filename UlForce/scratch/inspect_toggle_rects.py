import UnityPy

env = UnityPy.load('E:/GameH/VenusBlood RAGNAROK International (US)/VBRI_Data/level62')

for obj in env.objects:
    if obj.type.name == 'GameObject':
        data = obj.read()
        name = getattr(data, 'm_Name', '')
        if name.startswith('Toggle') and name[6:].isdigit():
            for c in data.m_Components:
                if c.type.name in ['RectTransform', 'Transform']:
                    try:
                        raw = c.get_raw_data()
                        # print object info or let's read typetree
                        # or check typetree
                    except:
                        pass

