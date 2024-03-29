Xiaomi Note Exporter
![workflow](https://github.com/nogiszd/xiaomi-note-exporter/actions/workflows/build.yml/badge.svg) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) ![maintenance-status](https://img.shields.io/badge/maintenance-mainenance--mode-yellow.svg)
=================================

> Export your notes from Mi Cloud directly to Markdown file! 📝➡️🧾

🔧 Maintenance mode
------------
**I dont have much time to maintain this repository, and I don't use Mi Cloud myself no more. That's why I'm placing this project in a maintenance mode. This means that I will fix any major bugs, if any will occur, and update libraries that this app is using, but there won't be any feature updates soon.**

🤔 Why?
------------
I wanted to export my own notes from Mi Cloud but there is no option for that. The only option was to rewrite the notes one by one. So I decided to make an app that would do it for me.

💁 How to use?
--------------------
**DISCLAIMER: This software will work only if you have your notes stored on [Mi Cloud](https://i.mi.com/)**

**Prerequisites :**
 - **Windows machine with _.NET Framework 6.x.x_ installed**
 - **Latest version of Google Chrome browser installed**

Download [latest release](https://github.com/nogiszd/xiaomi-note-exporter/releases/latest), extract the contents **to the same folder** and run `xiaomiNoteExporter.exe` executable.

**Steps how to use**:
 1. Launch the app - you will be prompted to input domain address (if it differs from the default one).
 2. Sign into your account via browser window that popped up.
 3. After succeeding, press any key as requested.
 4. Wait until process is done, and you'll be left with _Markdown_ file with your notes exported!

🔗 Importing into other apps (Advanced)
---------------
There is an **[fork](https://github.com/aviv926/xiaomi-note-exporter)** with additional scripts written in Python which are made to split complete markdown file into one per note, and convert them into JSON format file. 

More details are described in this fork's `readme.md`.

Credits are going to [aviv926](https://github.com/aviv926)

📜 License
---------------
This repository is distributed mainly under the [MIT license](https://github.com/nogiszd/xiaomi-note-exporter/blob/master/LICENSE.txt). 

Used libraries:

 - [Selenium](https://www.selenium.dev/) - `Apache License 2.0`
 - [Pastel](https://github.com/silkfire/Pastel) - `MIT License`
