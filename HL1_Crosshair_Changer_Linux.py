#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Half-Life 1 Crosshair Changer v5.1 (Linux Native Edition)
By FFBeaver and Gemini
"""

import os
import re
import math
import struct
import shutil
import tkinter as tk
from tkinter import filedialog, messagebox, colorchooser

class HL1CrosshairChangerLinux:
    def __init__(self, root):
        self.root = root
        self.root.title("Half-Life Crosshair Changer v5.1")
        self.root.geometry("610x545")
        self.root.resizable(False, False)

        # Base Colors & Game Mode Palettes
        self.bg_dark = "#181512"
        self.panel_bg = "#241F1A"
        self.text_white = "#F0E6D7"

        self.col_hl = "#FF9600"      # HL1 Orange
        self.col_of = "#32CD32"      # Opposing Force Green
        self.col_bs = "#1E90FF"      # Blue Shift Blue
        self.col_custom = "#FFD700"  # Custom Mod Gold

        self.active_accent = self.col_hl
        self.game_mode = 0  # 0=HL (valve), 1=OF (gearbox), 2=BS (bshift), 3=Custom

        self.root.configure(bg=self.bg_dark)

        self.is_russian = False  # Default: English
        self.secret_clicks = 0
        self.custom_rgb = (0, 255, 128)

        self.styles_en = [
            "Large Circle + Dot",
            "Medium Circle + Dot",
            "Small Circle + Dot",
            "Large Circle",
            "Medium Circle",
            "Small Circle",
            "Large Brackets (.) + Dot",
            "Medium Brackets (.) + Dot",
            "Small Brackets (.) + Dot",
            "Large V-Chevron",
            "Medium V-Chevron",
            "Small V-Chevron",
            "Medium V-Chevron + Dot",
            "Bold Dot",
            "Small Crisp Dot"
        ]

        self.styles_ru = [
            "Большой круг с точкой (Large Circle + Dot)",
            "Средний круг с точкой (Medium Circle + Dot)",
            "Малый круг с точкой (Small Circle + Dot)",
            "Большой круг (Large Circle)",
            "Средний круг (Medium Circle)",
            "Малый круг (Small Circle)",
            "Большие дужки (.) с точкой (Large (.) Brackets)",
            "Средние дужки (.) с точкой (Medium (.) Brackets)",
            "Малые дужки (.) с точкой (Small (.) Brackets)",
            "Большой V-образный (Large V-Chevron)",
            "Средний V-образный (Medium V-Chevron)",
            "Малый V-образный (Small V-Chevron)",
            "V-образный с точкой (Medium V-Chevron + Dot)",
            "Крупная точка (Bold Dot)",
            "Малая четкая точка (Small Dot)"
        ]

        self.colors_en = [
            "Classic HL1 Amber", "Green", "Cyan", "Blue",
            "Red", "Yellow", "Magenta", "White", "Custom RGB Color..."
        ]

        self.colors_ru = [
            "Янтарный HL1 (Classic Amber)", "Зеленый (Green)", "Голубой (Cyan)", "Синий (Blue)",
            "Красный (Red)", "Желтый (Yellow)", "Розовый (Magenta)", "Белый (White)", "Свой цвет... (Custom RGB)"
        ]

        self.res_en = [
            "All Screen Resolutions (Recommended)",
            "Standard / HD (< 1280px)",
            "Full HD / Quad HD (1280px - 2559px)",
            "4K Ultra HD (2560px and above)"
        ]

        self.res_ru = [
            "Все разрешения экрана (Универсально)",
            "Стандартное / HD (до 1280px)",
            "Full HD / Quad HD (1280px - 2559px)",
            "4K Ultra HD (2560px и выше)"
        ]

        self.preset_rgb = [
            (255, 160, 0),
            (0, 255, 0),
            (0, 255, 255),
            (0, 110, 255),
            (255, 35, 35),
            (255, 255, 0),
            (255, 0, 255),
            (255, 255, 255)
        ]

        self.res_idx = 0
        self.style_idx = 6
        self.col_idx = 0

        self.build_ui()
        self.apply_language()
        self.select_game_mode(0)
        self.auto_detect_path()

    def build_ui(self):
        # Header
        header = tk.Frame(self.root, bg="#120F0C", height=48, width=610)
        header.place(x=0, y=0)
        self.header_line = tk.Frame(self.root, bg=self.active_accent, height=2, width=610)
        self.header_line.place(x=0, y=46)

        self.lbl_title = tk.Label(
            header, text="HALF-LIFE  //  CROSSHAIR CHANGER",
            bg="#120F0C", fg=self.active_accent, font=("DejaVu Sans", 13, "bold")
        )
        self.lbl_title.place(x=18, y=11)

        tk.Label(
            header, text="v5.1",
            bg="#120F0C", fg="#8C6E3C", font=("Monospace", 10, "bold")
        ).place(x=445, y=15)

        self.btn_lang = tk.Button(
            header, text="Lang: EN", bg=self.panel_bg, fg=self.active_accent,
            activebackground="#3C3020", activeforeground="#FFFFFF",
            relief="solid", bd=1, font=("DejaVu Sans", 8, "bold"), command=self.toggle_language
        )
        self.btn_lang.place(x=495, y=10, width=90, height=26)

        # 4 Game / Mod Buttons
        self.btn_hl = tk.Button(
            self.root, text="Half-Life", relief="solid", bd=2,
            font=("DejaVu Sans", 8, "bold"), command=lambda: self.select_game_mode(0)
        )
        self.btn_hl.place(x=20, y=60, width=130, height=32)

        self.btn_of = tk.Button(
            self.root, text="Opposing Force", relief="solid", bd=1,
            font=("DejaVu Sans", 8, "bold"), command=lambda: self.select_game_mode(1)
        )
        self.btn_of.place(x=158, y=60, width=135, height=32)

        self.btn_bs = tk.Button(
            self.root, text="Blue Shift", relief="solid", bd=1,
            font=("DejaVu Sans", 8, "bold"), command=lambda: self.select_game_mode(2)
        )
        self.btn_bs.place(x=301, y=60, width=125, height=32)

        self.btn_custom = tk.Button(
            self.root, text="Custom Mod", relief="solid", bd=1,
            font=("DejaVu Sans", 8, "bold"), command=lambda: self.select_game_mode(3)
        )
        self.btn_custom.place(x=434, y=60, width=151, height=32)

        # Custom Mod Folder Entry
        self.lbl_custom_mod = tk.Label(self.root, bg=self.bg_dark, fg=self.col_custom, font=("DejaVu Sans", 9))
        self.txt_custom_mod = tk.Entry(
            self.root, bg=self.panel_bg, fg=self.col_custom,
            insertbackground=self.col_custom, relief="solid", bd=1, font=("Monospace", 9, "bold")
        )
        self.txt_custom_mod.insert(0, "cstrike")

        # Path
        self.lbl_path = tk.Label(self.root, bg=self.bg_dark, fg=self.active_accent, font=("DejaVu Sans", 9))
        self.lbl_path.place(x=20, y=130)

        self.txt_path = tk.Entry(
            self.root, bg=self.panel_bg, fg=self.text_white,
            insertbackground=self.active_accent, relief="solid", bd=1, font=("DejaVu Sans", 9)
        )
        self.txt_path.place(x=20, y=152, width=455, height=25)

        self.btn_browse = tk.Button(
            self.root, bg=self.panel_bg, fg=self.active_accent,
            relief="solid", bd=1, font=("DejaVu Sans", 8, "bold"), command=self.browse_dir
        )
        self.btn_browse.place(x=485, y=151, width=100, height=26)

        # Resolution
        self.lbl_res = tk.Label(self.root, bg=self.bg_dark, fg=self.active_accent, font=("DejaVu Sans", 9))
        self.lbl_res.place(x=20, y=190)

        self.var_res = tk.StringVar()
        self.opt_res = tk.OptionMenu(self.root, self.var_res, "")
        self.style_option_menu(self.opt_res)
        self.opt_res.place(x=20, y=212, width=360, height=28)

        # Style
        self.lbl_style = tk.Label(self.root, bg=self.bg_dark, fg=self.active_accent, font=("DejaVu Sans", 9))
        self.lbl_style.place(x=20, y=250)

        self.var_style = tk.StringVar()
        self.opt_style = tk.OptionMenu(self.root, self.var_style, "")
        self.style_option_menu(self.opt_style)
        self.opt_style.place(x=20, y=272, width=360, height=28)

        # Color
        self.lbl_color = tk.Label(self.root, bg=self.bg_dark, fg=self.active_accent, font=("DejaVu Sans", 9))
        self.lbl_color.place(x=20, y=310)

        self.var_color = tk.StringVar()
        self.opt_color = tk.OptionMenu(self.root, self.var_color, "")
        self.style_option_menu(self.opt_color)
        self.opt_color.place(x=20, y=332, width=240, height=28)

        self.btn_rgb = tk.Button(
            self.root, bg=self.panel_bg, fg=self.active_accent,
            relief="solid", bd=1, font=("DejaVu Sans", 8, "bold"), command=self.pick_custom_color
        )
        self.btn_rgb.place(x=270, y=332, width=110, height=28)

        # Preview
        self.lbl_prev = tk.Label(self.root, bg=self.bg_dark, fg=self.active_accent, font=("DejaVu Sans", 9))
        self.lbl_prev.place(x=405, y=190)

        self.canvas = tk.Canvas(
            self.root, width=175, height=150, bg="#0E0D0B",
            highlightbackground=self.active_accent, highlightthickness=1
        )
        self.canvas.place(x=405, y=212)

        # Action Buttons
        self.btn_install = tk.Button(
            self.root, bg="#8B4700", fg="#FFFFFF",
            relief="solid", bd=1, font=("DejaVu Sans", 10, "bold"), command=self.install_crosshair
        )
        self.btn_install.place(x=20, y=385, width=280, height=44)

        self.btn_restore = tk.Button(
            self.root, bg=self.panel_bg, fg=self.active_accent,
            relief="solid", bd=1, font=("DejaVu Sans", 9, "bold"), command=self.restore_backup
        )
        self.btn_restore.place(x=315, y=385, width=265, height=44)

        # Status
        self.lbl_status = tk.Label(self.root, bg=self.bg_dark, fg=self.active_accent, font=("DejaVu Sans", 9))
        self.lbl_status.place(x=20, y=442)

        # Secret Bottom-Left Easter Egg Zone (5 clicks -> "Kosh=💩")
        self.lbl_easter = tk.Label(
            self.root, text="", bg=self.bg_dark, fg=self.active_accent,
            font=("DejaVu Sans", 11, "bold"), anchor="w"
        )
        self.lbl_easter.place(x=18, y=480, width=160, height=32)
        self.lbl_easter.bind("<Button-1>", self.trigger_easter_egg)
        self.root.bind("<Button-1>", self.check_corner_click)

        # Credits
        self.lbl_credits = tk.Label(
            self.root, text="By FFBeaver and Gemini", bg=self.bg_dark, fg=self.active_accent,
            font=("DejaVu Sans", 9, "bold"), anchor="e"
        )
        self.lbl_credits.place(x=390, y=485, width=195, height=24)

    def select_game_mode(self, mode):
        self.game_mode = mode
        if mode == 0: self.active_accent = self.col_hl
        elif mode == 1: self.active_accent = self.col_of
        elif mode == 2: self.active_accent = self.col_bs
        else: self.active_accent = self.col_custom

        self.btn_hl.config(bg="#4A2C00" if mode == 0 else "#1C1916", fg="#FFFFFF" if mode == 0 else self.col_hl)
        self.btn_of.config(bg="#104010" if mode == 1 else "#1C1916", fg="#FFFFFF" if mode == 1 else self.col_of)
        self.btn_bs.config(bg="#0C3058" if mode == 2 else "#1C1916", fg="#FFFFFF" if mode == 2 else self.col_bs)
        self.btn_custom.config(bg="#4A3E00" if mode == 3 else "#1C1916", fg="#FFFFFF" if mode == 3 else self.col_custom)

        if mode == 3:
            self.lbl_custom_mod.place(x=20, y=100)
            self.txt_custom_mod.place(x=270, y=99, width=155, height=23)
        else:
            self.lbl_custom_mod.place_forget()
            self.txt_custom_mod.place_forget()

        for lbl in (self.lbl_title, self.lbl_path, self.lbl_res, self.lbl_style, self.lbl_color, self.lbl_prev, self.lbl_credits, self.lbl_status):
            lbl.config(fg=self.active_accent)

        self.header_line.config(bg=self.active_accent)
        self.canvas.config(highlightbackground=self.active_accent)
        self.btn_lang.config(fg=self.active_accent)
        self.btn_browse.config(fg=self.active_accent)
        self.btn_rgb.config(fg=self.active_accent)
        self.btn_restore.config(fg=self.active_accent)

        install_bg = {"#FF9600": "#9A5000", "#32CD32": "#1E781E", "#1E90FF": "#125496", "#FFD700": "#8A7000"}[self.active_accent]
        self.btn_install.config(bg=install_bg)
        self.update_status_target()

    def get_target_mod_folder(self):
        if self.game_mode == 0: return "valve"
        if self.game_mode == 1: return "gearbox"
        if self.game_mode == 2: return "bshift"
        c = self.txt_custom_mod.get().strip()
        return c if c else "cstrike"

    def update_status_target(self):
        mod = self.get_target_mod_folder()
        if self.is_russian:
            self.lbl_status.config(text=f"Целевая папка мода: {mod}/sprites")
        else:
            self.lbl_status.config(text=f"Target mod folder: {mod}/sprites")

    def style_option_menu(self, opt):
        opt.config(
            bg=self.panel_bg, fg=self.text_white, activebackground="#3C3020",
            activeforeground="#FFFFFF", highlightthickness=1,
            highlightbackground="#785014", relief="flat", font=("DejaVu Sans", 9), anchor="w"
        )
        opt["menu"].config(
            bg=self.panel_bg, fg=self.text_white, activebackground="#9A5000",
            activeforeground="#FFFFFF", font=("DejaVu Sans", 9)
        )

    def check_corner_click(self, event):
        if event.widget == self.root and event.x <= 150 and event.y >= 470:
            self.trigger_easter_egg(event)

    def trigger_easter_egg(self, event=None):
        self.secret_clicks += 1
        if self.secret_clicks >= 5:
            self.lbl_easter.config(text="Kosh=\U0001F4A9")

    def toggle_language(self):
        self.is_russian = not self.is_russian
        self.apply_language()

    def populate_menu(self, opt, var, items, callback):
        menu = opt["menu"]
        menu.delete(0, "end")
        for i, item in enumerate(items):
            menu.add_command(label=item, command=lambda idx=i, val=item: callback(idx, val))

    def on_res_select(self, idx, val):
        self.res_idx = idx
        self.var_res.set(val)

    def on_style_select(self, idx, val):
        self.style_idx = idx
        self.var_style.set(val)
        self.draw_preview()

    def on_color_select(self, idx, val):
        self.col_idx = idx
        self.var_color.set(val)
        if idx == len(self.preset_rgb):
            self.pick_custom_color()
        self.draw_preview()

    def apply_language(self):
        if self.is_russian:
            self.btn_lang.config(text="Язык: RU")
            self.btn_custom.config(text="Свой Вариант")
            self.lbl_custom_mod.config(text="Папка мода (например cstrike, tfc):")
            self.lbl_path.config(text="Корневая папка Half-Life:")
            self.btn_browse.config(text="Обзор...")
            self.lbl_res.config(text="Масштаб интерфейса (Разрешение):")
            self.lbl_style.config(text="Форма прицела:")
            self.lbl_color.config(text="Цвет прицела:")
            self.btn_rgb.config(text="Палитра RGB...")
            self.lbl_prev.config(text="Предпросмотр:")
            self.btn_install.config(text="УСТАНОВИТЬ ПРИЦЕЛ")
            self.btn_restore.config(text="ВЕРНУТЬ СТАНДАРТНЫЙ")

            res_list, style_list, col_list = self.res_ru, self.styles_ru, self.colors_ru
        else:
            self.btn_lang.config(text="Lang: EN")
            self.btn_custom.config(text="Custom Mod")
            self.lbl_custom_mod.config(text="Mod folder name (e.g. cstrike, tfc):")
            self.lbl_path.config(text="Half-Life Root Directory:")
            self.btn_browse.config(text="Browse...")
            self.lbl_res.config(text="HUD Scale (Screen Resolution):")
            self.lbl_style.config(text="Crosshair Shape:")
            self.lbl_color.config(text="Crosshair Color:")
            self.btn_rgb.config(text="RGB Palette...")
            self.lbl_prev.config(text="Live Preview:")
            self.btn_install.config(text="INSTALL CROSSHAIR")
            self.btn_restore.config(text="RESTORE DEFAULT")

            res_list, style_list, col_list = self.res_en, self.styles_en, self.colors_en

        self.populate_menu(self.opt_res, self.var_res, res_list, self.on_res_select)
        self.populate_menu(self.opt_style, self.var_style, style_list, self.on_style_select)
        self.populate_menu(self.opt_color, self.var_color, col_list, self.on_color_select)

        self.var_res.set(res_list[self.res_idx])
        self.var_style.set(style_list[self.style_idx])
        self.var_color.set(col_list[self.col_idx])
        self.update_status_target()
        self.draw_preview()

    def auto_detect_path(self):
        curr = os.path.dirname(os.path.abspath(__file__))
        if os.path.isdir(os.path.join(curr, "valve")):
            self.txt_path.insert(0, curr)
            return

        home = os.path.expanduser("~")
        candidates = [
            os.path.join(home, ".steam/steam/steamapps/common/Half-Life"),
            os.path.join(home, ".local/share/Steam/steamapps/common/Half-Life"),
            os.path.join(home, ".var/app/com.valvesoftware.Steam/.local/share/Steam/steamapps/common/Half-Life"),
        ]
        for c in candidates:
            if os.path.isdir(c):
                self.txt_path.insert(0, c)
                return

    def browse_dir(self):
        d = filedialog.askdirectory()
        if d:
            self.txt_path.delete(0, "end")
            self.txt_path.insert(0, d)

    def pick_custom_color(self):
        rgb, _ = colorchooser.askcolor(color=self.custom_rgb)
        if rgb:
            r, g, b = int(rgb[0]), int(rgb[1]), int(rgb[2])
            if r == 0 and g == 0 and b == 255:
                b = 250
            self.custom_rgb = (r, g, b)
            self.col_idx = len(self.preset_rgb)
            col_list = self.colors_ru if self.is_russian else self.colors_en
            self.var_color.set(col_list[self.col_idx])
            self.draw_preview()

    def get_selected_rgb(self):
        if 0 <= self.col_idx < len(self.preset_rgb):
            return self.preset_rgb[self.col_idx]
        return self.custom_rgb

    def generate_mask(self):
        size = 64
        cx, cy = 32, 32
        mask = bytearray(size * size)

        radius = 0.0
        side_arcs_only = False
        dot_radius = -1
        v_span = 0

        st = self.style_idx
        if st == 0: radius = 20.0; dot_radius = 1
        elif st == 1: radius = 14.0; dot_radius = 1
        elif st == 2: radius = 9.0; dot_radius = 1
        elif st == 3: radius = 20.0
        elif st == 4: radius = 14.0
        elif st == 5: radius = 9.0
        elif st == 6: radius = 20.0; dot_radius = 1; side_arcs_only = True
        elif st == 7: radius = 14.0; dot_radius = 1; side_arcs_only = True
        elif st == 8: radius = 9.0; dot_radius = 1; side_arcs_only = True
        elif st == 9: v_span = 16
        elif st == 10: v_span = 11
        elif st == 11: v_span = 7
        elif st == 12: v_span = 12; dot_radius = 1
        elif st == 13: dot_radius = 2
        elif st == 14: dot_radius = 1

        if radius > 0:
            for y in range(size):
                for x in range(size):
                    dx = x - cx
                    dy = y - cy
                    dist = math.sqrt(dx * dx + dy * dy)
                    if abs(dist - radius) <= 1.0:
                        if not side_arcs_only or abs(dy) <= abs(dx):
                            mask[y * size + x] = 1

        if v_span > 0:
            vertex_y = (cy + 10) if (dot_radius >= 0) else (cy + v_span // 2)
            for i in range(v_span + 1):
                y = vertex_y - i
                xl = cx - i
                xr = cx + i
                for t in (0, 1):
                    if 0 <= y < size:
                        if 0 <= xl + t < size: mask[y * size + (xl + t)] = 1
                        if 0 <= xr - t < size: mask[y * size + (xr - t)] = 1

        if dot_radius >= 0:
            for dy in range(-dot_radius, dot_radius + 1):
                for dx in range(-dot_radius, dot_radius + 1):
                    mask[(cy + dy) * size + (cx + dx)] = 1

        return mask

    def draw_preview(self):
        self.canvas.delete("all")
        self.canvas.create_line(87, 0, 87, 150, fill="#1C1A16")
        self.canvas.create_line(0, 75, 175, 75, fill="#1C1A16")

        mask = self.generate_mask()
        r, g, b = self.get_selected_rgb()
        hex_col = f"#{r:02x}{g:02x}{b:02x}"

        zoom = 2
        ox = (175 - 64 * zoom) // 2
        oy = (150 - 64 * zoom) // 2

        for y in range(64):
            for x in range(64):
                if mask[y * 64 + x] == 1:
                    x0 = ox + x * zoom
                    y0 = oy + y * zoom
                    self.canvas.create_rectangle(x0, y0, x0 + zoom, y0 + zoom, outline="", fill=hex_col)

    def build_spr(self, mask, rgb):
        w, h = 64, 64
        header = struct.pack(
            "<4siiifiiifi",
            b"IDSP", 2, 2, 3,
            float(((w / 2.0) ** 2 + (h / 2.0) ** 2) ** 0.5),
            w, h, 1, 0.0, 0
        )
        pal = bytearray(768)
        pal[3:6] = bytes(rgb)
        pal[255 * 3:255 * 3 + 3] = b"\x00\x00\xff"
        pal_sec = struct.pack("<H", 256) + bytes(pal)
        frame_hdr = struct.pack("<iiiii", 0, -(w // 2), (h // 2), w, h)
        pixels = bytes([1 if b == 1 else 255 for b in mask])
        return header + pal_sec + frame_hdr + pixels

    def resolve_sprites_dir(self, base_dir, mod_folder):
        cand_mod = os.path.join(base_dir, mod_folder)
        cand_sprites = os.path.join(cand_mod, "sprites")
        if os.path.isdir(cand_sprites):
            return cand_sprites, cand_mod
        direct_sprites = os.path.join(base_dir, "sprites")
        if os.path.isdir(direct_sprites):
            return direct_sprites, base_dir
        return cand_sprites, cand_mod

    def install_crosshair(self):
        game_dir = self.txt_path.get().strip()
        mod_folder = self.get_target_mod_folder()
        sprites_dir, resolved_mod = self.resolve_sprites_dir(game_dir, mod_folder)

        if not os.path.isdir(sprites_dir):
            if os.path.isdir(resolved_mod):
                os.makedirs(sprites_dir, exist_ok=True)
            else:
                msg = f"Папка '{mod_folder}/sprites' не найдена!" if self.is_russian else f"Folder '{mod_folder}/sprites' not found!"
                messagebox.showerror("Error", msg)
                return

        mask = self.generate_mask()
        spr_bytes = self.build_spr(mask, self.get_selected_rgb())

        if self.res_idx == 1: targets = ["crosshairs.spr", "640crosshairs.spr"]
        elif self.res_idx == 2: targets = ["1280crosshairs.spr"]
        elif self.res_idx == 3: targets = ["2560crosshairs.spr"]
        else: targets = ["crosshairs.spr", "640crosshairs.spr", "1280crosshairs.spr", "2560crosshairs.spr"]

        try:
            valve_sprites = os.path.join(game_dir, "valve", "sprites")
            if os.path.isdir(valve_sprites) and os.path.abspath(sprites_dir) != os.path.abspath(valve_sprites):
                for fname in os.listdir(valve_sprites):
                    if fname.endswith(".txt"):
                        src_txt = os.path.join(valve_sprites, fname)
                        dst_txt = os.path.join(sprites_dir, fname)
                        if not os.path.isfile(dst_txt):
                            shutil.copy2(src_txt, dst_txt)

            for t in targets:
                dest = os.path.join(sprites_dir, t)
                bak = dest + ".bak"
                if os.path.isfile(dest) and not os.path.isfile(bak):
                    shutil.copy2(dest, bak)
                with open(dest, "wb") as f:
                    f.write(spr_bytes)

            rx = re.compile(r"^(\s*(?:crosshair|zoom|autoaim)\s+\d+\s+)\S+\s+\d+\s+\d+\s+\d+\s+\d+(.*)$", re.IGNORECASE | re.MULTILINE)
            for fname in os.listdir(sprites_dir):
                if fname.endswith(".txt"):
                    fpath = os.path.join(sprites_dir, fname)
                    bak_txt = fpath + ".bak"
                    src_read = bak_txt if os.path.isfile(bak_txt) else fpath
                    with open(src_read, "r", encoding="utf-8", errors="ignore") as f:
                        content = f.read()
                    if rx.search(content):
                        if not os.path.isfile(bak_txt):
                            shutil.copy2(fpath, bak_txt)
                        updated = rx.sub(r"\1crosshairs\t0\t0\t64\t64\2", content)
                        with open(fpath, "w", encoding="utf-8") as f:
                            f.write(updated)

            user_cfg = os.path.join(resolved_mod, "userconfig.cfg")
            if not os.path.isfile(user_cfg):
                with open(user_cfg, "w", encoding="utf-8") as f:
                    f.write('crosshair "1"\n')

            self.lbl_status.config(
                fg="#78FF78",
                text=f"Прицел установлен в [{mod_folder}]! Перезапустите игру." if self.is_russian else f"Crosshair installed into [{mod_folder}]! Please restart the game."
            )
            messagebox.showinfo(
                "Half-Life Crosshair Changer v5.1",
                f"Выбранный прицел установлен для [{mod_folder}]!" if self.is_russian else f"Selected crosshair installed for [{mod_folder}]!"
            )
        except Exception as e:
            messagebox.showerror("Error", str(e))

    def restore_backup(self):
        game_dir = self.txt_path.get().strip()
        mod_folder = self.get_target_mod_folder()
        sprites_dir, _ = self.resolve_sprites_dir(game_dir, mod_folder)
        if not os.path.isdir(sprites_dir):
            return
        restored = 0
        for fname in os.listdir(sprites_dir):
            if fname.endswith(".bak"):
                bak_path = os.path.join(sprites_dir, fname)
                orig_path = bak_path[:-4]
                shutil.copy2(bak_path, orig_path)
                restored += 1
        if restored > 0:
            self.lbl_status.config(
                fg=self.active_accent,
                text=f"Стандартный прицел [{mod_folder}] восстановлен." if self.is_russian else f"Default crosshair [{mod_folder}] restored."
            )
            messagebox.showinfo(
                "Half-Life Crosshair Changer v5.1",
                f"Стандартные прицелы [{mod_folder}] восстановлены." if self.is_russian else f"Default crosshairs [{mod_folder}] restored."
            )
        else:
            messagebox.showwarning(
                "Half-Life Crosshair Changer v5.1",
                "Резервные копии (.bak) не найдены." if self.is_russian else "No backup files (.bak) found."
            )

if __name__ == "__main__":
    root = tk.Tk()
    app = HL1CrosshairChangerLinux(root)
    root.mainloop()
