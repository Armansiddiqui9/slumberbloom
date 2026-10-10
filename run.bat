cd /d %~dp0server
python -m pip install -q -r requirements.txt && python ..\scripts\build_site.py && python -m uvicorn main:app --port 8000 --reload
