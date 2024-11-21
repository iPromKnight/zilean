#!/bin/bash
rm -rf ../src/Zilean.Scraper/python
mkdir -p ../src/Zilean.Scraper/python
rm -rf ../src/Zilean.ApiService/python
mkdir -p ../src/Zilean.ApiService/python
python3.11 -m pip install -r ../requirements.txt -t ../src/Zilean.Scraper/python/
python3.11 -m pip install -r ../requirements.txt -t ../src/Zilean.ApiService/python/