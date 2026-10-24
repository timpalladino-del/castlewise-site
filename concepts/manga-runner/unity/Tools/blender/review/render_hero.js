const { chromium } = require('playwright');
const views = [['Hero', 2.6, 0.2, '', 'h-back'], ['Hero', 0.55, 0.2, '', 'h-front'], ['Hero', 1.57, 0.15, '', 'h-side'], ['Hero', 2.7, 0.25, 'run', 'h-run-back'], ['Hero', 0.5, 0.25, 'run', 'h-run-front'], ['Hero', 2.6, 0.3, 'jump', 'h-jump']];
(async () => {
  const browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader', '--enable-unsafe-swiftshader', '--ignore-gpu-blocklist'] });
  const page = await browser.newPage({ viewport: { width: 800, height: 800 } });
  const errs = []; page.on('console', m => { if (m.type() === 'error') errs.push(m.text()); }); page.on('pageerror', e => errs.push(e.message));
  for (const [m, yaw, pitch, pose, name] of views) {
    await page.goto(`http://127.0.0.1:8765/viewer.html?m=${m}.glb&yaw=${yaw}&pitch=${pitch}&pose=${pose}&zoom=1.15`);
    await page.waitForFunction(() => window.__done, null, { timeout: 60000 });
    await page.screenshot({ path: `out/${name}.png` });
  }
  console.log('errors:', errs.length ? errs.join(' | ') : 'none');
  await browser.close();
})().catch(e => { console.error('FAILED', e); process.exit(1); });
