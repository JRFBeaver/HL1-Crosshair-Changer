#!/usr/bin/env bash
cd "$(dirname "$0")"
chmod +x HL1_Crosshair_Changer_Linux.py 2>/dev/null
if command -v python3 >/dev/null 2>&1; then
    python3 HL1_Crosshair_Changer_Linux.py
else
    echo "[ERROR] python3 is required to run HL1_Crosshair_Changer_Linux.py"
    read -p "Press Enter to exit..."
fi
