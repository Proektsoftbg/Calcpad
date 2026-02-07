import * as vscode from 'vscode';
import * as fs from 'fs';
import * as os from 'os';
import * as path from 'path';
import { execFile } from 'child_process';

let settingsPanel: vscode.WebviewPanel | undefined = undefined;

const defaultSettingsXml = `<?xml version="1.0" encoding="utf-8"?>
<Settings xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
<Math>
    <Decimals>2</Decimals>
    <Degrees>0</Degrees>
    <IsComplex>false</IsComplex>
    <Substitute>true</Substitute>
    <FormatEquations>true</FormatEquations>
    <ZeroSmallMatrixElements>true</ZeroSmallMatrixElements>
    <MaxOutputCount>20</MaxOutputCount>
</Math>
<Plot>
    <IsAdaptive>true</IsAdaptive>
    <ScreenScaleFactor>2</ScreenScaleFactor>
    <ImagePath/>
    <ImageUri/>
    <VectorGraphics>false</VectorGraphics>
    <ColorScale>Rainbow</ColorScale>
    <SmoothScale>false</SmoothScale>
    <Shadows>true</Shadows>
    <LightDirection>NorthWest</LightDirection>
</Plot>
<Units>m</Units>
</Settings>`;

const settingTags = [
    'Decimals', 'Degrees', 'IsComplex', 'Substitute', 'FormatEquations',
    'ZeroSmallMatrixElements', 'MaxOutputCount', 'IsAdaptive', 'ScreenScaleFactor',
    'ImagePath', 'ImageUri', 'VectorGraphics', 'ColorScale', 'SmoothScale',
    'Shadows', 'LightDirection', 'Units'
];

function parseSettings(xml: string): { [key: string]: string } {
    const settings: { [key: string]: string } = {};
    for (const tag of settingTags) {
        const match = xml.match(new RegExp(`<${tag}>(.*?)</${tag}>`));
        if (match) {
            settings[tag] = match[1];
        } else {
            const selfClose = xml.match(new RegExp(`<${tag}\\s*/>`));
            if (selfClose) {
                settings[tag] = '';
            }
        }
    }
    return settings;
}

function buildXml(s: { [key: string]: string }): string {
    return `<?xml version="1.0" encoding="utf-8"?>
<Settings xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Math>
    <Decimals>${s['Decimals'] ?? '2'}</Decimals>
    <Degrees>${s['Degrees'] ?? '0'}</Degrees>
    <IsComplex>${s['IsComplex'] ?? 'false'}</IsComplex>
    <Substitute>${s['Substitute'] ?? 'true'}</Substitute>
    <FormatEquations>${s['FormatEquations'] ?? 'true'}</FormatEquations>
    <ZeroSmallMatrixElements>${s['ZeroSmallMatrixElements'] ?? 'true'}</ZeroSmallMatrixElements>
    <MaxOutputCount>${s['MaxOutputCount'] ?? '20'}</MaxOutputCount>
  </Math>
  <Plot>
    <IsAdaptive>${s['IsAdaptive'] ?? 'true'}</IsAdaptive>
    <ScreenScaleFactor>${s['ScreenScaleFactor'] ?? '2'}</ScreenScaleFactor>
    <ImagePath${s['ImagePath'] ? '>' + s['ImagePath'] + '</ImagePath' : ' /'}>
    <ImageUri${s['ImageUri'] ? '>' + s['ImageUri'] + '</ImageUri' : ' /'}>
    <VectorGraphics>${s['VectorGraphics'] ?? 'false'}</VectorGraphics>
    <ColorScale>${s['ColorScale'] ?? 'Rainbow'}</ColorScale>
    <SmoothScale>${s['SmoothScale'] ?? 'false'}</SmoothScale>
    <Shadows>${s['Shadows'] ?? 'true'}</Shadows>
    <LightDirection>${s['LightDirection'] ?? 'NorthWest'}</LightDirection>
  </Plot>
  <Units>${s['Units'] ?? 'm'}</Units>
</Settings>`;
}

/**
 * Writes content to a protected path by first saving to a temp file,
 * then using an elevated PowerShell to copy it to the destination.
 */
function writeFileElevated(targetPath: string, content: string): Promise<void> {
    return new Promise((resolve, reject) => {
        const tmpFile = path.join(os.tmpdir(), `calcpad_settings_${Date.now()}.xml`);
        try {
            fs.writeFileSync(tmpFile, content, 'utf8');
        } catch (err) {
            reject(err);
            return;
        }
        const innerCommand = `Copy-Item -LiteralPath '${tmpFile}' -Destination '${targetPath}' -Force; Remove-Item -LiteralPath '${tmpFile}' -Force`;
        execFile('powershell', [
            '-Command',
            `Start-Process powershell -ArgumentList '-Command', '${innerCommand.replace(/'/g, "''")}' -Verb RunAs -Wait`
        ], (error) => {
            // Clean up temp file in case the elevated process didn't
            try { fs.unlinkSync(tmpFile); } catch { /* ignore */ }
            if (error) {
                reject(error);
            } else {
                resolve();
            }
        });
    });
}

export function registerSettingsCommand(context: vscode.ExtensionContext): vscode.Disposable {
    // Keep track of the current settings path so the message listener
    // (registered once) always uses the latest value.
    let currentSettingsPath = '';

    function createPanel(): vscode.WebviewPanel {
        const panel = vscode.window.createWebviewPanel(
            'calcpadSettings',
            'Calcpad Settings',
            vscode.ViewColumn.One,
            { enableScripts: true }
        );

        panel.onDidDispose(() => { settingsPanel = undefined; });

        // Register the message listener exactly ONCE at panel creation
        panel.webview.onDidReceiveMessage(async (message: { command: string; settings?: { [key: string]: string } }) => {
            if (message.command === 'save' && message.settings) {
                const newXml = buildXml(message.settings);
                // Try direct write first
                try {
                    fs.writeFileSync(currentSettingsPath, newXml, 'utf8');
                    panel.webview.postMessage({ command: 'saved' });
                    vscode.window.showInformationMessage('Calcpad settings saved.');
                } catch {
                    // Needs admin — write via temp file + elevated copy
                    try {
                        await writeFileElevated(currentSettingsPath, newXml);
                        panel.webview.postMessage({ command: 'saved' });
                        vscode.window.showInformationMessage('Calcpad settings saved (admin).');
                    } catch (adminError: any) {
                        vscode.window.showErrorMessage(`Failed to save settings: ${adminError.message}`);
                        panel.webview.postMessage({ command: 'saveError', error: adminError.message });
                    }
                }
            } else if (message.command === 'reset') {
                const resetSettings = parseSettings(defaultSettingsXml);
                panel.webview.html = getSettingsHtml(resetSettings);
            }
        });

        return panel;
    }

    return vscode.commands.registerCommand("calcpad.settings", async () => {
        const config = vscode.workspace.getConfiguration('calcpad');
        currentSettingsPath = config.get<string>('settingsPath', 'C:\\Program Files\\Calcpad\\Settings.Xml');

        // Create file with default content if it doesn't exist
        if (!fs.existsSync(currentSettingsPath)) {
            try {
                fs.writeFileSync(currentSettingsPath, defaultSettingsXml, 'utf8');
            } catch {
                const choice = await vscode.window.showWarningMessage(
                    'Settings file does not exist and requires administrator privileges to create. Create it now?',
                    'Create as Admin'
                );
                if (choice === 'Create as Admin') {
                    try {
                        await writeFileElevated(currentSettingsPath, defaultSettingsXml);
                        vscode.window.showInformationMessage('Settings file created. Run the command again to open it.');
                    } catch (adminError: any) {
                        vscode.window.showErrorMessage(`Failed to create settings file: ${adminError.message}`);
                    }
                }
                return;
            }
        }

        let xmlContent: string;
        try {
            xmlContent = fs.readFileSync(currentSettingsPath, 'utf8');
        } catch {
            xmlContent = defaultSettingsXml;
        }
        const settings = parseSettings(xmlContent);

        // Reuse or create panel
        if (settingsPanel) {
            settingsPanel.reveal(vscode.ViewColumn.One);
        } else {
            settingsPanel = createPanel();
        }

        // Update the webview content
        settingsPanel.webview.html = getSettingsHtml(settings);
    });
}

function getSettingsHtml(s: { [key: string]: string }): string {
    const checked = (val: string) => val === 'true' ? 'checked' : '';
    const selected = (current: string, option: string) => current === option ? 'selected' : '';
    const degSelected = (val: string, opt: string) => val === opt ? 'selected' : '';

    return /*html*/`<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<style>
    :root {
        --bg: var(--vscode-editor-background);
        --fg: var(--vscode-editor-foreground);
        --input-bg: var(--vscode-input-background);
        --input-fg: var(--vscode-input-foreground);
        --input-border: var(--vscode-input-border, #3c3c3c);
        --btn-bg: var(--vscode-button-background);
        --btn-fg: var(--vscode-button-foreground);
        --btn-hover: var(--vscode-button-hoverBackground);
        --btn-sec-bg: var(--vscode-button-secondaryBackground);
        --btn-sec-fg: var(--vscode-button-secondaryForeground);
        --btn-sec-hover: var(--vscode-button-secondaryHoverBackground);
        --section-border: var(--vscode-panel-border, #2b2b2b);
        --desc: var(--vscode-descriptionForeground, #888);
        --focus: var(--vscode-focusBorder, #007fd4);
    }
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body {
        font-family: var(--vscode-font-family, system-ui, sans-serif);
        font-size: var(--vscode-font-size, 13px);
        color: var(--fg);
        background: var(--bg);
        padding: 20px 28px 40px;
        line-height: 1.5;
    }
    h1 {
        font-size: 20px;
        font-weight: 600;
        margin-bottom: 4px;
    }
    .subtitle {
        color: var(--desc);
        margin-bottom: 24px;
        font-size: 12px;
    }
    .section {
        border: 1px solid var(--section-border);
        border-radius: 6px;
        padding: 18px 20px 14px;
        margin-bottom: 16px;
    }
    .section-title {
        font-size: 14px;
        font-weight: 600;
        margin-bottom: 14px;
        display: flex;
        align-items: center;
        gap: 7px;
    }
    .section-title .icon {
        font-size: 16px;
        opacity: 0.85;
    }
    .field {
        display: grid;
        grid-template-columns: 220px 1fr;
        align-items: center;
        gap: 8px;
        margin-bottom: 10px;
    }
    .field label {
        font-weight: 500;
        white-space: nowrap;
    }
    .field .hint {
        grid-column: 1 / -1;
        color: var(--desc);
        font-size: 11px;
        margin-top: -6px;
        margin-bottom: 4px;
        padding-left: 228px;
    }
    input[type="number"], input[type="text"], select {
        background: var(--input-bg);
        color: var(--input-fg);
        border: 1px solid var(--input-border);
        border-radius: 4px;
        padding: 5px 8px;
        font-size: 13px;
        font-family: inherit;
        width: 100%;
        max-width: 260px;
        outline: none;
    }
    input:focus, select:focus {
        border-color: var(--focus);
    }
    input[type="number"] { max-width: 120px; }
    .toggle {
        position: relative;
        display: inline-flex;
        align-items: center;
        gap: 8px;
        cursor: pointer;
        user-select: none;
    }
    .toggle input { display: none; }
    .toggle .slider {
        width: 36px;
        height: 20px;
        background: var(--input-border);
        border-radius: 10px;
        position: relative;
        transition: background 0.2s;
    }
    .toggle .slider::after {
        content: '';
        position: absolute;
        width: 14px;
        height: 14px;
        background: var(--fg);
        border-radius: 50%;
        top: 3px;
        left: 3px;
        transition: transform 0.2s;
    }
    .toggle input:checked + .slider {
        background: var(--btn-bg);
    }
    .toggle input:checked + .slider::after {
        transform: translateX(16px);
    }
    .toggle .label-text {
        font-size: 12px;
        color: var(--desc);
    }
    .actions {
        display: flex;
        gap: 10px;
        margin-top: 20px;
    }
    button {
        padding: 7px 20px;
        border: none;
        border-radius: 4px;
        font-size: 13px;
        font-family: inherit;
        cursor: pointer;
        font-weight: 500;
    }
    .btn-primary {
        background: var(--btn-bg);
        color: var(--btn-fg);
    }
    .btn-primary:hover { background: var(--btn-hover); }
    .btn-secondary {
        background: var(--btn-sec-bg);
        color: var(--btn-sec-fg);
    }
    .btn-secondary:hover { background: var(--btn-sec-hover); }
    .toast {
        position: fixed;
        bottom: 20px;
        right: 24px;
        background: var(--btn-bg);
        color: var(--btn-fg);
        padding: 8px 18px;
        border-radius: 6px;
        font-size: 13px;
        opacity: 0;
        transform: translateY(8px);
        transition: all 0.3s;
        pointer-events: none;
    }
    .toast.show {
        opacity: 1;
        transform: translateY(0);
    }
</style>
</head>
<body>
    <h1>⚙ Calcpad Settings</h1>
    <p class="subtitle">Configure math, plotting, and unit defaults for Calcpad.</p>

    <!-- ───── Math ───── -->
    <div class="section">
        <div class="section-title"><span class="icon">∑</span> Math</div>

        <div class="field">
            <label for="Decimals">Decimal digits</label>
            <input type="number" id="Decimals" min="0" max="15" value="${s['Decimals'] ?? '2'}">
            <span class="hint">Number of digits after the decimal point (0–15)</span>
        </div>

        <div class="field">
            <label for="Degrees">Angle units</label>
            <select id="Degrees">
                <option value="0" ${degSelected(s['Degrees'] ?? '0', '0')}>Degrees (°)</option>
                <option value="1" ${degSelected(s['Degrees'] ?? '0', '1')}>Radians (rad)</option>
                <option value="2" ${degSelected(s['Degrees'] ?? '0', '2')}>Gradians (grad)</option>
            </select>
        </div>

        <div class="field">
            <label>Complex numbers</label>
            <label class="toggle">
                <input type="checkbox" id="IsComplex" ${checked(s['IsComplex'] ?? 'false')}>
                <span class="slider"></span>
                <span class="label-text">Enable complex number arithmetic</span>
            </label>
        </div>

        <div class="field">
            <label>Substitute values</label>
            <label class="toggle">
                <input type="checkbox" id="Substitute" ${checked(s['Substitute'] ?? 'true')}>
                <span class="slider"></span>
                <span class="label-text">Show substituted values in equations</span>
            </label>
        </div>

        <div class="field">
            <label>Format equations</label>
            <label class="toggle">
                <input type="checkbox" id="FormatEquations" ${checked(s['FormatEquations'] ?? 'true')}>
                <span class="slider"></span>
                <span class="label-text">Pretty-print equations in output</span>
            </label>
        </div>

        <div class="field">
            <label>Zero small matrix elements</label>
            <label class="toggle">
                <input type="checkbox" id="ZeroSmallMatrixElements" ${checked(s['ZeroSmallMatrixElements'] ?? 'true')}>
                <span class="slider"></span>
                <span class="label-text">Treat near-zero matrix elements as zero</span>
            </label>
        </div>

        <div class="field">
            <label for="MaxOutputCount">Max output count</label>
            <input type="number" id="MaxOutputCount" min="1" max="1000" value="${s['MaxOutputCount'] ?? '20'}">
            <span class="hint">Maximum number of output values for iterative procedures</span>
        </div>
    </div>

    <!-- ───── Plot ───── -->
    <div class="section">
        <div class="section-title"><span class="icon">📈</span> Plot</div>

        <div class="field">
            <label>Adaptive plotting</label>
            <label class="toggle">
                <input type="checkbox" id="IsAdaptive" ${checked(s['IsAdaptive'] ?? 'true')}>
                <span class="slider"></span>
                <span class="label-text">Use adaptive mesh refinement for smoother curves</span>
            </label>
        </div>

        <div class="field">
            <label for="ScreenScaleFactor">Screen scale factor</label>
            <input type="number" id="ScreenScaleFactor" min="1" max="4" step="0.5" value="${s['ScreenScaleFactor'] ?? '2'}">
            <span class="hint">HiDPI / Retina scale factor (1–4)</span>
        </div>

        <div class="field">
            <label for="ImagePath">Image path</label>
            <input type="text" id="ImagePath" value="${s['ImagePath'] ?? ''}" placeholder="(default)">
            <span class="hint">Local file path for saving plot images</span>
        </div>

        <div class="field">
            <label for="ImageUri">Image URI</label>
            <input type="text" id="ImageUri" value="${s['ImageUri'] ?? ''}" placeholder="(default)">
            <span class="hint">Base URI used to reference plot images in HTML output</span>
        </div>

        <div class="field">
            <label>Vector graphics (SVG)</label>
            <label class="toggle">
                <input type="checkbox" id="VectorGraphics" ${checked(s['VectorGraphics'] ?? 'false')}>
                <span class="slider"></span>
                <span class="label-text">Render plots as SVG instead of PNG</span>
            </label>
        </div>

        <div class="field">
            <label for="ColorScale">Color palette</label>
            <select id="ColorScale">
                ${['None', 'Gray', 'Rainbow', 'Terrain', 'Violet-yellow', 'Green-yellow', 'Blues', 'Blue-yellow', 'Blue-red', 'Purple-yellow']
                    .map(c => `<option value="${c}" ${selected(s['ColorScale'] ?? 'Rainbow', c)}>${c}</option>`).join('\n                ')}
            </select>
            <span class="hint">Color scheme for surface / map plots</span>
        </div>

        <div class="field">
            <label>Smooth color scale</label>
            <label class="toggle">
                <input type="checkbox" id="SmoothScale" ${checked(s['SmoothScale'] ?? 'false')}>
                <span class="slider"></span>
                <span class="label-text">Smooth gradient instead of discrete bands</span>
            </label>
        </div>

        <div class="field">
            <label>Shadows</label>
            <label class="toggle">
                <input type="checkbox" id="Shadows" ${checked(s['Shadows'] ?? 'true')}>
                <span class="slider"></span>
                <span class="label-text">Draw shadows on surface plots</span>
            </label>
        </div>

        <div class="field">
            <label for="LightDirection">Light direction</label>
            <select id="LightDirection">
                ${['North', 'NorthEast', 'East', 'SouthEast', 'South', 'SouthWest', 'West', 'NorthWest']
                    .map(d => `<option value="${d}" ${selected(s['LightDirection'] ?? 'NorthWest', d)}>${d}</option>`).join('\n                ')}
            </select>
            <span class="hint">Direction of the light source for surface plot shading</span>
        </div>
    </div>

    <!-- ───── Units ───── -->
    <div class="section">
        <div class="section-title"><span class="icon">📐</span> Units</div>
        <div class="field">
            <label for="Units">Default length unit</label>
            <select id="Units">
                ${['m', 'cm', 'mm']
                    .map(u => `<option value="${u}" ${selected(s['Units'] ?? 'm', u)}>${u}</option>`).join('\n                ')}
            </select>
        </div>
    </div>

    <!-- ───── Actions ───── -->
    <div class="actions">
        <button class="btn-primary" onclick="saveSettings()">Save</button>
        <button class="btn-secondary" onclick="resetDefaults()">Reset to Defaults</button>
    </div>

    <div class="toast" id="toast">Settings saved ✓</div>

<script>
    const vscode = acquireVsCodeApi();

    function gatherSettings() {
        const ids = [
            'Decimals', 'Degrees', 'MaxOutputCount', 'ScreenScaleFactor',
            'ImagePath', 'ImageUri', 'ColorScale', 'LightDirection', 'Units'
        ];
        const bools = [
            'IsComplex', 'Substitute', 'FormatEquations', 'ZeroSmallMatrixElements',
            'IsAdaptive', 'VectorGraphics', 'SmoothScale', 'Shadows'
        ];
        const settings = {};
        for (const id of ids) {
            settings[id] = document.getElementById(id).value;
        }
        for (const id of bools) {
            settings[id] = document.getElementById(id).checked ? 'true' : 'false';
        }
        return settings;
    }

    function saveSettings() {
        vscode.postMessage({ command: 'save', settings: gatherSettings() });
    }

    function resetDefaults() {
        vscode.postMessage({ command: 'reset' });
    }

    window.addEventListener('message', event => {
        const msg = event.data;
        if (msg.command === 'saved') {
            const toast = document.getElementById('toast');
            toast.classList.add('show');
            setTimeout(() => toast.classList.remove('show'), 2200);
        }
    });
</script>
</body>
</html>`;
}
