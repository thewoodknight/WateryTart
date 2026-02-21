# Changelog

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
