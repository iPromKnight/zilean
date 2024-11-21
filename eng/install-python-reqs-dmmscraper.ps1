Remove-Item -Path ../src/Zilean.Scraper/python -Recurse -Force
New-Item -Path ../src/Zilean.Scraper/python -ItemType Directory
Remove-Item -Path ../src/Zilean.ApiService/python -Recurse -Force
New-Item -Path ../src/Zilean.ApiService/python -ItemType Directory
python -m pip install -r ../requirements.txt -t ../src/Zilean.Scraper/python/ --no-user
python -m pip install -r ../requirements.txt -t ../src/Zilean.ApiService/python/ --no-user