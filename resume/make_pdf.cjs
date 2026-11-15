const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');
(async()=>{
 const browser=await chromium.launch({headless:true});
 const page=await browser.newPage({viewport:{width:1240,height:1754},deviceScaleFactor:1});
 const html=fs.readFileSync(path.resolve('resume/resume-fa.html'),'utf8');
 await page.setContent(html,{waitUntil:'networkidle'});
 await page.evaluate(async()=>{await document.fonts.ready});
 await page.screenshot({path:path.resolve('resume/preview-playwright.png'),fullPage:true});
 await page.pdf({path:path.resolve('resume/resume-fa.pdf'),format:'A4',printBackground:true,preferCSSPageSize:true,margin:{top:'0',right:'0',bottom:'0',left:'0'}});
 console.log('page count approx',await page.locator('.sheet').count());
 await browser.close();
})().catch(e=>{console.error(e);process.exit(1)});
