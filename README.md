Xiaomi Note Exporter
![workflow](https://github.com/nogiszd/xiaomi-note-exporter/actions/workflows/build.yml/badge.svg) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) ![maintenance-status](https://img.shields.io/badge/maintenance-actively--developed-brightgreen.svg)
=================================

> Export your notes from Mi Cloud directly to Markdown file! 📝➡️🧾

🤔 Why?
------------
I wanted to export my own notes from Mi Cloud but there is no option for that. The only option was to rewrite the notes one by one. So I decided to make an app that would do it for me.

💁 How to use?
--------------------
**DISCLAIMER: This software will work only if you have your notes stored on [Mi Cloud](https://i.mi.com/)**

Download [latest release](https://github.com/nogiszd/xiaomi-note-exporter/releases/latest), extract the contents **to the same folder** and run `xiaomiNoteExporter.exe` executable.

For this to work you'll need `serviceToken` (session) and `userId`.  
As this software uses Selenium for automation, and thus it is Chrome WebDriver based, we need to provide session data manually to access cloud content. 

Here's how you can obtain them:

 1. Go to the https://i.mi.com and sign in.
 2. Choose Notes, when page is loaded press F12
 3. Go to the Application > Storage > Cookies and choose current URL
 4. Copy `serviceToken` and `userId` content and paste as prompted in application.
 5. **Do not close your browser as `serviceToken` is refreshing after every session is ended.**
 6. Wait until process is done, and you'll be left with _Markdown_ file with your notes exported!

⭐ Contribution
---------------------
Feel free to contribute to this open source project!  You can help by:

 - Forking this project
 - Creating your own branch
 - Commiting and opening pull requests

Just remember to check if everything work as intended.

> I would like to see your approach and implementations of my code in a different way 😄

📜 License
---------------
This repository is distributed mainly under the [MIT license](https://github.com/nogiszd/xiaomi-note-exporter/blob/master/LICENSE.txt). 

Used libraries:

 - [Selenium](https://www.selenium.dev/) - `Apache License 2.0`
 - [Pastel](https://github.com/silkfire/Pastel) - `MIT License`
 - [nupkg-selenium-webdriver-chromedriver](https://github.com/jsakamoto/nupkg-selenium-webdriver-chromedriver/) - `Unlicense`