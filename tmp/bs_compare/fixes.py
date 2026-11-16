from docx import Document
from docx.enum.section import WD_SECTION_START
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.shared import Pt
from docx.oxml.ns import qn
from pathlib import Path
import re
out=Path(r'D:\Emo\Proj\SBR\tmp\bs_compare')
base=Path(r'C:\Users\Administrator\Desktop\bs')
# Taskbook: follow the template's continuous transition and prevent restarted auto list numbers.
p=base/'本科毕业论文(设计)任务书.docx'; d=Document(p)
d.sections[1].start_type=WD_SECTION_START.CONTINUOUS
for t in d.tables:
 for row in t.rows:
  for cell in row.cells:
   for q in cell.paragraphs:
    if re.match(r'^\[\d+\]',q.text.strip()):
     ppr=q._p.get_or_add_pPr(); num=ppr.find(qn('w:numPr'))
     if num is not None:ppr.remove(num)
d.save(out/'task_fixed.docx')
# Proposal: put the large inline flowchart in an auto-height centered paragraph so it cannot cover adjacent lines.
p=base/'本科毕业论文(设计)开题报告.docx'; d=Document(p)
for t in d.tables:
 for row in t.rows:
  for cell in row.cells:
   for q in cell.paragraphs:
    if q._p.xpath('.//w:drawing') and q._p.xpath('.//wp:inline'):
     q.alignment=WD_ALIGN_PARAGRAPH.CENTER
     q.paragraph_format.line_spacing=1.0
     q.paragraph_format.space_before=Pt(0)
     q.paragraph_format.space_after=Pt(6)
d.save(out/'report_fixed.docx')
# Translation: restore the backup's order: cover title and student-information table before translation content.
p=base/'本科毕业论文(设计)外文翻译.docx'; d=Document(p)
tbl=d.tables[0]._tbl; body=d.element.body; body.remove(tbl); d.paragraphs[0]._p.addnext(tbl)
d.save(out/'translation_fixed.docx')
