from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import SimpleDocTemplate,Paragraph,Spacer,Table,TableStyle
from reportlab.lib import colors
from reportlab.lib.enums import TA_RIGHT,TA_LEFT
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.units import mm
from xml.sax.saxutils import escape
import re
# Unicode Arabic shaping and bidi implementation, using contextual presentation forms in the embedded font.
forms={}
def add(ch,iso,fin=None,init=None,med=None): forms[ord(ch)]=(iso,fin,init,med)
# Arabic alphabet contextual forms: isolated/final/initial/medial.
data={
'ء':(0xfe80,), 'آ':(0xfe81,0xfe82), 'أ':(0xfe83,0xfe84), 'ؤ':(0xfe85,0xfe86), 'إ':(0xfe87,0xfe88), 'ئ':(0xfe89,0xfe8a,0xfe8b,0xfe8c),
'ا':(0xfe8d,0xfe8e),'ب':(0xfe8f,0xfe90,0xfe91,0xfe92),'ة':(0xfe93,0xfe94),'ت':(0xfe95,0xfe96,0xfe97,0xfe98),'ث':(0xfe99,0xfe9a,0xfe9b,0xfe9c),'ج':(0xfe9d,0xfe9e,0xfe9f,0xfea0),'ح':(0xfea1,0xfea2,0xfea3,0xfea4),'خ':(0xfea5,0xfea6,0xfea7,0xfea8),'د':(0xfea9,0xfeaa),'ذ':(0xfeab,0xfeac),'ر':(0xfead,0xfeae),'ز':(0xfeaf,0xfeb0),'س':(0xfeb1,0xfeb2,0xfeb3,0xfeb4),'ش':(0xfeb5,0xfeb6,0xfeb7,0xfeb8),'ص':(0xfeb9,0xfeba,0xfebb,0xfebc),'ض':(0xfebd,0xfebe,0xfebf,0xfec0),'ط':(0xfec1,0xfec2,0xfec3,0xfec4),'ظ':(0xfec5,0xfec6,0xfec7,0xfec8),'ع':(0xfec9,0xfeca,0xfecb,0xfecc),'غ':(0xfecd,0xfece,0xfecf,0xfed0),'ف':(0xfed1,0xfed2,0xfed3,0xfed4),'ق':(0xfed5,0xfed6,0xfed7,0xfed8),'ك':(0xfed9,0xfeda,0xfedb,0xfedc),'ل':(0xfedd,0xfede,0xfedf,0xfee0),'م':(0xfee1,0xfee2,0xfee3,0xfee4),'ن':(0xfee5,0xfee6,0xfee7,0xfee8),'ه':(0xfee9,0xfeea,0xfeeb,0xfeec),'و':(0xfeed,0xfeee),'ى':(0xfeef,0xfef0),'ي':(0xfef1,0xfef2,0xfef3,0xfef4),
'پ':(0xfb56,0xfb57,0xfb58,0xfb59),'چ':(0xfb7a,0xfb7b,0xfb7c,0xfb7d),'ژ':(0xfb8a,0xfb8b),'گ':(0xfb92,0xfb93,0xfb94,0xfb95),'ک':(0xfb8e,0xfb8f,0xfb90,0xfb91),'ی':(0xfbfc,0xfbfd,0xfbfe,0xfbff),'ۀ':(0x06c0,0x06c0)}
for c,x in data.items(): forms[ord(c)]=x
def joinprev(c): return ord(c) in forms and len(forms[ord(c)])>=4
def joinnext(c): return ord(c) in forms and len(forms[ord(c)])>=2
def shape_ar(s):
 a=list(s); result=[]; i=0
 while i<len(a):
  # Arabic lam-alef ligatures
  if a[i]=='ل' and i+1<len(a) and a[i+1] in 'اآأإ':
   nxt=a[i+1]; lig={'ا':(0xfefb,0xfefc),'آ':(0xfef5,0xfef6),'أ':(0xfef7,0xfef8),'إ':(0xfef9,0xfefa)}[nxt]
   prev=next((a[j] for j in range(i-1,-1,-1) if ord(a[j]) in forms), '')
   result.append(chr(lig[1] if prev and joinprev(prev) else lig[0])); i+=2; continue
  ch=a[i]; f=forms.get(ord(ch))
  if not f: result.append(ch);i+=1;continue
  prev=next((a[j] for j in range(i-1,-1,-1) if ord(a[j]) in forms), '')
  nex=next((a[j] for j in range(i+1,len(a)) if ord(a[j]) in forms), '')
  jp=bool(prev and joinnext(prev) and joinprev(ch)); jn=bool(nex and joinnext(ch) and joinprev(nex))
  if jp and jn and len(f)>3: form=f[3]
  elif jp: form=f[1]
  elif jn and len(f)>2: form=f[2]
  else: form=f[0]
  result.append(chr(form));i+=1
 return ''.join(result)
# Put logical RTL Arabic runs into visual order; preserve Latin technical tokens in their original readable order.
rx=re.compile(r'([A-Za-z0-9][A-Za-z0-9 ._+#/&-]*[A-Za-z0-9+#])')
def bidi(s):
 parts=rx.split(s); out=[]
 for part in parts:
  if not part:continue
  if rx.fullmatch(part): out.append(part)
  else:
   shaped=shape_ar(part)
   out.append(shaped[::-1])
 return ''.join(out)
def fa(s): return bidi(s)
def mixed(s): return bidi(s)
base='resume'
pdfmetrics.registerFont(TTFont('F','C:/Windows/Fonts/tahoma.ttf'))
pdfmetrics.registerFont(TTFont('FB','C:/Windows/Fonts/tahomabd.ttf'))
pdfmetrics.registerFontFamily('F',normal='F',bold='FB',italic='F',boldItalic='FB')
ink=colors.HexColor('#182b3c'); accent=colors.HexColor('#087e83'); muted=colors.HexColor('#546577'); line=colors.HexColor('#dce6e9')
S={
'name':ParagraphStyle('name',fontName='FB',fontSize=23,leading=27,textColor=ink,alignment=TA_RIGHT),
'role':ParagraphStyle('role',fontName='FB',fontSize=11,leading=14,textColor=accent,alignment=TA_RIGHT),
'focus':ParagraphStyle('focus',fontName='F',fontSize=8,leading=11,textColor=muted,alignment=TA_RIGHT),
'contact':ParagraphStyle('contact',fontName='F',fontSize=7.3,leading=10,textColor=muted,alignment=TA_RIGHT),
'head':ParagraphStyle('head',fontName='FB',fontSize=9.4,leading=12,textColor=accent,alignment=TA_RIGHT),
'body':ParagraphStyle('body',fontName='F',fontSize=8.2,leading=12,textColor=ink,alignment=TA_RIGHT),
'small':ParagraphStyle('small',fontName='F',fontSize=7.7,leading=10.5,textColor=ink,alignment=TA_RIGHT),
'bullet':ParagraphStyle('bullet',fontName='F',fontSize=7.8,leading=10.7,textColor=ink,alignment=TA_RIGHT,rightIndent=8,firstLineIndent=-7,spaceAfter=0.5),
'skill':ParagraphStyle('skill',fontName='F',fontSize=7.5,leading=10.2,textColor=ink,alignment=TA_RIGHT),
'title':ParagraphStyle('title',fontName='FB',fontSize=8.8,leading=11,textColor=ink,alignment=TA_RIGHT),
'stack':ParagraphStyle('stack',fontName='F',fontSize=6.5,leading=9,textColor=muted,alignment=TA_LEFT),
'note':ParagraphStyle('note',fontName='F',fontSize=7,leading=9,textColor=colors.HexColor('#37565d'),alignment=TA_RIGHT)}
def P(s,style):return Paragraph(escape(s),S[style])
def sec(title,content,width):
 t=Table([[P(title,'head')],[content]],colWidths=[width]);t.setStyle(TableStyle([('LINEBEFORE',(0,0),(0,0),1.8,accent),('LEFTPADDING',(0,0),(-1,-1),6),('RIGHTPADDING',(0,0),(-1,-1),5),('TOPPADDING',(0,0),(-1,0),0),('BOTTOMPADDING',(0,0),(-1,0),2),('TOPPADDING',(0,1),(-1,1),0),('BOTTOMPADDING',(0,1),(-1,-1),0)]));return t
W=178*mm; story=[]
right=Table([[P('موبایل: [شماره تماس]','contact'),P('ایمیل: [Email]','contact')],[P('GitHub: [GitHub URL]','contact'),P('LinkedIn: [LinkedIn URL]','contact')],[P('محل سکونت: [شهر]','contact'),P('وضعیت همکاری: [قابل تکمیل]','contact')]],colWidths=[51*mm,51*mm]);right.setStyle(TableStyle([('VALIGN',(0,0),(-1,-1),'MIDDLE'),('LEFTPADDING',(0,0),(-1,-1),2),('RIGHTPADDING',(0,0),(-1,-1),2),('TOPPADDING',(0,0),(-1,-1),1),('BOTTOMPADDING',(0,0),(-1,-1),1)]))
intro=Table([[P('محمد','name'),right],[P('برنامه‌نویس بک‌اند جونیور','role'),P('Junior Back-End Developer | C# & ASP.NET Core','focus')]],colWidths=[72*mm,106*mm]);intro.setStyle(TableStyle([('VALIGN',(0,0),(-1,-1),'MIDDLE'),('LEFTPADDING',(0,0),(-1,-1),0),('RIGHTPADDING',(0,0),(-1,-1),0),('TOPPADDING',(0,0),(-1,-1),1),('BOTTOMPADDING',(0,0),(-1,-1),2),('LINEBELOW',(0,-1),(-1,-1),1.5,accent)]));story += [intro,Spacer(1,5)]
summary='دانشجوی دانشگاه و برنامه‌نویس بک‌اند جونیور با تمرکز بر C# و ASP.NET Core MVC. دارای تجربه عملی در توسعه اجزای برنامه‌های وب، اتصال برنامه به پایگاه داده با Entity Framework Core و SQL Server و پیاده‌سازی زیرساخت احراز هویت با ASP.NET Core Identity. در پروژه تیمی PeakLabs در توسعه بک‌اند، سازمان‌دهی ساختار پروژه و همکاری با Git و GitHub مشارکت دارم.'
story += [sec('خلاصه حرفه‌ای',P(summary,'body'),W),Spacer(1,5)]
skills=[('زبان برنامه‌نویسی: ','C#'),('بک‌اند: ','ASP.NET Core MVC، ASP.NET Core Identity'),('داده و ORM: ','Entity Framework Core، LINQ'),('پایگاه داده: ','SQL Server، Code First، Migrations'),('فرانت‌اند: ','HTML، CSS، JavaScript، Razor'),('ابزارها و کتابخانه‌ها: ','Bootstrap، Axios، Visual Studio Code'),('کنترل نسخه و پیگیری کار: ','Git، GitHub، Trello')]
rows=[]
for i in range(0,len(skills),2):
 row=[P(k+v,'skill') for k,v in skills[i:i+2]]
 if len(row)<2:row.append(P('','skill'))
 rows.append(row)
st=Table(rows,colWidths=[W/2,W/2]);st.setStyle(TableStyle([('VALIGN',(0,0),(-1,-1),'MIDDLE'),('LINEBELOW',(0,0),(-1,-1),.35,line),('LEFTPADDING',(0,0),(-1,-1),5),('RIGHTPADDING',(0,0),(-1,-1),5),('TOPPADDING',(0,0),(-1,-1),3),('BOTTOMPADDING',(0,0),(-1,-1),3)]));story += [sec('مهارت‌های فنی',st,W),Spacer(1,5)]
ph=Table([[P('PeakLabs | وب‌سایت شرکتی و سامانه مدیریت محتوا','title'),P('ASP.NET Core MVC · C# · EF Core · SQL Server · Identity','stack')]],colWidths=[94*mm,84*mm]);ph.setStyle(TableStyle([('VALIGN',(0,0),(-1,-1),'MIDDLE'),('LEFTPADDING',(0,0),(-1,-1),0),('RIGHTPADDING',(0,0),(-1,-1),0),('TOPPADDING',(0,0),(-1,-1),1),('BOTTOMPADDING',(0,0),(-1,-1),1)]))
items=['مشارکت در توسعه بک‌اند، ساختاردهی بخش‌های پروژه و اتصال به پایگاه داده.','همکاری در پیاده‌سازی زیرساخت احراز هویت با ASP.NET Core Identity.','کار با Entity Framework Core، مدیریت Migrations و SQL Server.','مشارکت در ساختار فرانت‌اند و پیاده‌سازی رابط کاربری واکنش‌گرا.','هماهنگی وظایف تیمی و همکاری از طریق شاخه‌ها و Pull Requestها در GitHub.']
pc=[ph,P('پروژه نرم‌افزاری تیمی برای توسعه وب‌سایت شرکت و بستر مدیریت محتوای آن.','small')]+[P('• '+x,'bullet') for x in items]
story += [sec('پروژه',pc,W),Spacer(1,5)]
collab=Table([[P('• برنامه‌ریزی و هماهنگی وظایف پروژه','bullet')],[P('• همکاری تیمی مبتنی بر Git','bullet')],[P('• مشارکت در بازبینی کد و حل تعارض‌های ادغام','bullet')],[P('• سازمان‌دهی ساختار پروژه و اجزای نرم‌افزار','bullet')]],colWidths=[85*mm]);collab.setStyle(TableStyle([('LEFTPADDING',(0,0),(-1,-1),0),('RIGHTPADDING',(0,0),(-1,-1),0),('TOPPADDING',(0,0),(-1,-1),0),('BOTTOMPADDING',(0,0),(-1,-1),0)]))
edu=[P('تحصیلات: دانشجوی دانشگاه','small'),P('رشته / مقطع: [رشته و مقطع تحصیلی]','small'),P('دانشگاه: [نام دانشگاه]','small'),P('فارسی: زبان مادری | انگلیسی: مکالمه‌ای (خودارزیابی)','small')]
bottom=Table([[sec('همکاری تیمی',collab,85*mm),sec('تحصیلات و زبان‌ها',edu,85*mm)]],colWidths=[89*mm,89*mm]);bottom.setStyle(TableStyle([('VALIGN',(0,0),(-1,-1),'TOP'),('LEFTPADDING',(0,0),(-1,-1),0),('RIGHTPADDING',(0,0),(-1,-1),4),('TOPPADDING',(0,0),(-1,-1),0),('BOTTOMPADDING',(0,0),(-1,-1),0)]));story += [bottom,Spacer(1,4)]
note=Table([[P('اطلاعات داخل کروشه را پیش از ارسال رزومه با اطلاعات واقعی تکمیل کنید.','note')]],colWidths=[W]);note.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,-1),colors.HexColor('#eef6f5')),('LEFTPADDING',(0,0),(-1,-1),7),('RIGHTPADDING',(0,0),(-1,-1),7),('TOPPADDING',(0,0),(-1,-1),4),('BOTTOMPADDING',(0,0),(-1,-1),4)]));story.append(note)
doc=SimpleDocTemplate('resume/resume-fa.pdf',pagesize=A4,rightMargin=16*mm,leftMargin=16*mm,topMargin=10*mm,bottomMargin=9*mm,title='رزومه فارسی محمد | برنامه‌نویس بک‌اند جونیور',author='محمد',pageCompression=1)
doc.build(story)
print('resume/resume-fa.pdf created')






