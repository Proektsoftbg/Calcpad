# Calcpad for VS Code

Language support for Calcpad (.cpd files) in Visual Studio Code.

## Features

- **Syntax Highlighting** - Full syntax highlighting for Calcpad files including:
  - Keywords (#if, #else, #for, #while, #def, etc.)
  - Methods ($Root, $Find, $Plot, $Integral, etc.)
  - Built-in functions (sin, cos, sqrt, matrix, etc.)
  - Numbers with units (10m, 5kg, 100Pa, etc.)
  - Operators and special symbols
  - Strings and comments

- **Auto-completion** - IntelliSense completions for:
  - All Calcpad keywords
  - Built-in functions
  - Units (SI and imperial)
  - Special methods

- **Hover Documentation** - Hover over functions to see descriptions

- **Run Files** - Run Calcpad files directly from VS Code (Ctrl+Shift+B)

- **Color Theme** - Includes a Monokai-based color theme optimized for Calcpad

## Installation

### From VSIX (Recommended)

1. Download the `.vsix` file
2. In VS Code, press `Ctrl+Shift+P` and type "Install from VSIX"
3. Select the downloaded `.vsix` file

### Manual Installation

1. Copy the entire `calcpad-vscode` folder to:
   - Windows: `%USERPROFILE%\.vscode\extensions\`
   - macOS/Linux: `~/.vscode/extensions/`
2. Restart VS Code

### From Source (Development)

1. Clone/copy this folder
2. Run `npm install` to install dependencies
3. Run `npm run compile` to build the extension
4. Press F5 to launch a new VS Code window with the extension loaded

## Building the VSIX Package

1. Install vsce: `npm install -g @vscode/vsce`
2. Run: `vsce package`
3. This creates a `.vsix` file you can share and install

## Usage

1. Open any `.cpd` file - syntax highlighting is automatic
2. Start typing to see auto-completions
3. Hover over functions for documentation
4. Press `Ctrl+Shift+B` or right-click and select "Run Calcpad File" to execute

## Color Theme

To use the included Calcpad Monokai theme:
1. Press `Ctrl+K Ctrl+T` to open the theme picker
2. Select "Calcpad Monokai"

## File Association

The extension automatically associates with `.cpd` files. To manually set the language:
1. Click on the language indicator in the status bar (bottom right)
2. Select "Calcpad"

## Requirements

- VS Code 1.74.0 or higher
- Calcpad must be installed and `.cpd` files associated with it for the Run command to work

## Known Issues

- The Run command assumes `.cpd` files are associated with the Calcpad executable

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## License

MIT
