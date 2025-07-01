import os
import yaml

REQUIRED_FIELDS = [
    'source_documents_path',
    'vector_store_path',
    'output_data_path',
    'database_path',
    'llm_providers',
]


def load_config(config_path="config.yaml"):
    """Load and validate the configuration file."""
    if not os.path.exists(config_path):
        raise FileNotFoundError(f"Config file not found: {config_path}")
    with open(config_path, 'r') as f:
        config = yaml.safe_load(f)

    # Expand environment variables in API keys
    for provider in config.get('llm_providers', []):
        api_key = provider.get('api_key', '')
        if api_key.startswith('${') and api_key.endswith('}'):
            env_var = api_key[2:-1]
            provider['api_key'] = os.environ.get(env_var, '')

    # Validate required fields
    for field in REQUIRED_FIELDS:
        if field not in config or config[field] is None:
            raise ValueError(f"Missing required config field: {field}")

    return config 