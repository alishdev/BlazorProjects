import sqlite3
import datetime
from typing import List, Tuple, Optional

class DatabaseManager:
    def __init__(self, db_path: str = "assistant_storage.db"):
        self.db_path = db_path
        self.init_database()
    
    def init_database(self):
        """Initialize the database with required tables"""
        with sqlite3.connect(self.db_path) as conn:
            cursor = conn.cursor()
            
            # Create projects table
            cursor.execute('''
                CREATE TABLE IF NOT EXISTS projects (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL UNIQUE,
                    created_ts DATETIME NOT NULL,
                    last_accessed_ts DATETIME NOT NULL
                )
            ''')
            
            # Create llm_a_history table
            cursor.execute('''
                CREATE TABLE IF NOT EXISTS llm_a_history (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    project_id INTEGER NOT NULL,
                    timestamp DATETIME NOT NULL,
                    role TEXT NOT NULL,
                    content TEXT NOT NULL,
                    FOREIGN KEY (project_id) REFERENCES projects (id)
                )
            ''')
            
            # Create llm_b_history table
            cursor.execute('''
                CREATE TABLE IF NOT EXISTS llm_b_history (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    project_id INTEGER NOT NULL,
                    timestamp DATETIME NOT NULL,
                    role TEXT NOT NULL,
                    content TEXT NOT NULL,
                    FOREIGN KEY (project_id) REFERENCES projects (id)
                )
            ''')
            
            conn.commit()
            
            # Create default "General" project if no projects exist
            cursor.execute("SELECT COUNT(*) FROM projects")
            if cursor.fetchone()[0] == 0:
                now = datetime.datetime.now().isoformat()
                cursor.execute('''
                    INSERT INTO projects (name, created_ts, last_accessed_ts)
                    VALUES (?, ?, ?)
                ''', ("General", now, now))
                conn.commit()
    
    def get_all_projects(self) -> List[Tuple[int, str, str, str]]:
        """Get all projects ordered by last accessed timestamp"""
        with sqlite3.connect(self.db_path) as conn:
            cursor = conn.cursor()
            cursor.execute('''
                SELECT id, name, created_ts, last_accessed_ts 
                FROM projects 
                ORDER BY last_accessed_ts DESC
            ''')
            return cursor.fetchall()
    
    def get_project_by_name(self, name: str) -> Optional[Tuple[int, str, str, str]]:
        """Get project by name"""
        with sqlite3.connect(self.db_path) as conn:
            cursor = conn.cursor()
            cursor.execute('''
                SELECT id, name, created_ts, last_accessed_ts 
                FROM projects 
                WHERE name = ?
            ''', (name,))
            return cursor.fetchone()
    
    def create_project(self, name: str) -> int:
        """Create a new project and return its ID"""
        with sqlite3.connect(self.db_path) as conn:
            cursor = conn.cursor()
            now = datetime.datetime.now().isoformat()
            cursor.execute('''
                INSERT INTO projects (name, created_ts, last_accessed_ts)
                VALUES (?, ?, ?)
            ''', (name, now, now))
            conn.commit()
            return cursor.lastrowid
    
    def update_project_access(self, project_id: int):
        """Update the last accessed timestamp for a project"""
        with sqlite3.connect(self.db_path) as conn:
            cursor = conn.cursor()
            now = datetime.datetime.now().isoformat()
            cursor.execute('''
                UPDATE projects 
                SET last_accessed_ts = ? 
                WHERE id = ?
            ''', (now, project_id))
            conn.commit()
    
    def get_project_history(self, project_id: int, llm_type: str) -> List[Tuple[str, str, str]]:
        """Get conversation history for a specific project and LLM"""
        table_name = f"llm_{llm_type.lower()}_history"
        with sqlite3.connect(self.db_path) as conn:
            cursor = conn.cursor()
            cursor.execute(f'''
                SELECT timestamp, role, content 
                FROM {table_name} 
                WHERE project_id = ? 
                ORDER BY timestamp ASC
            ''', (project_id,))
            return cursor.fetchall()
    
    def save_message(self, project_id: int, llm_type: str, role: str, content: str):
        """Save a message to the appropriate history table"""
        table_name = f"llm_{llm_type.lower()}_history"
        with sqlite3.connect(self.db_path) as conn:
            cursor = conn.cursor()
            timestamp = datetime.datetime.now().isoformat()
            cursor.execute(f'''
                INSERT INTO {table_name} (project_id, timestamp, role, content)
                VALUES (?, ?, ?, ?)
            ''', (project_id, timestamp, role, content))
            conn.commit()
    
    def get_project_context(self, project_id: int, llm_type: str) -> List[dict]:
        """Get conversation context for LLM API calls"""
        history = self.get_project_history(project_id, llm_type)
        context = []
        for timestamp, role, content in history:
            context.append({
                "role": role,
                "content": content
            })
        return context 