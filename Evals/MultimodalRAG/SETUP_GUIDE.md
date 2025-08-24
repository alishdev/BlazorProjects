# Quick Setup Guide for RAG System

## 🚀 Prerequisites

1. **Python 3.8+** ✅ (You have Python 3.13)
2. **Jina AI API Key** (Get from [https://jina.ai/?sui=apikey](https://jina.ai/?sui=apikey))
3. **PostgreSQL with pgvector extension**

## 🔑 Step 1: Get Your Jina AI API Key

1. Visit [https://jina.ai/?sui=apikey](https://jina.ai/?sui=apikey)
2. Sign up for an account
3. Navigate to API Keys section
4. Generate a new API key
5. Copy the key and add it to your `.env` file:

```bash
# Edit .env file
JINA_API_KEY=your_actual_api_key_here
```

**Get your Jina AI API key for free: [https://jina.ai/?sui=apikey](https://jina.ai/?sui=apikey)**

## 🗄️ Step 2: Set Up PostgreSQL with pgvector

### Option A: Using Homebrew (macOS - Recommended)

```bash
# Install PostgreSQL
brew install postgresql

# Install pgvector
brew install pgvector

# Start PostgreSQL service
brew services start postgresql

# Create database
createdb rag_db

# Connect to database and enable pgvector
psql rag_db -c "CREATE EXTENSION IF NOT EXISTS vector;"
```

### Option B: Using Docker (Cross-platform)

```bash
# Pull and run PostgreSQL with pgvector
docker run --name postgres-pgvector \
  -e POSTGRES_PASSWORD=your_password \
  -e POSTGRES_DB=rag_db \
  -p 5432:5432 \
  -d pgvector/pgvector:pg16

# Wait a few seconds for container to start, then test connection
psql -h localhost -U postgres -d rag_db -c "SELECT version();"
```

### Option C: Using Supabase (Cloud - No local setup)

1. Go to [https://supabase.com/](https://supabase.com/)
2. Create a new project
3. Get your database connection string
4. Update your `.env` file:

```bash
DATABASE_URL=postgresql://postgres:[YOUR-PASSWORD]@db.[YOUR-PROJECT-REF].supabase.co:5432/postgres
```

## 🧪 Step 3: Test Your Setup

### Test Jina AI (No database required):
```bash
python test_jina_only.py
```

### Test Full System (Requires database):
```bash
python test_rag.py
```

### Test Database Connection:
```bash
python -c "
from database import DatabaseManager
db = DatabaseManager()
print('✅ Database connection successful!')
db.close()
"
```

## 📚 Step 4: Run the RAG System

1. **Prepare your PDF documents** in a folder
2. **Run the system**:
```bash
python main.py /path/to/your/pdf/documents
```

## 🔧 Troubleshooting

### Common Issues:

1. **"Connection refused" error**:
   - PostgreSQL is not running
   - Run: `brew services start postgresql` (macOS) or start your Docker container

2. **"pgvector extension not found"**:
   - Install pgvector: `brew install pgvector` (macOS) or use Docker image

3. **"JINA_API_KEY not set"**:
   - Add your API key to `.env` file
   - Get API key from [https://jina.ai/?sui=apikey](https://jina.ai/?sui=apikey)

4. **"Permission denied"**:
   - Check database user permissions
   - Ensure database exists: `createdb rag_db`

### Quick Commands:

```bash
# Check PostgreSQL status (macOS)
brew services list | grep postgresql

# Restart PostgreSQL (macOS)
brew services restart postgresql

# Check if pgvector is installed
psql rag_db -c "SELECT * FROM pg_extension WHERE extname = 'vector';"

# Drop and recreate database (if needed)
dropdb rag_db && createdb rag_db
psql rag_db -c "CREATE EXTENSION IF NOT EXISTS vector;"
```

## 🎯 Next Steps

Once everything is working:
1. Add your Jina AI API key to `.env`
2. Set up PostgreSQL with pgvector
3. Test the system: `python test_rag.py`
4. Start using: `python main.py /path/to/pdfs`

## 📞 Need Help?

- Check the main [README.md](README.md) for detailed information
- Review the logs for specific error messages
- Ensure all prerequisites are met
- Test components individually before running the full system
- Get help with Jina AI: [https://docs.jina.ai/](https://docs.jina.ai/)
