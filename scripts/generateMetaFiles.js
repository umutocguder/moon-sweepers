import fs from 'fs';
import path from 'path';
import crypto from 'crypto';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

function generateGuid(filePath) {
    return crypto.createHash('md5').update('11hopem_' + filePath).digest('hex');
}

function processDirectory(dirPath) {
    const entries = fs.readdirSync(dirPath, { withFileTypes: true });

    for (const entry of entries) {
        if (entry.name.endsWith('.meta') || entry.name === '.DS_Store') continue;

        const fullPath = path.join(dirPath, entry.name);
        const metaPath = `${fullPath}.meta`;

        if (!fs.existsSync(metaPath)) {
            const relPath = path.relative(process.cwd(), fullPath);
            const guid = generateGuid(relPath);

            let metaContent = '';
            if (entry.isDirectory()) {
                metaContent = `fileFormatVersion: 2\nguid: ${guid}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n`;
            } else if (entry.name.endsWith('.cs')) {
                metaContent = `fileFormatVersion: 2\nguid: ${guid}\nMonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n`;
            } else {
                metaContent = `fileFormatVersion: 2\nguid: ${guid}\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n`;
            }

            fs.writeFileSync(metaPath, metaContent, 'utf8');
            console.log(`Generated meta for: ${relPath}`);
        }

        if (entry.isDirectory()) {
            processDirectory(fullPath);
        }
    }
}

const assetsDir = path.join(__dirname, '..', 'Assets');
if (fs.existsSync(assetsDir)) {
    processDirectory(assetsDir);
}
console.log('Unity .meta synchronization complete!');
