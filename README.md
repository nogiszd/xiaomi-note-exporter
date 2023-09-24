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

**Prerequisites :**
 - **Windows machine with _.NET Framework 6.x.x_ installed**
 - **Latest version of Google Chrome browser installed**

Download [latest release](https://github.com/nogiszd/xiaomi-note-exporter/releases/latest), extract the contents **to the same folder** and run `xiaomiNoteExporter.exe` executable.

For this to work you'll need `serviceToken` (session) and `userId`.  
As this software uses Selenium for automation, and thus it is Chrome WebDriver based, we need to provide session data manually to access cloud content. 

Here's how you can obtain them:

 1. Go to the https://i.mi.com and sign in.
 2. Choose Notes, when page is loaded press F12
 3. Go to the Application > Storage > Cookies and choose current URL
 4. Copy `serviceToken` and `userId` content and paste as prompted in application.
 5. **Do not close your browser as `serviceToken` is refreshed after every session ends.**
 6. Wait until process is done, and you'll be left with _Markdown_ file with your notes exported!

Import to other apps
------------
After you have received your md file you will probably want to use it to import it into an application that does not restrict you to export the notes.
You can find some [good recommendations here](https://www.reddit.com/r/opensource/comments/gk1xl5/list_of_opensource_notetaking_softwares/)

In my case, I needed to import the md file into a new application (Simplenote) but I ran into a problem that I couldn't do it because applications don't support importing an md file or at least don't know how to split it according to the "****" that separates each note in the original file, and what happens It's that the file is imported as another large note into the app

Therefore, I created a script in Python that cuts each note into a new md file and gives you the option to choose which note to import and which not (assuming the application supports importing md files separately)

After that, I created another script in Python that is responsible for connecting all the md files to one json file and allowing an orderly import of all the files easily into the application without a problem

**Instructions for using scripts:**

**In order to split the notes into separate md files:**
1. Place the md file created by the software in folder (folder name not matter)
2. The file `markdown-splitter.py` must be edited and give it the path to the folder we created with the original md file and choose a folder to export the new notes to (further explanation appears in the comments of the code)
3. At the bottom of the code, **you must specify what the original file name of the md file is called**, it is advisable to change it to a simple name in order to avoid bugs


**In order to combine the md files into one big json file: (It is mandatory to do the previous section in order for it to work)**
1. The `markdown-to-json.py` file must be edited and given the path to the folder with all the split md files that the `markdown-splitter.py` script created for us in the folder we created earlier
2. Specify a path where to export the new json file that will be created
3. **A very important part must be read carefully** After the file is created, you must enter it and edit the file and add the opening and closing, for example in my case I used the Simplenote application and in order to check what the opening and closing of the json file is I created a temporary backup file in order Check what the opening and closing that the software requires
In my case I had to edit the json file so that the opening and closing would be as follows:

form: 

![image](https://github.com/aviv926/xiaomi-note-exporter/assets/51673860/d09dc1cc-96f7-4f35-b992-b7d045e66add)

To

![image](https://github.com/aviv926/xiaomi-note-exporter/assets/51673860/2dbb4b59-3e09-4eeb-be6c-02b210ad9de0)


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
