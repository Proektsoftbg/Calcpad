#!/bin/sh

# If the calcpad directory does not exist in this user's home, then we populate it
if [ ! -e "$HOME/calcpad" ]; then
    mkdir "$HOME/calcpad"
    cp -a "/usr/share/Calcpad/Examples" "$HOME/calcpad/Examples"
    mkdir -p "$HOME/calcpad/syntax/Sublime"
    cp -a "/usr/share/Calcpad/Syntax/Sublime" "$HOME/calcpad/syntax/Sublime"
fi

# If the Calcpad configuration directory does not exist yet, then we create it
if [ ! -e "$HOME/.config/calcpad" ]; then
    mkdir -p "$HOME/.config/calcpad/"
fi

# If the Calcpad configuration file itself does not exist yet, we create it
if [ ! -e "$HOME/.config/calcpad/Settings.xml" ]; then
    cp "/usr/share/Calcpad/Settings.xml" "$HOME/.config/calcpad/Settings.xml"
fi

# Exec into the actual executable, passing all arguments
exec dotnet /usr/share/Calcpad/Calcpad.dll "$@"
