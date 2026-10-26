import sqlite3

conn = sqlite3.connect("game.db")
cursor = conn.cursor()

cursor.execute("SELECT sqlite_version()")
print(cursor.fetchone())

conn.close()