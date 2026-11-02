#!/usr/bin/env python3
"""Merge the USDA FoodKeeper dataset into the app's shelf_life.json.

Usage:
  1. Download foodkeeper.json (English) from https://catalog.data.gov/dataset/fsis-foodkeeper-data
  2. python3 Scripts/import_foodkeeper.py path/to/foodkeeper.json Readiness/Library/Resources/shelf_life.json

FoodKeeper is public domain (CC0). Rows become entries keyed "fk_<ID>" and are appended after the
hand-curated entries, which keep priority in search. Re-running replaces earlier fk_ entries.
The FoodKeeper JSON is a list of "sheets"; each row is a list of single-key dicts, which this script flattens.
"""
import json
import sys

UNIT_DAYS = {"days": 1, "day": 1, "weeks": 7, "week": 7, "months": 30, "month": 30, "years": 365, "year": 365}

CATEGORY_MAP = {
    "baby food": "baby", "dairy": "food", "produce": "food", "meat": "food", "poultry": "food",
    "seafood": "food", "beverages": "food", "condiments": "food", "grains": "food", "shelf stable": "food",
    "deli": "food", "eggs": "food", "vegetarian": "food",
}


def flatten(row):
    out = {}
    for cell in row:
        if isinstance(cell, dict):
            out.update(cell)
    return out


def days(value, metric):
    if value in (None, "", 0):
        return None
    try:
        v = float(value)
    except (TypeError, ValueError):
        return None
    mult = UNIT_DAYS.get(str(metric or "").strip().lower())
    if not mult:
        return None
    return int(round(v * mult))


def main(src, dst):
    data = json.load(open(src, encoding="utf-8"))
    sheets = {s.get("name"): s.get("data", []) for s in data.get("sheets", [])}
    categories = {}
    for row in sheets.get("Category", []):
        r = flatten(row)
        categories[r.get("ID")] = (r.get("Category_Name") or "").strip()

    entries = []
    for row in sheets.get("Product", []):
        r = flatten(row)
        name = (r.get("Name") or "").strip()
        if not name:
            continue
        subtitle = (r.get("Name_subtitle") or "").strip()
        full = f"{name}, {subtitle}" if subtitle else name
        cat_name = categories.get(r.get("Category_ID"), "").lower()
        category = CATEGORY_MAP.get(cat_name, "food")
        unopened = days(r.get("Pantry_Max"), r.get("Pantry_Metric")) or days(r.get("DOP_Pantry_Max"), r.get("DOP_Pantry_Metric"))
        opened = days(r.get("Pantry_After_Opening_Max"), r.get("Pantry_After_Opening_Metric"))
        fridge_opened = days(r.get("Refrigerate_After_Opening_Max"), r.get("Refrigerate_After_Opening_Metric")) \
            or days(r.get("Refrigerate_Max"), r.get("Refrigerate_Metric"))
        if not any([unopened, opened, fridge_opened]):
            continue
        keywords = [k.strip().lower() for k in (r.get("Keywords") or "").split(",") if k.strip()]
        keywords = list(dict.fromkeys(keywords + [name.lower()]))
        tips = (r.get("Pantry_tips") or "").strip() or None
        entries.append({
            "key": f"fk_{r.get('ID')}",
            "name": full,
            "category": category,
            "keywords": keywords,
            "unopenedDays": unopened,
            "openedDays": opened,
            "openedRefrigeratedDays": fridge_opened,
            "note": tips,
            "source": "USDA FoodKeeper",
            "sourceURL": "https://www.foodsafety.gov/keep-food-safe/foodkeeper-app",
        })

    target = json.load(open(dst, encoding="utf-8"))
    kept = [e for e in target["entries"] if not e["key"].startswith("fk_")]
    target["entries"] = kept + entries
    json.dump(target, open(dst, "w", encoding="utf-8"), indent=1, ensure_ascii=False)
    print(f"merged {len(entries)} FoodKeeper entries; total {len(target['entries'])}")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        print(__doc__)
        sys.exit(1)
    main(sys.argv[1], sys.argv[2])
