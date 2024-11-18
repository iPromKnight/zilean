Remove-Item -Path ../src/Zilean.Scraper/python -Recurse -Force
New-Item -Path ../src/Zilean.Scraper/python -ItemType Directory
python -m pip install -r ../requirements.txt -t ../src/Zilean.Scraper/python/ --no-user