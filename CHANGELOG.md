# Changelog

## [2.3.7](https://github.com/iPromKnight/zilean/compare/v2.3.6...v2.3.7) (2024-11-17)


### 🔥 Bug Fixes

* log batch import error ([b325076](https://github.com/iPromKnight/zilean/commit/b325076593e877343efbf360a8cb982d79e5bb75))

## [2.3.6](https://github.com/iPromKnight/zilean/compare/v2.3.5...v2.3.6) (2024-11-17)


### 🔥 Bug Fixes

* db indexes for new columns ([bab4059](https://github.com/iPromKnight/zilean/commit/bab405917d3d27cf8eab53bee3040b3b63fd58fb))

## [2.3.5](https://github.com/iPromKnight/zilean/compare/v2.3.4...v2.3.5) (2024-11-17)


### 🔥 Bug Fixes

* broken imdbid ([135d389](https://github.com/iPromKnight/zilean/commit/135d389b9fc50c4d7082f66643a1f8ac3be59052))
* remove warnings from dataprotection stack ([1a11e54](https://github.com/iPromKnight/zilean/commit/1a11e5414e5002a181b8377ea95d4f56b83a06ce))

## [2.3.4](https://github.com/iPromKnight/zilean/compare/v2.3.3...v2.3.4) (2024-11-17)


### 🔥 Bug Fixes

* ensure imdbId from torznab has tt prefixed ([354ae43](https://github.com/iPromKnight/zilean/commit/354ae43a56e45285183031eb3ec7477c86888ffe))

## [2.3.3](https://github.com/iPromKnight/zilean/compare/v2.3.2...v2.3.3) (2024-11-17)


### 🔥 Bug Fixes

* include ingestion request timeout ([92426da](https://github.com/iPromKnight/zilean/commit/92426da0af85f6595e1886576615664ee525b75c))
* increase command timeout in dockerfile example ([007f514](https://github.com/iPromKnight/zilean/commit/007f514097afb386ca2d5da7ccbfda3c8f121e68))

## [2.3.2](https://github.com/iPromKnight/zilean/compare/v2.3.1...v2.3.2) (2024-11-17)


### 🔥 Bug Fixes

* Store it in ingestion too :/ ([1cac03c](https://github.com/iPromKnight/zilean/commit/1cac03c726e8f28a1e5ca1fb0f4b1127cdfd0485))

## [2.3.1](https://github.com/iPromKnight/zilean/compare/v2.3.0...v2.3.1) (2024-11-17)


### 🔥 Bug Fixes

* stop word removal ([026b1bc](https://github.com/iPromKnight/zilean/commit/026b1bc67bc95df3784b56943b357842c9582b34))

## [2.3.0](https://github.com/iPromKnight/zilean/compare/v2.2.1...v2.3.0) (2024-11-17)


### 🚀 New Features

* add blacklist feature and API key authentication ([562bd14](https://github.com/iPromKnight/zilean/commit/562bd1433aa741c13e417a1608a628ee7093a0e6))

## [2.2.1](https://github.com/iPromKnight/zilean/compare/v2.2.0...v2.2.1) (2024-11-16)


### 🔥 Bug Fixes

* add configuration tests and update ingestion scraping logic ([ae73304](https://github.com/iPromKnight/zilean/commit/ae73304a6713fec098bcc5a39b5d5b088ca68e6b))


### ⚙️ Chores

* uddate readme ([a3461a6](https://github.com/iPromKnight/zilean/commit/a3461a606e5ff6cbad90bf6b33acaefe9c7cb831))
* update readme ([d9fed86](https://github.com/iPromKnight/zilean/commit/d9fed86a5382e7a5030811f157adbf8d908dabe0))

## [2.2.0](https://github.com/iPromKnight/zilean/compare/v2.1.3...v2.2.0) (2024-11-16)


### 🚀 New Features

* completed generic ingestion with service discovery ([0db28ba](https://github.com/iPromKnight/zilean/commit/0db28bad3c60308beb1942f18884b1c816518b55))
* Implement cli to allow choice of sync process to run ([6402511](https://github.com/iPromKnight/zilean/commit/6402511c44d9fe00f53785e597956c182a77221b))
* service discovery and manual config ([b39b6a3](https://github.com/iPromKnight/zilean/commit/b39b6a36b57df5be350cdf3905492f64a93a3626))
* Streaming endpoint ([79e12e1](https://github.com/iPromKnight/zilean/commit/79e12e155eec9fb2f541dbe841689eb02e477b0c))


### ⌨️ Code Refactoring

* collection of selectors in case they dont all match ([3bce11e](https://github.com/iPromKnight/zilean/commit/3bce11ec8bedd34eae66dc6c8f381485a0fcffc5))
* rename scraper ([88af4fd](https://github.com/iPromKnight/zilean/commit/88af4fd2ab652f34b129e13b2748327892679af5))
* started before scraper expansion ([1673841](https://github.com/iPromKnight/zilean/commit/16738410743b6e689aaf03c136d60c578cbabfc3))
* wire up secondary service, with separate schedule ([dceb6cb](https://github.com/iPromKnight/zilean/commit/dceb6cba9d205eb82ed8a680e1eac3d5e73159a5))

## [2.1.3](https://github.com/iPromKnight/zilean/compare/v2.1.2...v2.1.3) (2024-11-16)


### 🔥 Bug Fixes

* also handle xx ([09c70fb](https://github.com/iPromKnight/zilean/commit/09c70fbf0ed34e4f314c9928bbce8d13a89a9176))

## [2.1.2](https://github.com/iPromKnight/zilean/compare/v2.1.1...v2.1.2) (2024-11-16)


### 🔥 Bug Fixes

* Add filtering logic to exclude certain torrents during DMM scraping ([50612a7](https://github.com/iPromKnight/zilean/commit/50612a7d1467e9b47289882b762f658f4c09c5c6))
* reverse predicate ([c083aac](https://github.com/iPromKnight/zilean/commit/c083aacb999121b9d341cce0a0ab65e0a0d2fe9a))
* this will allow maxxxine, and xxx ([1f3c5c0](https://github.com/iPromKnight/zilean/commit/1f3c5c05f9979128a5cc49c9de72990550c87aa8))

## [2.1.1](https://github.com/iPromKnight/zilean/compare/v2.1.0...v2.1.1) (2024-11-15)


### 🔥 Bug Fixes

* Ensure culture libs are included in the container for .net to use. ([4a7a3ba](https://github.com/iPromKnight/zilean/commit/4a7a3ba46ba97f181d3fb8353d8db412e7bd2f06))

## [2.1.0](https://github.com/iPromKnight/zilean/compare/v2.0.2...v2.1.0) (2024-11-15)


### 🚀 New Features

* Add Torznab API support and enhance search capabilities ([d41abaf](https://github.com/iPromKnight/zilean/commit/d41abaf105fd3efe4e6e93e3e0fd0211cce66d33))
* torznab support started ([709be25](https://github.com/iPromKnight/zilean/commit/709be255de892677fa3155decb61eb81018e3991))


### ⌨️ Code Refactoring

* Updated to .net 9 ([709be25](https://github.com/iPromKnight/zilean/commit/709be255de892677fa3155decb61eb81018e3991))

## [2.0.2](https://github.com/iPromKnight/zilean/compare/v2.0.1...v2.0.2) (2024-11-12)


### 🔥 Bug Fixes

* Ensure infohashes only 40 chars in results ([cb4ae26](https://github.com/iPromKnight/zilean/commit/cb4ae26b60a08a083175810fdbe4e2c630fed674))
