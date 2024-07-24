#!/bin/bash

rm -rf ../src/Zilean.Benchmarks/python
mkdir -p ../src/Zilean.Benchmarks/python
python3.11 -m pip install -r ../requirements.txt -t ../src/Zilean.Benchmarks/python/