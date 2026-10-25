import fs from 'fs';
import path from 'path';
import { execSync } from 'child_process';

const unityHubPath = '/Applications/Unity Hub.app';
const standardEditorDir = '/Applications/Unity/Hub/Editor';

console.log('=== 11Hopem Moon Sweepers - macOS Standalone Build Runner ===');

let unityBinary = null;

if (fs.existsSync(standardEditorDir)) {
  const versions = fs.readdirSync(standardEditorDir);
  for (const v of versions) {
    const candidate = path.join(standardEditorDir, v, 'Unity.app', 'Contents', 'MacOS', 'Unity');
    if (fs.existsSync(candidate)) {
      unityBinary = candidate;
      console.log(`Detected Unity Editor: ${v} at ${candidate}`);
      break;
    }
  }
}

if (!unityBinary) {
  try {
    const whichUnity = execSync('which unity', { encoding: 'utf8' }).trim();
    if (whichUnity) unityBinary = whichUnity;
  } catch (e) {
    // not in PATH
  }
}

if (!unityBinary) {
  console.log('\n[INFO] Unity Editor is not yet linked in PATH or /Applications/Unity/Hub/Editor/.');
  console.log('[INFO] Unity Hub is installed at: ' + unityHubPath);
  console.log('\nTo build or play on Mac:');
  console.log('  1. Open Unity Hub: `npm run open:unity` or launch Unity Hub from Applications.');
  console.log('  2. Click "Installs" -> Install Editor (Unity 6 or Unity 2022.3 LTS recommended).');
  console.log('  3. In Unity Hub "Projects", click "Add" -> Select this folder: ' + process.cwd());
  console.log('  4. Click Play (Cmd+P) or in top menu: 11Hopem -> Build -> Build macOS Standalone App (.app)!');
  process.exit(0);
}

console.log(`Executing Headless Standalone macOS Build via: ${unityBinary}`);
const projectPath = process.cwd();
const cmd = `"${unityBinary}" -quit -batchmode -projectPath "${projectPath}" -executeMethod ElevenHopem.EditorScripts.MacBuildScript.PerformMacBuild -logFile build_mac.log`;

try {
  execSync(cmd, { stdio: 'inherit' });
  console.log('macOS Standalone Build complete! Check Builds/macOS/11HopemMoonSweepers.app');
} catch (err) {
  console.error('Build execution error:', err.message);
  process.exit(1);
}
