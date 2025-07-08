import sqlite3
import os

def init_db(db_path):
    """Initialize SQLite database for evaluation results."""
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS evaluations (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            question_id TEXT,
            question_text TEXT,
            provider_name TEXT,
            answer_text TEXT,
            timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
        )
    ''')
    
    conn.commit()
    conn.close() 