#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Half-Life 1 Crosshair Changer v5.0 (Linux Native Edition)
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
        self.root.title("Half-Life Crosshair Changer v5.0")
        self.root.geometry("600x485")
        self.root.resizable(False, False)

        # HL1 Classic Color Scheme
        self.bg_dark = "#181512"
        self.panel_bg = "#241F1A"
        self.amber = "#FFA000"
        self.amber_bright = "#FFC328"
        self.border_col = "#785014"
        self.text_white = "#F0E6D7"
        self.btn_primary = "#B95F00"

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
        self.style_idx = 6  # Default to Large Brackets (.) + Dot
        self.col_idx = 0

        self.build_ui()
        self.apply_language()
        self.auto_detect_path()

    def build_ui(self):
        # Header
        header = tk.Frame(self.root, bg="#120F0C", height=48, width=600)
        header.place(x=0, y=0)
        tk.Frame(self.root, bg=self.amber, height=2, width=600).place(x=0, y=46)

        tk.Label(
            header, text="HALF-LIFE  //  CROSSHAIR CHANGER",
            bg="#120F0C", fg=self.amber, font=("DejaVu Sans", 13, "bold")
        ).place(x=18, y=11)

        tk.Label(
            header, text="v5.0",
            bg="#120F0C", fg=self.border_col, font=("Monospace", 10, "bold")
        ).place(x=435, y=15)

        self.btn_lang = tk.Button(
            header, text="Lang: EN", bg=self.panel_bg, fg=self.amber,
            activebackground="#3C3020", activeforeground=self.amber_bright,
            highlightbackground=self.amber, relief="solid", bd=1,
            font=("DejaVu Sans", 8, "bold"), command=self.toggle_language
        )
        self.btn_lang.place(x=485, y=10, width=90, height=26)

        # Path
        self.lbl_path = tk.Label(self.root, bg=self.bg_dark, fg=self.amber, font=("DejaVu Sans", 9))
        self.lbl_path.place(x=20, y=60)

        self.txt_path = tk.Entry(
            self.root, bg=self.panel_bg, fg=self.text_white,
            insertbackground=self.amber, relief="solid", bd=1, font=("DejaVu Sans", 9)
        )
        self.txt_path.place(x=20, y=83, width=445, height=25)

        self.btn_browse = tk.Button(
            self.root, bg=self.panel_bg, fg=self.amber,
            activebackground="#3C3020", activeforeground=self.amber_bright,
            relief="solid", bd=1, font=("DejaVu Sans", 8, "bold"), command=self.browse_dir
        )
        self.btn_browse.place(x=475, y=82, width=100, height=26)

        # Resolution
        self.lbl_res = tk.Label(self.root, bg=self.bg_dark, fg=self.amber, font=("DejaVu Sans", 9))
        self.lbl_res.place(x=20, y=122)

        self.var_res = tk.StringVar()
        self.opt_res = tk.OptionMenu(self.root, self.var_res, "")
        self.style_option_menu(self.opt_res)
        self.opt_res.place(x=20, y=145, width=355, height=28)

        # Style
        self.lbl_style = tk.Label(self.root, bg=self.bg_dark, fg=self.amber, font=("DejaVu Sans", 9))
        self.lbl_style.place(x=20, y=184)

        self.var_style = tk.StringVar()
        self.opt_style = tk.OptionMenu(self.root, self.var_style, "")
        self.style_option_menu(self.opt_style)
        self.opt_style.place(x=20, y=207, width=355, height=28)

        # Color
        self.lbl_color = tk.Label(self.root, bg=self.bg_dark, fg=self.amber, font=("DejaVu Sans", 9))
        self.lbl_color.place(x=20, y=246)

        self.var_color = tk.StringVar()
        self.opt_color = tk.OptionMenu(self.root, self.var_color, "")
        self.style_option_menu(self.opt_color)
        self.opt_color.place(x=20, y=269, width=235, height=28)

        self.btn_rgb = tk.Button(
            self.root, bg=self.panel_bg, fg=self.amber,
            activebackground="#3C3020", activeforeground=self.amber_bright,
            relief="solid", bd=1, font=("DejaVu Sans", 8, "bold"), command=self.pick_custom_color
        )
        self.btn_rgb.place(x=265, y=269, width=110, height=28)

        # Preview
        self.lbl_prev = tk.Label(self.root, bg=self.bg_dark, fg=self.amber, font=("DejaVu Sans", 9))
        self.lbl_prev.place(x=400, y=122)

        self.canvas = tk.Canvas(
            self.root, width=170, height=150, bg="#0E0D0B",
            highlightbackground=self.border_col, highlightthickness=1
        )
        self.canvas.place(x=400, y=145)

        # Action Buttons
        self.btn_install = tk.Button(
            self.root, bg=self.btn_primary, fg="#FFFFFF",
            activebackground="#E17D0A", activeforeground="#FFFFFF",
            relief="solid", bd=1, font=("DejaVu Sans", 10, "bold"), command=self.install_crosshair
        )
        self.btn_install.place(x=20, y=322, width=275, height=44)

        self.btn_restore = tk.Button(
            self.root, bg=self.panel_bg, fg=self.amber,
            activebackground="#3C3020", activeforeground=self.amber_bright,
            relief="solid", bd=1, font=("DejaVu Sans", 9, "bold"), command=self.restore_backup
        )
        self.btn_restore.place(x=310, y=322, width=265, height=44)

        # Status
        self.lbl_status = tk.Label(self.root, bg=self.bg_dark, fg=self.amber_bright, font=("DejaVu Sans", 9))
        self.lbl_status.place(x=20, y=382)

        # Secret Bottom-Left Easter Egg Zone (5 clicks -> "Kosh=💩")
        self.lbl_easter = tk.Label(
            self.root, text="", bg=self.bg_dark, fg=self.amber_bright,
            font=("DejaVu Sans", 11, "bold"), anchor="w"
        )
        self.lbl_easter.place(x=18, y=425, width=160, height=32)
        self.lbl_easter.bind("<Button-1>", self.trigger_easter_egg)
        self.root.bind("<Button-1>", self.check_corner_click)

        # Credits
        tk.Label(
            self.root, text="By FFBeaver and Gemini", bg=self.bg_dark, fg=self.amber,
            font=("DejaVu Sans", 9, "bold"), anchor="e"
        ).place(x=380, y=430, width=195, height=24)

    def style_option_menu(self, opt):
        opt.config(
            bg=self.panel_bg, fg=self.text_white, activebackground="#3C3020",
            activeforeground=self.amber_bright, highlightthickness=1,
            highlightbackground=self.border_col, relief="flat", font=("DejaVu Sans", 9), anchor="w"
        )
        opt["menu"].config(
            bg=self.panel_bg, fg=self.text_white, activebackground=self.btn_primary,
            activeforeground="#FFFFFF", font=("DejaVu Sans", 9)
        )

    def check_corner_click(self, event):
        if event.widget == self.root and event.x <= 150 and event.y >= 415:
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
            self.lbl_path.config(text="Директория игры Half-Life:")
            self.btn_browse.config(text="Обзор...")
            self.lbl_res.config(text="Масштаб интерфейса (Разрешение):")
            self.lbl_style.config(text="Форма прицела:")
            self.lbl_color.config(text="Цвет прицела:")
            self.btn_rgb.config(text="Палитра RGB...")
            self.lbl_prev.config(text="Предпросмотр:")
            self.btn_install.config(text="УСТАНОВИТЬ ПРИЦЕЛ")
            self.btn_restore.config(text="ВЕРНУТЬ СТАНДАРТНЫЙ")
            self.lbl_status.config(text="Система готова к установке прицела.")

            res_list, style_list, col_list = self.res_ru, self.styles_ru, self.colors_ru
        else:
            self.btn_lang.config(text="Lang: EN")
            self.lbl_path.config(text="Half-Life Game Directory:")
            self.btn_browse.config(text="Browse...")
            self.lbl_res.config(text="HUD Scale (Screen Resolution):")
            self.lbl_style.config(text="Crosshair Shape:")
            self.lbl_color.config(text="Crosshair Color:")
            self.btn_rgb.config(text="RGB Palette...")
            self.lbl_prev.config(text="Live Preview:")
            self.btn_install.config(text="INSTALL CROSSHAIR")
            self.btn_restore.config(text="RESTORE DEFAULT")
            self.lbl_status.config(text="System ready to install crosshair.")

            res_list, style_list, col_list = self.res_en, self.styles_en, self.colors_en

        self.populate_menu(self.opt_res, self.var_res, res_list, self.on_res_select)
        self.populate_menu(self.opt_style, self.var_style, style_list, self.on_style_select)
        self.populate_menu(self.opt_color, self.var_color, col_list, self.on_color_select)

        self.var_res.set(res_list[self.res_idx])
        self.var_style.set(style_list[self.style_idx])
        self.var_color.set(col_list[self.col_idx])
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
        self.canvas.create_line(85, 0, 85, 150, fill="#1C1A16")
        self.canvas.create_line(0, 75, 170, 75, fill="#1C1A16")

        mask = self.generate_mask()
        r, g, b = self.get_selected_rgb()
        hex_col = f"#{r:02x}{g:02x}{b:02x}"

        zoom = 2
        ox = (170 - 64 * zoom) // 2
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

    def install_crosshair(self):
        game_dir = self.txt_path.get().strip()
        sprites_dir = os.path.join(game_dir, "valve", "sprites")
        if not os.path.isdir(sprites_dir):
            msg = "Папка 'valve/sprites' не найдена! Проверьте путь к Half-Life." if self.is_russian else "Folder 'valve/sprites' not found! Please check your Half-Life directory."
            messagebox.showerror("Error", msg)
            return

        mask = self.generate_mask()
        spr_bytes = self.build_spr(mask, self.get_selected_rgb())

        if self.res_idx == 1: targets = ["crosshairs.spr", "640crosshairs.spr"]
        elif self.res_idx == 2: targets = ["1280crosshairs.spr"]
        elif self.res_idx == 3: targets = ["2560crosshairs.spr"]
        else: targets = ["crosshairs.spr", "640crosshairs.spr", "1280crosshairs.spr", "2560crosshairs.spr"]

        try:
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

            user_cfg = os.path.join(game_dir, "valve", "userconfig.cfg")
            if not os.path.isfile(user_cfg):
                with open(user_cfg, "w", encoding="utf-8") as f:
                    f.write('crosshair "1"\n')

            self.lbl_status.config(
                fg="#78FF78",
                text="Прицел успешно установлен! Перезапустите Half-Life." if self.is_russian else "Crosshair installed successfully! Please restart Half-Life."
            )
            messagebox.showinfo(
                "Half-Life Crosshair Changer",
                "Выбранный прицел успешно установлен!" if self.is_russian else "Selected crosshair has been installed!"
            )
        except Exception as e:
            messagebox.showerror("Error", str(e))

    def restore_backup(self):
        sprites_dir = os.path.join(self.txt_path.get().strip(), "valve", "sprites")
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
                fg=self.amber_bright,
                text="Стандартный прицел успешно восстановлен." if self.is_russian else "Default crosshair restored successfully."
            )
            messagebox.showinfo(
                "Half-Life Crosshair Changer",
                "Стандартные прицелы восстановлены из резервной копии." if self.is_russian else "Default Half-Life crosshairs restored from backup."
            )
        else:
            messagebox.showwarning(
                "Half-Life Crosshair Changer",
                "Резервные копии (.bak) не найдены." if self.is_russian else "No backup files (.bak) found."
            )

if __name__ == "__main__":
    root = tk.Tk()
    app = HL1CrosshairChangerLinux(root)
    root.mainloop()
