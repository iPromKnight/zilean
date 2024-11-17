# Changelog

## [2.4.0](https://github.com/iPromKnight/zilean/compare/v2.3.0...v2.4.0) (2024-11-17)


### 🚀 New Features

* Add batched processing option for DMM scraper and update Docker configuration ([c97d1b0](https://github.com/iPromKnight/zilean/commit/c97d1b0f86a5b50a5f6d6ce090ca4deca3793963))
* add blacklist feature and API key authentication ([5a3d426](https://github.com/iPromKnight/zilean/commit/5a3d4261a2407480656caee66a870e231dc1a9ba))
* Add Torznab API support and enhance search capabilities ([f67f684](https://github.com/iPromKnight/zilean/commit/f67f684c24240b61032ce1d2784bf0925a523cec))
* completed generic ingestion with service discovery ([1b79a2b](https://github.com/iPromKnight/zilean/commit/1b79a2b65ba7673bbca110cc1e3e98b547504492))
* Enhance DMM and IMDB import processes with metadata tracking and configuration updates ([50c7246](https://github.com/iPromKnight/zilean/commit/50c72465e13629751c996af5454ed3a60720e0fa))
* Implement cli to allow choice of sync process to run ([898435d](https://github.com/iPromKnight/zilean/commit/898435ddbed92046ff4871f99dafcb739710a76f))
* Integrate PostgreSQL for data storage and search, replace ElasticSearch ([3f6fe92](https://github.com/iPromKnight/zilean/commit/3f6fe92b9de5c67b5a437d59df7e6c1ad4c3a0f5))
* Rework ingestion ([e9171cd](https://github.com/iPromKnight/zilean/commit/e9171cd52ff0810c5e1cfa5c4ce0155065efc09c))
* service discovery and manual config ([2ba73b1](https://github.com/iPromKnight/zilean/commit/2ba73b106bf257ab9a8a521a58d14f8d1a2f44cb))
* Streaming endpoint ([ca3e0ea](https://github.com/iPromKnight/zilean/commit/ca3e0eabfcb1b95b1c6b8fa61be1a18ca10f1b0d))
* torznab support started ([f3dc9bf](https://github.com/iPromKnight/zilean/commit/f3dc9bf1a911954fb762bd7643deb64b4e5c9c47))


### 🔥 Bug Fixes

* add configuration tests and update ingestion scraping logic ([a830d01](https://github.com/iPromKnight/zilean/commit/a830d01de17c263167c9d7d1db6d6fade4d1f0af))
* Add filtering logic to exclude certain torrents during DMM scraping ([cbc46f0](https://github.com/iPromKnight/zilean/commit/cbc46f0a7ba3040471447ef90d496cc05ad3c8be))
* Add missing parameters for category and year in TorrentInfoService query ([d32dce6](https://github.com/iPromKnight/zilean/commit/d32dce656f6fa5447f6aecb171d96139f92346a9))
* also handle xx ([0ce2cb8](https://github.com/iPromKnight/zilean/commit/0ce2cb8a21947a06b44a99d1078fa33840ddad1b))
* Correct order of SQL commands in migration script ([7cfb090](https://github.com/iPromKnight/zilean/commit/7cfb090c4548a96d2436f6e111705f4d2ec1004e))
* **curl:** Fix curl version ([54fddb8](https://github.com/iPromKnight/zilean/commit/54fddb88f7c86f8fdd0d6dd6b8bd1bf3314b9ef3))
* **eng:** Updated Python version for pip install in benchmark setup script from 3 to 3.11 ([e53a6d9](https://github.com/iPromKnight/zilean/commit/e53a6d9c4c9ddc7ca5750b2594ac7611eb8ab6f7))
* Ensure culture libs are included in the container for .net to use. ([ab46324](https://github.com/iPromKnight/zilean/commit/ab463242484b0407c9f0abd31b3bf48cf8e798e0))
* Ensure infohashes only 40 chars in results ([640497f](https://github.com/iPromKnight/zilean/commit/640497f761184c4f61183f97eb30bec777cb3d2a))
* handle v2 hashlists ([65b66d4](https://github.com/iPromKnight/zilean/commit/65b66d4bb26e55898ba041a6fb82e76412c59481))
* Refine IMDB search conditions and update logging messages ([1fa6d64](https://github.com/iPromKnight/zilean/commit/1fa6d64553a7f6d5cc3c64f0c37028fbcdf4c813))
* reverse predicate ([fa585ff](https://github.com/iPromKnight/zilean/commit/fa585ff6e1629d873d6e2467bb74496f72950dff))
* this will allow maxxxine, and xxx ([0cc6c2f](https://github.com/iPromKnight/zilean/commit/0cc6c2f80549835256c881e9bbc83f78fb4b0831))
* torrent parsing v2 ([77d170f](https://github.com/iPromKnight/zilean/commit/77d170f7242500ef73bb335f93c3b3471b7116df))


### ⚙️ Chores

* **main:** release 2.0.2 ([f199cc1](https://github.com/iPromKnight/zilean/commit/f199cc15d46c1b88ca5848955d71c2fd3c2afc34))
* **main:** release 2.1.0 ([b7342b4](https://github.com/iPromKnight/zilean/commit/b7342b49babde7d99d0587de65372b7d78ee28ef))
* **main:** release 2.1.1 ([5658ace](https://github.com/iPromKnight/zilean/commit/5658ace3554261be606314e072e632a4a43b58c8))
* **main:** release 2.1.2 ([59686a7](https://github.com/iPromKnight/zilean/commit/59686a798bb3685f458bc9cf623a49b07fb8147c))
* **main:** release 2.1.3 ([fb9a7f9](https://github.com/iPromKnight/zilean/commit/fb9a7f93823bbf33a70566ffc1fb1f7712b60fea))
* **main:** release 2.2.0 ([0eb73ca](https://github.com/iPromKnight/zilean/commit/0eb73ca6aef2676d12f30f80ee751fc1930e79f9))
* **main:** release 2.2.1 ([619c106](https://github.com/iPromKnight/zilean/commit/619c10600c90691a343de50454b15be508c2a6a2))
* **main:** release 2.3.0 ([273d820](https://github.com/iPromKnight/zilean/commit/273d82018a08bb7e6d078ae610db95b1c4f5fcea))
* uddate readme ([16ec154](https://github.com/iPromKnight/zilean/commit/16ec154a4f0acd0ace29bd1e984b9bf7af2ebec0))
* update readme ([6b0a0e6](https://github.com/iPromKnight/zilean/commit/6b0a0e65fa34df4b9124e26a9c19d9cedf7baa6b))


### ⌨️ Code Refactoring

* collection of selectors in case they dont all match ([2f6854d](https://github.com/iPromKnight/zilean/commit/2f6854d2f97904e2b49fc2adb8de7f2a0b77883e))
* Optimize DMM file processing and simplify database command execution ([20de564](https://github.com/iPromKnight/zilean/commit/20de56478dc49d9ca08abdb291155fb0e9010bfb))
* Remove hardcoded limit on file parsing in DmmScraping ([77816e2](https://github.com/iPromKnight/zilean/commit/77816e214156aa7b8222be3a09c145cbfa9a1399))
* rename scraper ([15bc2cf](https://github.com/iPromKnight/zilean/commit/15bc2cf468e3f6ef2d92154e66ced1408e62522f))
* Replace List properties with array properties in ZileanDbContextModelSnapshot ([2f588bd](https://github.com/iPromKnight/zilean/commit/2f588bde8cbbfd50c0e555b86fc151036f0c0f62))
* Simplify TorrentInfoService search methods and update DmmEndpoints ([b78509a](https://github.com/iPromKnight/zilean/commit/b78509adda6638dfbf3509b66ce888571149a46f))
* started before scraper expansion ([8f2e22c](https://github.com/iPromKnight/zilean/commit/8f2e22c0a030eca461946d8cef5d1682e6b10d42))
* Updated to .net 9 ([f3dc9bf](https://github.com/iPromKnight/zilean/commit/f3dc9bf1a911954fb762bd7643deb64b4e5c9c47))
* wire up secondary service, with separate schedule ([4407a10](https://github.com/iPromKnight/zilean/commit/4407a10aa7422599105b07bc09286eedb08318e7))

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
