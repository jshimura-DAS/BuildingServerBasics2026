# db.py
import sqlite3

DB_PATH = "game.db"

def get_connection():
	conn = sqlite3.connect(DB_PATH)
	conn.row_factory = sqlite3.Row
	return conn

def init_db():
	conn = get_connection()
	try:
		conn.execute("""
			CREATE TABLE IF NOT EXISTS scores (
				id INTEGER PRIMARY KEY AUTOINCREMENT,
				name TEXT NOT NULL,
				score INTEGER NOT NULL
			)
		""")
		conn.commit()
	finally:
		conn.close()
