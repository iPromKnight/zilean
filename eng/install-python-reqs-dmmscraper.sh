#!/bin/bash
rm -rf ../src/Zilean.DmmScraper/python
mkdir -p ../src/Zilean.DmmScraper/python
python3.11 -m pip install -r ../requirements.txt -t ../src/Zilean.DmmScraper/python/