import sqlite3
conn = sqlite3.connect('sims.db')
for row in conn.execute("SELECT sql FROM sqlite_master WHERE sql IS NOT NULL"):
    print(row[0])
