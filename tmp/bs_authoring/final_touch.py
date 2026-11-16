from docx import Document
from docx.shared import Pt
from pathlib import Path
import re
base=Path(r'C:\Users\Administrator\Desktop\bs')
p=base/'本科毕业论文(设计)开题报告.docx'; d=Document(p)
for table in d.tables:
    for row in table.rows:
        for cell in row.cells:
            for para in cell.paragraphs:
                m=re.match(r'^([^\[]*)',para.text)
                # In this template every bibliography entry is inside the final table and includes DOI or a URL.
                if ('DOI:' in para.text or 'docs.unity3d.com' in para.text) and para.text.strip() and not re.match(r'^\[\d+\]',para.text.strip()):
                    idx=sum(1 for t in d.tables for r in t.rows for c in r.cells for q in c.paragraphs if q._p is para)
                    # position is assigned below using the existing entry order in the table.
                    pass
# Restore one visible, typed number per bibliography paragraph in order.
n=0
for table in d.tables:
    for row in table.rows:
        for cell in row.cells:
            for para in cell.paragraphs:
                if para.text.strip() and ('DOI:' in para.text or 'docs.unity3d.com' in para.text):
                    n+=1
                    if n<=16 and not re.match(r'^\[\d+\]',para.text.strip()):
                        first=next((r for r in para.runs if r.text),None)
                        if first: first.text=f'[{n}] '+first.text
                    elif re.match(r'^\[\d+\]',para.text.strip()):
                        continue
if n!=16: raise RuntimeError(f'Expected 16 entries, got {n}')
d.save(p)
p=base/'本科毕业论文(设计)文献综述.docx'; d=Document(p); inside=False
for para in d.paragraphs:
    if para.text.strip()=='参考文献': inside=True; continue
    if inside and re.match(r'^\[\d+\]',para.text.strip()):
        for run in para.runs: run.font.size=Pt(9.5)
d.save(p)
