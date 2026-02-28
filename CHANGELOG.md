# Changelog

## [1.5.0](https://github.com/TemuWolverine/WateryTart/compare/v1.4.0...v1.5.0) (2026-02-28)


### Features

* added Home2View/Model, a redesigned Home page with discovery lists, much quicker loading than the recommendations. ([7549da6](https://github.com/TemuWolverine/WateryTart/commit/7549da6d8336ed128c49690b638600885a824eca))
* **AlbumView:** Album view now looks cleaner, has slightly more functionality. ([16e58a4](https://github.com/TemuWolverine/WateryTart/commit/16e58a4d970e71efa8c685b9f0bc597bdc7977a8))
* **Artist:** Artist profile now prettier, has more functional buttons (play, (un)favourite) ([1e6f6d9](https://github.com/TemuWolverine/WateryTart/commit/1e6f6d9ce2c39bb161a6dee659b7adad3a90c48f))
* more improvements to Home2View, with it now correctly showing  discovery album/artist ([703d6ee](https://github.com/TemuWolverine/WateryTart/commit/703d6ee2745e8aabebba55e6dac2814fc281d162))
* **Playback:** New experimental playback engine for Windows, powered by SoundFlow. If it turns out to be acceptable, it should make its way to other platforms too. ([9a0e755](https://github.com/TemuWolverine/WateryTart/commit/9a0e7553fcb420ce2e6fccb143b1853334f9ec09))
* **Visuals:** Players tray now looks significantly nicer. ([95ed02e](https://github.com/TemuWolverine/WateryTart/commit/95ed02e847efc62ba22c784cbe3aec6cb25ebe39))
* **Visuals:** Settings reworked to be much easier on the eyes and more logically grouped. ([99cf9f0](https://github.com/TemuWolverine/WateryTart/commit/99cf9f0c7c30c9d8be52677980e01508d7be73dd))
* **Visuals:** Visual cleanup, button consistency (radio, alt menu) & big player view improvements ([917d9a9](https://github.com/TemuWolverine/WateryTart/commit/917d9a960d78a5e26c598de6f8787777890bb5c9))


### Bug Fixes

* Addressed clickability of individual items on the home page. ([fde2a29](https://github.com/TemuWolverine/WateryTart/commit/fde2a2945f44b1035c0549b93035ac9a478b7d82))
* album view prettified ([94da85c](https://github.com/TemuWolverine/WateryTart/commit/94da85cccec239ca452df58aef62e4a38ed426bf))
* albums won't reload tracks if the tracks had previously been loaded. ([2564e6f](https://github.com/TemuWolverine/WateryTart/commit/2564e6fad7c9d9878de08cce5238bdc4202ac1c1))
* artist view now has a dropshadow ([f8acf55](https://github.com/TemuWolverine/WateryTart/commit/f8acf5505bf6c4b8ac9f5495c5767d5cc5c1f708))
* big player now correctly loads albums ([465023b](https://github.com/TemuWolverine/WateryTart/commit/465023b7a1fa8a8c9f7bf77543fc06f90a41a5db))
* build script should now use native-aot for windows ([57fcced](https://github.com/TemuWolverine/WateryTart/commit/57fcced4258287844cf247f55cf7a49eebbc3384))
* cleanup of windows platform's players ([9a341d7](https://github.com/TemuWolverine/WateryTart/commit/9a341d759b4fc9ab4fc94399c4e241db08ce643b))
* consistency in settings navigation visibility ([e249de6](https://github.com/TemuWolverine/WateryTart/commit/e249de68ec5cdd1b017b3a3afd6836aa1d5d21e4))
* corrected artist details not loading ([d8ee5fc](https://github.com/TemuWolverine/WateryTart/commit/d8ee5fcde584914bb17bb001a16bb80a4dde2ced))
* corrected IsLoading on several viewmodels ([0f5f36e](https://github.com/TemuWolverine/WateryTart/commit/0f5f36e8c4c62e065159ba068daf96372bd951e1))
* fixed linux-x64 for aot mode ([a31f3f3](https://github.com/TemuWolverine/WateryTart/commit/a31f3f39472300cdf55608bdb2614df7aaa281bf))
* Home2View uses a reusable template now ([205006a](https://github.com/TemuWolverine/WateryTart/commit/205006a6a0d575932a8f5298e18dac36d9b0e955))
* improved the base vm class, implemented it more ([5fa1d84](https://github.com/TemuWolverine/WateryTart/commit/5fa1d848a0cbe73af7fd2d85f7894171a3b84201))
* Library view is prettfied ([7715440](https://github.com/TemuWolverine/WateryTart/commit/77154404e8aaae53e5a56fa9c58931b69fab2f77))
* Linux publish profiles more correcter ([2a3964b](https://github.com/TemuWolverine/WateryTart/commit/2a3964b83ec4f2f575d7887b2c932ccf569e94f7))
* linux-arm64 build fixes ([7342f5f](https://github.com/TemuWolverine/WateryTart/commit/7342f5f3fc22e6e9c837f36a0c4c778607786946))
* linuxarm64 presents issues with AOT currently. Fixed up the GPIO service causing crashes ([09e1678](https://github.com/TemuWolverine/WateryTart/commit/09e1678d063b6a45b08fe51de24b9d56da71b58f))
* More players tray visual tweaks ([46799ca](https://github.com/TemuWolverine/WateryTart/commit/46799cad722c19ca8acc796ee586f0f9494ed1a6))
* **Playback:** Address WASAPI exception issues ([087bf94](https://github.com/TemuWolverine/WateryTart/commit/087bf9400984d77eefd73cf5b57eb9b97d1c3c28))
* **Playback:** more MVVM ([e1183dd](https://github.com/TemuWolverine/WateryTart/commit/e1183dd9e1de94d454112960e3f4f5020eacd7b8))
* playlist view prettified ([0262beb](https://github.com/TemuWolverine/WateryTart/commit/0262bebef402098df4b8a450fc8ad7c8fa836358))
* removed old homeview/model, added default svg (blank) icon ([37e5945](https://github.com/TemuWolverine/WateryTart/commit/37e59458daa82d50821f63cadd489aef2e92339b))
* the big one. This addresses nearly all classes, cleaning up old code, reducing build warnings and messages, and switches to use teh new ViewModelBase where appropriate. ([893b3a2](https://github.com/TemuWolverine/WateryTart/commit/893b3a2bb99c58603309d22c4a836d691b80598b))
* **Visuals:** more restyling ([5a80f85](https://github.com/TemuWolverine/WateryTart/commit/5a80f85aecd76ae5a18e6033a97030e8e80b0dfd))
* XAML cleanup - moved repeated resources into single source of truth, split out of app.xaml into Converters, DataTemplates, GlobalResourceStyles, Styles.axaml ([fabdfcb](https://github.com/TemuWolverine/WateryTart/commit/fabdfcbf27e1f8d8dde9613e904322e0776ace42))

## [1.4.0](https://github.com/TemuWolverine/WateryTart/compare/v1.3.1...v1.4.0) (2026-02-24)


### Features

* Added shuffle from BigPlayerView ([c19f0b8](https://github.com/TemuWolverine/WateryTart/commit/c19f0b8e7a83d464c8a9845970d6ddde473b4271))
* **AlbumView:** Album view now lets you play album radio, shows the provider source icon, and tapping artist name takes you to the artist page ([038829a](https://github.com/TemuWolverine/WateryTart/commit/038829ab0dbb8f9db4dd382075e32bdf57c04b9a))
* **AlbumView:** Loading animation for visual feedback when loading album data ([9015c4a](https://github.com/TemuWolverine/WateryTart/commit/9015c4aae7e9ec033c3321aecb28ea09ad4aa768))
* **ArtistView:** Artist view now uses tabs for album list and bio, as long discographies made finding the bio impossible. ([13fd160](https://github.com/TemuWolverine/WateryTart/commit/13fd160c713a80ee678cf01e3e84175aeff022e8))
* Cycle repeat mode in big player, ([15fae06](https://github.com/TemuWolverine/WateryTart/commit/15fae0625032b74ace8b2a6468f30ba843bd63ea))


### Bug Fixes

* **ArtistView:** Corrected radio icon to radiotower ([56d8892](https://github.com/TemuWolverine/WateryTart/commit/56d88925edc570ed76dfc2cea459f59cccdce142))
* **GPIOVolume:** (Hopefully) have settings being saved/recalled for GPIO volume service so pins, rotation steps, enabled can all work properly. ([667dfbf](https://github.com/TemuWolverine/WateryTart/commit/667dfbf4a721b7a08af7ce3e09e04e2b436bcbbe))
* **GPIOVolume:** Linux x64 shouldn't be effected by Linux Arm64's GPIO service now ([f115729](https://github.com/TemuWolverine/WateryTart/commit/f115729ced1752bada42beb4e32dc0a2ac17c1e2))
* Solves [#28](https://github.com/TemuWolverine/WateryTart/issues/28), Volume changes to 100% when switching back to player drawer if currently selected player ([3591f4e](https://github.com/TemuWolverine/WateryTart/commit/3591f4e61dbc3d06fc8f194cdc5f5dc975e4574e))
* **TrayIcon:** Tray icon now correctly saves if its enabled state ([e94b554](https://github.com/TemuWolverine/WateryTart/commit/e94b554143914dd9444462693dc280f3c061b437))

## [1.3.1](https://github.com/TemuWolverine/WateryTart/compare/v1.3.0...v1.3.1) (2026-02-21)


### Bug Fixes

* nuked macOS build as it keeps failing due to space constraints ([758a873](https://github.com/TemuWolverine/WateryTart/commit/758a8731335c805dbd73a5a76e153f91fa3e89ab))

## [1.3.0](https://github.com/TemuWolverine/WateryTart/compare/v1.2.0...v1.3.0) (2026-02-20)


### Features

* **volume:** volume now should debounce enough that it doesn't get into weird race conditions ([b03b6b8](https://github.com/TemuWolverine/WateryTart/commit/b03b6b8a7e26b0547ce0cab5bf5041f05d67776c))


### Bug Fixes

* Lambda Registrations actually apply properly now ([9280887](https://github.com/TemuWolverine/WateryTart/commit/92808877db0ecbad8d99719ad3dace0b9f07465d))
* Logger now sets a default folder ([a3a1ed5](https://github.com/TemuWolverine/WateryTart/commit/a3a1ed59e9a966f905432d97f2bcefa1f94ecf21))
* Settings reorganised ([306f950](https://github.com/TemuWolverine/WateryTart/commit/306f950255986cc677b9d1b165b3e72df195f0a6))
* Settings view now loads without issue ([02493d3](https://github.com/TemuWolverine/WateryTart/commit/02493d3f77a64da5f19f523141a4aa00a66c2ce7))
* switched to IconPacks.Avalonia ([a6802b8](https://github.com/TemuWolverine/WateryTart/commit/a6802b80f8409d35709fdca9150e6e9c289b14b2))
* Tray icon now optional ([5589fde](https://github.com/TemuWolverine/WateryTart/commit/5589fdee1871900d0c08eb6166b559ac481993e5))

## [1.2.0](https://github.com/TemuWolverine/WateryTart/compare/v1.1.0...v1.2.0) (2026-02-19)


### Features

* App will now be restricted to a single instance ([21d7e46](https://github.com/TemuWolverine/WateryTart/commit/21d7e46aaa50b8bdf8218c1dfed2e68a0dcb8d17))
* **PlayersTray:** Can control the volume of any items in the player tray ([cb44fc9](https://github.com/TemuWolverine/WateryTart/commit/cb44fc9a1e4d44347486e9556e95bad73713ddaf))
* **PlayersTray:** Players Tray addresses much of [#15](https://github.com/TemuWolverine/WateryTart/issues/15). Player service can now clear queue, enable dont-stop-music, enable shuffle ([0eac782](https://github.com/TemuWolverine/WateryTart/commit/0eac78235b3069ea0fda14bc9c8907e8c59273fa))


### Bug Fixes

* addressed many of the build warnings ([641ab49](https://github.com/TemuWolverine/WateryTart/commit/641ab4930beffcfc2f18cb7d154a77415ab634f4))
* basic handling of queue added event ([644a24f](https://github.com/TemuWolverine/WateryTart/commit/644a24f2dc8c5e6ac4b87be5230d69d4c4fd33e6))
* Converter now checks for null, so when music stops it doesn't crash ([63519fd](https://github.com/TemuWolverine/WateryTart/commit/63519fd4141e4629f493c41c719fd2ddbdc935f6))
* some fixes for playing from external services (ie, sonos playing on speakers that are connected to MA) ([8f85816](https://github.com/TemuWolverine/WateryTart/commit/8f858165e16f996dbbbbc5bab9c1c607f0a537bf))

## [1.1.0](https://github.com/TemuWolverine/WateryTart/compare/v1.0.11...v1.1.0) (2026-02-18)


### Features

* **streamdetails:** began work on trackinfoview - a more details/MA way of showing the streams details from input -&gt; format -&gt; effects -&gt; output ([340b7b4](https://github.com/TemuWolverine/WateryTart/commit/340b7b49df9bc02275d08716cb5b6a88e5118efb))
* **streamdetails:** brought the stream quality badges inline with how MusicAssistant presents it with LQ/HQ/HiRes. Introduced ProviderService to better access all the provider icons/information ([7f8d890](https://github.com/TemuWolverine/WateryTart/commit/7f8d89009fe11df5930fe8c5940d0b8b0bbde324))


### Bug Fixes

* default playmode now set to replace instead of play ([75e4d98](https://github.com/TemuWolverine/WateryTart/commit/75e4d9846b9360eaca2cb89fafbd81417d6c1c76))
* Missed some references for iplayerservice -&gt; playerservice rename ([b952f76](https://github.com/TemuWolverine/WateryTart/commit/b952f76b3819a6380e80f9571f6744e5fe4e9d23))
* Playlists view (from library) no longer crashes ([1cb2687](https://github.com/TemuWolverine/WateryTart/commit/1cb26874fb6f00eb089ab5d7cd9e14f95f2176b8))
* Reworked menu popup to take a more generic IPopupViewModel, allowing for menus or other info popups to slide up from the bottom of the screen ([b8aa872](https://github.com/TemuWolverine/WateryTart/commit/b8aa872687be6c8bf47f8952d670e0ed03ab2146))

## [1.0.11](https://github.com/TemuWolverine/WateryTart/compare/v1.0.10...v1.0.11) (2026-02-15)


### Bug Fixes

* stopped being gaslit by copilot ([3bc1e20](https://github.com/TemuWolverine/WateryTart/commit/3bc1e208c0fb8875839195d2b3ba29187c103bdf))

## [1.0.10](https://github.com/TemuWolverine/WateryTart/compare/v1.0.9...v1.0.10) (2026-02-15)


### Bug Fixes

* Switch to PAT to hopefully trigger things ([04296d0](https://github.com/TemuWolverine/WateryTart/commit/04296d02a89efbd1284ee6ce58dfcb2211ec85fc))

## [1.0.9](https://github.com/TemuWolverine/WateryTart/compare/v1.0.8...v1.0.9) (2026-02-15)


### Bug Fixes

* revert issues with build, split into two files. ([ddd8906](https://github.com/TemuWolverine/WateryTart/commit/ddd8906e7caea7a00e4fc12cded71fc6e0515411))

## [1.0.8](https://github.com/TemuWolverine/WateryTart/compare/v1.0.7...v1.0.8) (2026-02-15)


### Features

* Added global "loading" animation so things aren't so empty when data is loading ([bae55cd](https://github.com/TemuWolverine/WateryTart/commit/bae55cd8a3884b5f26bffa73532caccb1866c0af))
* Repeat mode can now be set from BigPlayerView, though there is no visual indicator ([d8757a0](https://github.com/TemuWolverine/WateryTart/commit/d8757a013b35147bfb89eeb605314948a6d09f18))


### Bug Fixes

* build-script tag should be correctly set now ([f25e40d](https://github.com/TemuWolverine/WateryTart/commit/f25e40d7ee7bf7389144b3cf73d5523d26b716ae))
* Null Menu commands won't cause the menu to crash or close ([e65c7fe](https://github.com/TemuWolverine/WateryTart/commit/e65c7feb67a486d33b2534fd76bd3a03d6f92050))
* Updated package ([0a8478f](https://github.com/TemuWolverine/WateryTart/commit/0a8478f85dd81330f5da8595aa8c3f5caccc15a9))

## [1.0.7](https://github.com/TemuWolverine/WateryTart/compare/1.0.6...v1.0.7) (2026-02-15)


### Bug Fixes

* Fixed small player view's play/pause button ([76e515d](https://github.com/TemuWolverine/WateryTart/commit/76e515dfe354ebca90c01e7be143e667aefa3eff))
