#!/usr/bin/env bash
cd "$(dirname "$0")/server" && python3 -m pip install -q -r requirements.txt && python3 ../scripts/build_site.py && python3 -m uvicorn main:app --port 8000 --reload
