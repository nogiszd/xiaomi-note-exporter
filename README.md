Xiaomi Note Exporter
![workflow](https://github.com/nogiszd/xiaomi-note-exporter/actions/workflows/publish.yml/badge.svg) [![License: MIT](https://img.shields.io/badge/License-GPL3.0-yellow.svg)](https://opensource.org/licenses/MIT) ![maintenance-status](https://img.shields.io/badge/maintenance-active-success.svg)
=================================

> Export your notes from Mi Cloud directly to Markdown file! 📝➡️🧾

### If you like the app, consider supporting the project ⭐

## 🤔 Why?

I wanted to export my own notes from Mi Cloud but there is no option for that. The only option was to rewrite the notes one by one. So I decided to make an app that would do it for me.

## 💁 How to use?

**DISCLAIMER: This software will work only if you have your notes stored on [Mi Cloud](https://i.mi.com/)**

### Prerequisites :

- **Windows machine (MacOS/Linux support coming soon)**

**Download [latest release](https://github.com/nogiszd/xiaomi-note-exporter/releases/latest)**, install the app as prompted, and launch.

![Export tab](https://i.imgur.com/R3P98i7.png)

### Normal usage:

1.  Launch the app - you will be greeted with main screen.
2.  Main tab allows you to input a Mi Cloud domain (if it differs than default one), choose if you want to split notes, and whether you want to export the images.
3.  Pressing `Start Export` will open another window. You should sign-in to your Mi Cloud account.
4.  After succeeding, app will automatically start to export your notes. Wait until process is done, and you'll be left with _Markdown_ file with your notes exported!
5.  The session is persistent, so if you decide to export again, you shouldn't be asked to sign-in again.

### _Warning!_

Installer **can trigger** Windows SmartScreen because the code is unsigned.  
In that case you can click **More info** and **Run anyway**.

## 🦀 Rust rewrite

Version `v2.0.0` is a complete rewrite of the application in Rust. The project has evolved from a CLI tool into a fully-featured graphical application.

This release marks a major development milestone, significantly streamlining the export process by eliminating redundant dependencies:

- Selenium has been removed and replaced with a pure JavaScript implementation that replicates the previous behavior.
- The entire application is now built with Tauri, providing a modern GUI and fully asynchronous export execution.
- The application is distributed as a standalone, installable executable, requiring no external runtime or dependencies.

Old code (C#) will be available on [legacy](https://github.com/nogiszd/xiaomi-note-exporter/tree/legacy) branch, and old executables are still available to download via **Releases**, but they're **out of support**.

## 🗓️ History:

App allows you to see history of your exports.  
Files and export results are stored in OS's default Documents directory.

Below are defaults that are used by the app:

```
Windows: C:\Users\<username>\Documents
Linux: XDG_DOCUMENTS_DIR
MacOS: $HOME/Documents
```

![History tab](https://i.imgur.com/HKBbi7B.png)

App creates a folder named `Xiaomi Note Exporter` in your default Documents directory.

Each export has few actions, that are described below:

1. View - you can preview the markdown file (or multiple if export was selected as "split"), and here you can make manual edits, or convert them to JSON.
2. Open directory - it will open directory where the file/folder is stored.
3. Delete - you can delete the entry from the history, pop-up will ask you if you want to delete produced files too.

## 🗒️ Note splitting

On `Export` tab there is an option to select if your notes should be splitted into separate files.

If so, an directory will be created and each note will be stored in separate file.

This option shows another field to input specific format for timestamp - **but it must be compatible with [.NET specification](https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings)**.

## 🖼️ Image export

As it is with note splitting, there is also an option to decide whether images stored in the notes should be exported along the notes.

It is selected by default, but user can choose not to export them.

## 🔗 Convert to JSON

Third `Converter` tab is a tool that allows user to select a file (or folder if notes were split) and convert them to JSON format.

![Converter tab](https://i.imgur.com/aFFjwCr.png)

## ⚙️ Settings

App has built in settings tab, that allows user to choose default export directory (instead of default created in Documents folder), and to choose theme (default is system deferred).

This tab also has an update checker, for user convenience, without leaving app, you can check if your app is up to date.

![Settings tab](https://i.imgur.com/Lyaoldl.png)

---

## 🤝 Open-source contributions

This project is developed and maintained **independently**.
Contributions are very welcome and appreciated!

If you’d like to help improve this project, you can:

- report bugs or suggest improvements via GitHub Issues,
- submit pull requests (bug fixes, refactors, documentation),
- improve documentation or examples.

Whether it’s a small typo fix or a larger feature proposal — every contribution helps.
Please keep changes focused and well-documented.

Thank you for helping make this project better ❤️

## ❤️ Support this project

This project is **free and open source** and will remain that way.

If you find it useful and want to support its development, you can do so voluntarily:

- ⭐ Star the repository
- 💖 Sponsor the project on GitHub

Sponsorship is **100% optional** and does not unlock or restrict any features.
It simply helps cover spent time, maintenance, and future improvements.

Thank you for your support!

## 📜 License

This project is licensed under the [GNU General Public License v3.0](https://github.com/nogiszd/xiaomi-note-exporter/blob/master/LICENSE.txt).

Used libraries:

- [Tauri](https://tauri.app/) - [MIT License](https://opensource.org/licenses/MIT)
- [Vue 3](https://vuejs.org/) - [MIT License](https://opensource.org/licenses/MIT)
- [Vite](https://vite.dev/) - [MIT License](https://opensource.org/licenses/MIT)
- [Tailwind CSS](https://tailwindcss.com/) - [MIT License](https://opensource.org/licenses/MIT)
- [shadcn-vue](https://www.shadcn-vue.com/) - [MIT License](https://opensource.org/licenses/MIT)
- [Pinia](https://pinia.vuejs.org/) - [MIT License](https://opensource.org/licenses/MIT)
- [Vue Router](https://router.vuejs.org/) - [MIT License](https://opensource.org/licenses/MIT)
- [md-editor-v3](https://imzbf.github.io/md-editor-v3/) - [MIT License](https://opensource.org/licenses/MIT)
- [rusqlite](https://github.com/rusqlite/rusqlite) - [MIT License](https://opensource.org/licenses/MIT)
- [reqwest](https://github.com/seanmonstar/reqwest) - [MIT License](https://opensource.org/licenses/MIT)
- [chrono](https://github.com/chronotope/chrono) - [MIT License](https://opensource.org/licenses/MIT)
