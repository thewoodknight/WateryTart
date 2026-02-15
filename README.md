# WateryTart, a MediaAssistant Client
> Disclaimer: This is an unofficial project and is not affiliated with, endorsed by, or associated with the Music Assistant project.
> It is really just a proof of concept that has gotten out of hand - I'm very much a hobbyist coder, so I won't be surprised if I haven't somehow invented the definitive example of 'Worst Practices'

Please see the Issues for some of the things not yet implemented. There is a long list.

## Goals
This is aimed as a Plexamp-like experience for MediaAssistant, with a personal focus on Windows and Linux/RaspberryPi on a 5" touch screen.

While there is an Android project, this is mostly provided through Avalonia's crossplatform project setup and is currently not actively being tested/developed for Android.

Windows  
<img src="https://github.com/user-attachments/assets/d236796a-8e50-48ff-811b-0df6037d08de" width="300" /> <img src="https://github.com/user-attachments/assets/85e48cb2-788e-48ac-804d-069601fa1630" width="300" />

Raspberry Pi5 w/ Hifiberry Amp4  
<img src="https://github.com/user-attachments/assets/c75337c9-f48c-4ac3-a268-8214215ad99d" width="300" />
<img src="https://github.com/user-attachments/assets/e985a9c7-9882-4061-aaa9-4a04b1c84014" width="300" />
## Download & Install

### Requirements
- [.NET 10.0 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)

### Options
* Windows x64
* Linux x64
* Linux ARM x64 ie for Raspberry Pi 5

## Platform specific
Linux ARM x64 can take advantage of GPIO pins for rotary encoders for volume control. This currently uses PINs 17 and 27, with 20 pulses per rotation. Eventually this will all be configurable.

## Raspberry Pi Touch Screen
Raspbian's Desktop Environment doesn't implement touchscreen gestures like scrolling. As a result, if you're using a touchscreen, don't use the default Rasbian desktop environment. Instead, use something like [KDE Plasma](https://kde.org/plasma-desktop) or [GNOME](https://www.gnome.org/), which do support touchscreen gestures.


## Whats with the name?
WateryTart is uses Avalonia, which sounds like it comes from Camelot, and "You can't expect to wield supreme executive power just 'cause some watery tart threw a sword at you!".

## License

MIT License — see [LICENSE](LICENSE) for details.
