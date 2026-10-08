from pathlib import Path
import sys,json
import pypdfium2 as pdf
from PIL import Image,ImageDraw
root=Path(r'D:\Đồ Án\doi_chieu_quy_dinh')
version=sys.argv[1] if len(sys.argv)>1 else 'v1'
d=pdf.PdfDocument(root/f'report_{version}.pdf'); dest=root/f'qa_{version}';dest.mkdir(exist_ok=True)
summary=[]
for i,page in enumerate(d):
    tp=page.get_textpage();text=tp.get_text_range();(dest/f'page-{i+1}.txt').write_text(text,encoding='utf8')
    im=page.render(scale=1.4).to_pil();im.save(dest/f'page-{i+1}.png')
    lines=[x.strip() for x in text.splitlines() if x.strip()]
    summary.append({'page':i+1,'chars':len(text),'start':' | '.join(lines[:3]),'end':' | '.join(lines[-2:])})
for start in range(0,len(d),4):
    sheet=Image.new('RGB',(1200,1730),'#d0d0d0');dr=ImageDraw.Draw(sheet)
    for i in range(start,min(start+4,len(d))):
        im=Image.open(dest/f'page-{i+1}.png');im.thumbnail((590,835));x=(i-start)%2*600;y=(i-start)//2*865;sheet.paste(im,(x+5,y+24));dr.text((x+10,y+5),str(i+1),fill='black')
    sheet.save(dest/f'sheet-{start+1}.png')
(dest/'summary.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps(summary,ensure_ascii=False))
