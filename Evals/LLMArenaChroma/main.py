import click
import yaml
from config import load_config
from rag_builder import build_rag_systems
from question_gen import generate_and_curate_questions
from answer_gen import generate_answers
from db import init_db

@click.group()
def cli():
    """LLM Arena - ChromaDB-based RAG evaluation system."""
    pass

@cli.command()
@click.option('--config', default='config.yaml', help='Configuration file path')
def build(config):
    """Build RAG systems for all configured LLM providers."""
    try:
        config_data = load_config(config)
        print("[INFO] Building RAG systems...")
        build_rag_systems(config_data)
        print("[INFO] RAG systems built successfully!")
    except Exception as e:
        print(f"[ERROR] Failed to build RAG systems: {e}")

@cli.command()
@click.option('--config', default='config.yaml', help='Configuration file path')
@click.option('--skip-deduplication', is_flag=True, help='Skip deduplication step')
def questions(config, skip_deduplication):
    """Generate evaluation questions."""
    try:
        config_data = load_config(config)
        print("[INFO] Generating questions...")
        generate_and_curate_questions(config_data, skip_deduplication)
        print("[INFO] Questions generated successfully!")
    except Exception as e:
        print(f"[ERROR] Failed to generate questions: {e}")

@cli.command()
@click.option('--config', default='config.yaml', help='Configuration file path')
def answers(config):
    """Generate answers for all questions."""
    try:
        config_data = load_config(config)
        print("[INFO] Generating answers...")
        generate_answers(config_data)
        print("[INFO] Answers generated successfully!")
    except Exception as e:
        print(f"[ERROR] Failed to generate answers: {e}")

@cli.command()
@click.option('--config', default='config.yaml', help='Configuration file path')
def init(config):
    """Initialize the system (create directories, database, etc.)."""
    try:
        config_data = load_config(config)
        print("[INFO] Initializing system...")
        
        # Initialize database
        db_path = config_data.get('database_path', './output/evaluation_results.db')
        init_db(db_path)
        print(f"[INFO] Database initialized: {db_path}")
        
        print("[INFO] System initialized successfully!")
    except Exception as e:
        print(f"[ERROR] Failed to initialize system: {e}")

@cli.command()
@click.option('--config', default='config.yaml', help='Configuration file path')
def run_all(config):
    """Run the complete evaluation pipeline."""
    try:
        config_data = load_config(config)
        print("[INFO] Starting complete evaluation pipeline...")
        
        # Step 1: Build RAG systems
        print("\n[STEP 1] Building RAG systems...")
        build_rag_systems(config_data)
        
        # Step 2: Generate questions
        print("\n[STEP 2] Generating questions...")
        generate_and_curate_questions(config_data)
        
        # Step 3: Generate answers
        print("\n[STEP 3] Generating answers...")
        generate_answers(config_data)
        
        print("\n[INFO] Complete evaluation pipeline finished successfully!")
    except Exception as e:
        print(f"[ERROR] Pipeline failed: {e}")

if __name__ == '__main__':
    cli() 