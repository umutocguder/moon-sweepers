/**
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 * Licensed under 11Hopem Proprietary License.
 */

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const ROOT_DIR = path.resolve(__dirname, '..');
const PUBLIC_DIR = path.join(ROOT_DIR, 'public');

function copyRecursive(src, dest) {
  if (!fs.existsSync(src)) return;
  const stat = fs.statSync(src);
  if (stat.isDirectory()) {
    if (!fs.existsSync(dest)) fs.mkdirSync(dest, { recursive: true });
    for (const item of fs.readdirSync(src)) {
      copyRecursive(path.join(src, item), path.join(dest, item));
    }
  } else {
    fs.copyFileSync(src, dest);
  }
}

console.log('[Build] Synchronizing moon-sweepers into public/ directory...');
if (!fs.existsSync(PUBLIC_DIR)) {
  fs.mkdirSync(PUBLIC_DIR, { recursive: true });
}

// Copy game sources and assets to public
copyRecursive(path.join(ROOT_DIR, 'src'), path.join(PUBLIC_DIR, 'src'));
copyRecursive(path.join(ROOT_DIR, 'assets'), path.join(PUBLIC_DIR, 'assets'));

for (const fav of ['favicon.ico', 'favicon.svg', 'apple-touch-icon.png', 'og-image.png', 'logo.svg']) {
  const srcFav = path.join(ROOT_DIR, 'assets', fav);
  const destFav = path.join(PUBLIC_DIR, fav);
  if (fs.existsSync(srcFav)) {
    fs.copyFileSync(srcFav, destFav);
  }
}

console.log('[Build] Build complete! Public directory is production-ready.');
