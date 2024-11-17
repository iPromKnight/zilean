Remove-Item -Path ../src/Zilean.DmmScraper/python -Recurse -Force
New-Item -Path ../src/Zilean.DmmScraper/python -ItemType Directory
python -m pip install -r ../requirements.txt -t ../src/Zilean.DmmScraper/python/ --no-user