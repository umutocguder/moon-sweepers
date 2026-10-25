/**
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 * Licensed under 11Hopem Proprietary License.
 */

export class TelemetryService {
  static FIREBASE_PROJECT_ID = 'hopem-software-cloud';
  static BASE_URL = `https://firestore.googleapis.com/v1/projects/${this.FIREBASE_PROJECT_ID}/databases/(default)/documents`;

  static async logEvent(collection, eventName, metadata = {}) {
    const body = {
      fields: {
        appName: { stringValue: 'moon-sweepers' },
        eventName: { stringValue: eventName },
        timestamp: { timestampValue: new Date().toISOString() },
        metadata: { stringValue: JSON.stringify(metadata) }
      }
    };

    try {
      if (typeof fetch === 'function') {
        fetch(`${this.BASE_URL}/${collection}`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(body)
        }).catch(() => {
          // Non-blocking failover
        });
      }
    } catch {
      // Non-blocking
    }
  }
}
