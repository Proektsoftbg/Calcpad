#How to develop engineering calculations with Calcpad and Claude in VS Code

1. Install software
> Visual Studio Code - https://code.visualstudio.com/download
> Calcpad for desktop - https://calcpad.eu/download/calcpad-VM-setup-en-x64.zip
> Node.js - https://nodejs.org/en/download/current

2. Install VS Code extensions
> Calcpad - https://marketplace.visualstudio.com/items?itemName=ProektsoftEOOD.calcpad
> Claude - https://marketplace.visualstudio.com/items?itemName=anthropic.claude-code

3. Install Claude Code in the terminal 
> Start VS Code press Ctrl+\` top open the terminal. Then type the following:
> `npm install -g @anthropic-ai/claude-code`

4. Create a work folder for your Calcpad project and place these two files in it:
> https://github.com/Proektsoftbg/Calcpad/blob/main/Setup/AI/Work/CALCPAD_CLAUDE_INSTRUCTIONS.txt
> https://github.com/Proektsoftbg/Calcpad/blob/main/Setup/AI/Work/CALCPAD_LANGUAGE_REFERENCE_FOR_CLAUDE.md

5. Start creating
> Open your work folder in VS Code
> Press Ctrl+\` to open the terminal and type: `claude`
> Give Claude prompts, e.g.: 
>> `Create a Calcpad program for analysis of a simply supported beam`
>> `Calculate I-section properties with Calcpad`
> Open the generated file and press Ctrl+Shift+B to run it with Calcpad
> Correct errors if any or ask Claude to do it. Paste error messages from Calcpad into Claude console to give it clues.