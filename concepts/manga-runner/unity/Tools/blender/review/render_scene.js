const { chromium } = require('playwright');
(async () => {
  const browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader', '--enable-unsafe-swiftshader', '--ignore-gpu-blocklist'] });
  for (const shot of ['run', 'jump']) {
    const page = await browser.newPage({ viewport: { width: 1080, height: 1920 } });
    const errs = []; page.on('console', m => { if (m.type() === 'error') errs.push(m.text()); }); page.on('pageerror', e => errs.push(e.message));
    await page.goto(`http://127.0.0.1:8765/scene.html?shot=${shot}`);
    await page.waitForFunction(() => window.__done, null, { timeout: 180000 });
    await page.screenshot({ path: `out/scene-${shot}.png` });
    console.log(shot, 'errors:', errs.length ? errs.join(' | ') : 'none');
    await page.close();
  }
  await browser.close();
})().catch(e => { console.error('FAILED', e); process.exit(1); });
