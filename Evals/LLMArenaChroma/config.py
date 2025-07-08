import yaml
import os

def load_config(config_path="config.yaml"):
    """Load configuration from YAML file."""
    if not os.path.exists(config_path):
        raise FileNotFoundError(f"Configuration file not found: {config_path}")
    
    with open(config_path, 'r', encoding='utf-8') as f:
        config = yaml.safe_load(f)
    
    # Validate required fields
    required_fields = ['source_documents_path', 'vector_store_path', 'output_data_path', 'llm_providers']
    for field in required_fields:
        if field not in config:
            raise ValueError(f"Missing required configuration field: {field}")
    
    # Ensure paths exist
    for path_field in ['source_documents_path', 'vector_store_path', 'output_data_path']:
        path = config[path_field]
        if not os.path.exists(path):
            os.makedirs(path, exist_ok=True)
            print(f"[INFO] Created directory: {path}")
    
    return config

def validate_provider_config(provider):
    """Validate individual provider configuration."""
    required_fields = ['name', 'api_key', 'embedding_model', 'generation_model']
    for field in required_fields:
        if field not in provider:
            raise ValueError(f"Provider {provider.get('name', 'unknown')} missing required field: {field}")
    
    # Special handling for Claude which might have separate embedding API key
    if provider['name'].startswith('claude') and 'embedding_api_key' not in provider:
        print(f"[WARN] Claude provider {provider['name']} missing embedding_api_key, using api_key for embeddings")

if __name__ == "__main__":
    # Test configuration loading
    try:
        config = load_config()
        print("[INFO] Configuration loaded successfully")
        print(f"[INFO] Found {len(config['llm_providers'])} LLM providers")
        for provider in config['llm_providers']:
            validate_provider_config(provider)
            print(f"[INFO] Provider {provider['name']} configuration valid")
    except Exception as e:
        print(f"[ERROR] Configuration validation failed: {e}") 