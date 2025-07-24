import json
import os

def list_get_endpoints_without_parameters(openapi_file_path):
    """
    Read OpenAPI JSON file and list all GET endpoints that don't require parameters.
    
    Args:
        openapi_file_path (str): Path to the openapi.json file
    
    Returns:
        list: List of tuples containing (summary, full_url)
    """
    
    # Base URL from the OpenAPI spec
    base_url = "https://demo-api.partners.daxko.com/api/v1"
    
    try:
        with open(openapi_file_path, 'r', encoding='utf-8') as f:
            openapi_data = json.load(f)
    except FileNotFoundError:
        print(f"Error: File '{openapi_file_path}' not found.")
        return []
    except json.JSONDecodeError as e:
        print(f"Error: Invalid JSON in '{openapi_file_path}': {e}")
        return []
    
    endpoints_without_params = []
    
    # Iterate through all paths
    for path, path_data in openapi_data.get('paths', {}).items():
        # Check if this path has a GET operation
        if 'get' in path_data:
            get_operation = path_data['get']
            
            # Check if the GET operation has parameters
            parameters = get_operation.get('parameters', [])
            
            # If no parameters, this is what we're looking for
            if not parameters:
                summary = get_operation.get('summary', 'No summary available')
                full_url = f"{base_url}{path}"
                endpoints_without_params.append((summary, full_url))
    
    return endpoints_without_params

def main():
    """
    Main function to execute the script
    """
    # Path to the openapi.json file
    openapi_file = "openapi.json"
    
    print("Analyzing OpenAPI specification for GET endpoints without parameters...")
    print("=" * 80)
    
    # Get the endpoints
    endpoints = list_get_endpoints_without_parameters(openapi_file)
    
    if not endpoints:
        print("No GET endpoints found without parameters.")
        return
    
    print(f"Found {len(endpoints)} GET endpoints without parameters:\n")
    
    # Print each endpoint in the requested C# format
    for summary, url in endpoints:
        # Clean up the summary text for use as Text property
        text = summary.replace('"', '\\"')  # Escape quotes
        print(f'new Endpoint() {{ ID= "{url}", Text= "{text}" }},')
    
    print(f"\nTotal: {len(endpoints)} endpoints")

if __name__ == "__main__":
    main() 