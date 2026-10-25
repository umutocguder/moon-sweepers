/**
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 * Licensed under 11Hopem Proprietary License.
 */

import { SaveService } from '../src/services/saveService.js';

console.log('[Backup] Running autonomous Moon Sweepers game backup...');
const demoState = {
  credits: 1000,
  tool: 'CYCLONE_VACUUM',
  totalCleaned: 500,
  timestamp: new Date().toISOString()
};

const result = SaveService.saveToFile(demoState);
console.log(`[Backup] Completed successfully. Hash: ${result.checksum.substring(0, 16)}...`);
