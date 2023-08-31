#!/usr/bin/env bash

# packagesNeeded='cmatrix'

declare -a packagesNeeded=(
    "xdotool"
    "wmctrl"
)

if [ -x "$(command -v pacman)" ]; then packagesNeeded+=("xorg-xprop" "xorg-xwininfo");
else packagesNeeded+=("xprop" "xwininfo"); fi

if [ -x "$(command -v apk)" ];       then sudo apk add --no-cache "${packagesNeeded[@]}"
elif [ -x "$(command -v apt-get)" ]; then sudo apt-get install "${packagesNeeded[@]}"
elif [ -x "$(command -v dnf)" ];     then sudo dnf install "${packagesNeeded[@]}"
elif [ -x "$(command -v zypper)" ];  then sudo zypper install "${packagesNeeded[@]}"
elif [ -x "$(command -v pacman)" ];  then sudo pacman -S "${packagesNeeded[@]}"
elif [ ${#packagesNeeded[@]} -gt 1 ]; then
    echo "FAILED TO INSTALL PACKAGES: Package manager not found. You must manually install: ">&2;
    echo "${packagesNeeded[@]}";
else 
    echo "FAILED TO INSTALL PACKAGE: Package manager not found. You must manually install: ">&2;
    echo "${packagesNeeded[@]}"
fi