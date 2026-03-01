Xiaomi Note Exporter
![workflow](https://github.com/nogiszd/xiaomi-note-exporter/actions/workflows/publish.yml/badge.svg) [![License: MIT](https://img.shields.io/badge/License-GPL3.0-yellow.svg)](https://opensource.org/licenses/MIT) ![maintenance-status](https://img.shields.io/badge/maintenance-active-success.svg)
=================================

> Export your notes from Mi Cloud directly to Markdown file! 📝➡️🧾

### If you like the app, consider supporting the project ⭐

## 🤔 Why?

I wanted to export my own notes from Mi Cloud but there is no option for that. The only option was to rewrite the notes one by one. So I decided to make an app that would do it for me.

## [💾 Download](https://github.com/nogiszd/xiaomi-note-exporter/releases/latest)

### You can find help in [Wikis](https://github.com/nogiszd/xiaomi-note-exporter/wiki), or use direct links below:

* ### [💻 Setup and Requirements](https://github.com/nogiszd/xiaomi-note-exporter/wiki/Setup)
* ### [💁 How to use?](https://github.com/nogiszd/xiaomi-note-exporter/wiki/How-to-use)
* ### [✨ Features](https://github.com/nogiszd/xiaomi-note-exporter/wiki/Features)
* ### [❓ FAQ](https://github.com/nogiszd/xiaomi-note-exporter/wiki/FAQ)

---

## 🦀 Rust rewrite

Version `v2.0.0` is a complete rewrite of the application in Rust. The project has evolved from a CLI tool into a fully-featured graphical application.

This release marks a major development milestone, significantly streamlining the export process by eliminating redundant dependencies:

- Selenium has been removed and replaced with a pure JavaScript implementation that replicates the previous behavior.
- The entire application is now built with Tauri, providing a modern GUI and fully asynchronous export execution.
- The application is distributed as a standalone, installable executable, requiring no external runtime or dependencies.

Old code (C#) will be available on [legacy](https://github.com/nogiszd/xiaomi-note-exporter/tree/legacy) branch, and old executables are still available to download via **Releases**, but they're **out of support**.


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
