/**
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 * Licensed under 11Hopem Proprietary License.
 */

import { test } from 'node:test';
import assert from 'node:assert';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const ROOT_DIR = path.resolve(__dirname, '..');

test('Unity Engine Setup: Verify ProjectSettings, Packages, and Editor configuration', () => {
  const manifestPath = path.join(ROOT_DIR, 'Packages', 'manifest.json');
  assert.ok(fs.existsSync(manifestPath), 'Packages/manifest.json must exist');
  const manifest = JSON.parse(fs.readFileSync(manifestPath, 'utf8'));
  assert.ok(manifest.dependencies['com.unity.modules.core'], 'Core Unity module must be present');
  assert.ok(manifest.dependencies['com.unity.inputsystem'], 'Input system must be present');

  const projectVersionPath = path.join(ROOT_DIR, 'ProjectSettings', 'ProjectVersion.txt');
  assert.ok(fs.existsSync(projectVersionPath), 'ProjectSettings/ProjectVersion.txt must exist');
  const versionContent = fs.readFileSync(projectVersionPath, 'utf8');
  assert.match(versionContent, /m_EditorVersion:\s*6000\./, 'Editor version should be Unity 6 or LTS');

  const projectSettingsAsset = path.join(ROOT_DIR, 'ProjectSettings', 'ProjectSettings.asset');
  assert.ok(fs.existsSync(projectSettingsAsset), 'ProjectSettings.asset must exist');
  const settingsContent = fs.readFileSync(projectSettingsAsset, 'utf8');
  assert.match(settingsContent, /companyName:\s*11Hopem/);
  assert.match(settingsContent, /productName:\s*Moon Sweepers/);
  assert.match(settingsContent, /applicationIdentifier:/);
});

test('Unity C# Architecture: Verify all 16 modular C# scripts and .meta integrity', () => {
  const expectedScripts = [
    'Assets/Scripts/Core/GameManager.cs',
    'Assets/Scripts/Core/AudioManager.cs',
    'Assets/Scripts/Core/SaveManager.cs',
    'Assets/Scripts/Player/PlayerController.cs',
    'Assets/Scripts/Player/PlayerInventory.cs',
    'Assets/Scripts/Tools/CleaningTool.cs',
    'Assets/Scripts/Tools/ToolController.cs',
    'Assets/Scripts/Environment/LunarSurfaceGenerator.cs',
    'Assets/Scripts/Environment/RegolithDustPatch.cs',
    'Assets/Scripts/Environment/DepotHopper.cs',
    'Assets/Scripts/Economy/ShopManager.cs',
    'Assets/Scripts/Economy/AutomationStation.cs',
    'Assets/Scripts/UI/HUDController.cs',
    'Assets/Scripts/UI/ShopUIController.cs',
    'Assets/Scripts/Editor/MacBuildScript.cs',
    'Assets/Scripts/Editor/LunarSceneBuilder.cs'
  ];

  for (const rel of expectedScripts) {
    const fullPath = path.join(ROOT_DIR, rel);
    const metaPath = `${fullPath}.meta`;

    assert.ok(fs.existsSync(fullPath), `Script must exist: ${rel}`);
    assert.ok(fs.existsSync(metaPath), `Meta file must exist for: ${rel}`);

    const code = fs.readFileSync(fullPath, 'utf8');
    assert.ok(code.length > 100, `Script must contain substantial logic: ${rel}`);
    assert.match(code, /namespace ElevenHopem/, `Script must belong to ElevenHopem namespace: ${rel}`);
  }
});

test('Unity Scene & Mac Standalone Build: Verify MainLunarBase.unity and MacBuildScript', () => {
  const scenePath = path.join(ROOT_DIR, 'Assets', 'Scenes', 'MainLunarBase.unity');
  assert.ok(fs.existsSync(scenePath), 'MainLunarBase.unity scene file must exist');
  const sceneContent = fs.readFileSync(scenePath, 'utf8');
  assert.match(sceneContent, /%YAML 1\.1/);
  assert.match(sceneContent, /m_Name: Directional Light \(Sun\)/);
  assert.match(sceneContent, /m_Name: AstronautPlayer/);
  assert.match(sceneContent, /m_Name: DepotHopper/);

  const buildScriptPath = path.join(ROOT_DIR, 'Assets', 'Scripts', 'Editor', 'MacBuildScript.cs');
  const buildScript = fs.readFileSync(buildScriptPath, 'utf8');
  assert.match(buildScript, /BuildTarget\.StandaloneOSX/);
  assert.match(buildScript, /11HopemMoonSweepers\.app/);
});

test('Brand and Security Governance: Rule 6 and Rule 12 validation', () => {
  const gitignorePath = path.join(ROOT_DIR, '.gitignore');
  const gitignore = fs.readFileSync(gitignorePath, 'utf8');
  assert.ok(gitignore.includes('/[Ll]ibrary/'), 'Must ignore Unity Library');
  assert.ok(gitignore.includes('/[Tt]emp/'), 'Must ignore Unity Temp');
  assert.ok(gitignore.includes('.env'), 'Must ignore .env');
  assert.ok(gitignore.includes('.env.*'), 'Must ignore .env.*');
  assert.ok(gitignore.includes('*.pem'), 'Must ignore *.pem');
  assert.ok(gitignore.includes('*credentials*.json'), 'Must ignore credentials');

  const hudScript = fs.readFileSync(path.join(ROOT_DIR, 'Assets/Scripts/UI/HUDController.cs'), 'utf8');
  assert.match(hudScript, /https:\/\/11hopem\.com/);
  assert.doesNotMatch(hudScript, /Software Ltd\. Şti\./);
});
