const { chromium } = require('playwright');
const views = [
  ['Hero', 2.6, 0.25, 'hero-back'], ['Hero', 0.6, 0.25, 'hero-front'], ['Train', 3.6, 0.3, 'train'], ['Barrier', 2.6, 0.4, 'barrier'], ['Sign', 2.6, 0.3, 'sign'],
  ['BuildingCity', -2.3, 0.35, 'building-city'], ['Containers', -2.3, 0.4, 'containers'], ['BuildingAlley', -2.3, 0.3, 'building-alley'],
  ['Coin', 2.7, 0.2, 'coin'], ['Magnet', 2.7, 0.2, 'magnet'], ['Dash', 2.7, 0.2, 'dash'], ['Lantern', 2.7, 0.3, 'lantern'],
];
(async () => {
  const browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader', '--enable-unsafe-swiftshader', '--ignore-gpu-blocklist'] });
  const page = await browser.newPage({ viewport: { width: 700, height: 700 } });
  const errs = []; page.on('console', m => { if (m.type() === 'error') errs.push(m.text()); }); page.on('pageerror', e => errs.push(e.message));
  for (const [m, yaw, pitch, name] of views) {
    await page.goto(`http://127.0.0.1:8765/viewer.html?m=${m}.glb&yaw=${yaw}&pitch=${pitch}`);
    await page.waitForFunction(() => window.__done, null, { timeout: 30000 });
    await page.screenshot({ path: `out/${name}.png` });
  }
  console.log('errors:', errs.length ? errs.join(' | ') : 'none');
  await browser.close();
})().catch(e => { console.error('FAILED', e); process.exit(1); });
