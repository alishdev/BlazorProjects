import click
from config import load_config
from rag_builder import build_rag_systems
from question_gen import generate_and_curate_questions
from answer_gen import generate_answers
from eval_ui import launch_evaluation_ui

@click.group()
def cli():
    pass

@cli.command()
def build():
    """Build RAG systems for all providers."""
    config = load_config()
    build_rag_systems(config)

@cli.command()
@click.option('--llm', '-l', help='Specific LLM provider to use (e.g., gemini, claude, openai)')
def generate_questions(llm):
    """Generate and curate evaluation questions."""
    config = load_config()
    
    # Filter providers if specific LLM is requested
    skip_deduplication = False
    if llm:
        original_providers = config['llm_providers']
        filtered_providers = []
        
        for provider in original_providers:
            provider_name = provider['name'].lower()
            if llm.lower() in provider_name:
                filtered_providers.append(provider)
        
        if filtered_providers:
            config['llm_providers'] = filtered_providers
            skip_deduplication = True  # Skip deduplication when using -l parameter
            print(f"[INFO] Filtered to {len(filtered_providers)} provider(s) matching '{llm}':")
            for provider in filtered_providers:
                print(f"  - {provider['name']}")
        else:
            print(f"[WARN] No providers found matching '{llm}'. Available providers:")
            for provider in original_providers:
                print(f"  - {provider['name']}")
            return
    
    generate_and_curate_questions(config, skip_deduplication=skip_deduplication)

@cli.command()
@click.option('--llm', '-l', help='Specific LLM provider to use (e.g., gemini, claude, openai)')
def generate_answers_cmd(llm):
    """Generate answers for each question using all LLMs."""
    config = load_config()
    
    # Filter providers if specific LLM is requested
    if llm:
        original_providers = config['llm_providers']
        filtered_providers = []
        
        for provider in original_providers:
            provider_name = provider['name'].lower()
            if llm.lower() in provider_name:
                filtered_providers.append(provider)
        
        if filtered_providers:
            config['llm_providers'] = filtered_providers
            print(f"[INFO] Filtered to {len(filtered_providers)} provider(s) matching '{llm}':")
            for provider in filtered_providers:
                print(f"  - {provider['name']}")
        else:
            print(f"[WARN] No providers found matching '{llm}'. Available providers:")
            for provider in original_providers:
                print(f"  - {provider['name']}")
            return
    
    generate_answers(config)

@cli.command()
def eval():
    """Launch the evaluation UI."""
    config = load_config()
    launch_evaluation_ui(config)

if __name__ == '__main__':
    cli() 