# Changelog

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
