ADBKeyboard
===========

A useful Windows app for forwarding PC keyboard input to a connected Android device over ADB.

When testing Android apps, we often have to repeatedly enter textual infomation in via the keyboard (usernames, passwords etc). We can hard code things into the app or create fancy gradle scripts to avoid this but often we just don't have the time or expertise for this. 

This app allows you to direct your PC's keyboard input to a connected Android device via ADB. You can enter snippets of text into the macro boxes as well if you have pieces of text you commonly have to enter.

![alt tag](http://i.imgur.com/gLhoSd5.png)


How does it work? It simply opens a hidden cmd command prompt, creates a shell session with the device and makes use of the handy ADB commands "input keyevent" and "input text".
