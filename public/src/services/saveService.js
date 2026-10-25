/**
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 * Licensed under 11Hopem Proprietary License.
 */

import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';

export class SaveService {
  static STORAGE_KEY = '11hopem_moon_sweepers_save';

  /**
   * Save game state to file with cryptographic SHA-256 checksum
   */
  static saveToFile(gameState, dir = path.resolve(process.cwd(), 'data/backups')) {
    if (!fs.existsSync(dir)) {
      fs.mkdirSync(dir, { recursive: true });
    }

    const payload = JSON.stringify(gameState, null, 2);
    const checksum = crypto.createHash('sha256').update(payload, 'utf8').digest('hex');
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    const filename = `moon-save-${timestamp}.json`;
    const targetPath = path.join(dir, filename);

    const record = {
      timestamp: new Date().toISOString(),
      game: 'Moon Sweepers (11Hopem)',
      version: '1.0.0',
      checksum,
      state: gameState
    };

    fs.writeFileSync(targetPath, JSON.stringify(record, null, 2), 'utf8');
    return { success: true, targetPath, checksum };
  }

  /**
   * Verify integrity of a save file
   */
  static verifySaveFile(filePath) {
    if (!fs.existsSync(filePath)) return false;
    try {
      const content = fs.readFileSync(filePath, 'utf8');
      const parsed = JSON.parse(content);
      const computedHash = crypto.createHash('sha256')
        .update(JSON.stringify(parsed.state, null, 2), 'utf8')
        .digest('hex');
      return computedHash === parsed.checksum;
    } catch {
      return false;
    }
  }
}
