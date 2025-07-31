# Daxko Member Creation Script

This Python script creates new members in Daxko using the Daxko Operations API. It follows the three-step process required by the API:

1. **Start Join Process** (`POST /membership/join`) - Creates a cart and returns a cart_id
2. **Add Member to Cart** (`POST /membership/{cart_id}/members`) - Adds member details to the cart
3. **Complete Checkout** (`POST /membership/{cart_id}/checkout`) - Finalizes the registration

## Features

- ✅ Complete logging of all API calls, requests, and responses
- ✅ Logs to both console and file (`daxko_member_creation.log`)
- ✅ Error handling and validation
- ✅ Configurable authentication and headers
- ✅ Sample data for testing
- ✅ Type hints for better code clarity

## Installation

1. Install Python dependencies:
```bash
pip install -r requirements.txt
```

2. Set up your API key (if required):
```bash
export DAXKO_API_KEY="your_api_key_here"
```

## Usage

### Basic Usage

Run the script with default sample data:
```bash
python create_member.py
```

### Custom Usage

You can also use the `DaxkoMemberCreator` class in your own code:

```python
from create_member import DaxkoMemberCreator

# Initialize the creator
creator = DaxkoMemberCreator(api_key="your_api_key")

# Create member data
member_data = {
    "name": {
        "first": "John",
        "last": "Doe"
    },
    "email": "john.doe@example.com",
    "gender": "M",
    "birth_date": "1990-01-01",
    "primary_address": {
        "line1": "123 Main Street",
        "city": "Anytown",
        "state": "CA",
        "zip": "12345",
        "country": "USA"
    },
    "primary_phone": {
        "phone": "(555) 123-4567"
    },
    "race": "White",
    "emergency_contact": {
        "first": "Jane",
        "last": "Doe"
    },
    "emergency_phone": {
        "phone": "(555) 987-6543"
    }
}

# Customer info for checkout
customer_info = {
    "name": "John Doe",
    "email": "john.doe@example.com"
}

# Payment info
payment_info = [
    {
        "payment_method_amount": 100.00,
        "apply_system_credit_amount": 0,
        "billing_method": {
            "id": "PTLvg3hhYLPUqaBJcHIybjIokxLwR6RHG4zdhF6zZQdZM",
            "save": True
        },
        "line_item_payments": [
            {
                "line_item_id": "4f04bd57-a51b-4d55-85ea-b6f4d7a64090",
                "amount": 100.00,
                "schedule_remaining": False
            }
        ]
    }
]

# Create the member
unit_id = creator.create_member(
    membership_type_id="MT1234",
    member_data=member_data,
    customer_info=customer_info,
    payment_info=payment_info,
    branch_id="B1234"
)

print(f"Member created with Unit ID: {unit_id}")
```

## Configuration

### Required Parameters

- **membership_type_id**: The ID of the membership type (e.g., "MT1234")
- **member_data**: Complete member information including name, address, contact info
- **customer_info**: Customer name and email for checkout
- **payment_info**: Payment method and line item payment details

### Optional Parameters

- **branch_id**: Branch ID (required for member rate customers)
- **age_group_id**: Age group ("any_child", "any_adult", or specific ID)
- **member_rate_id**: Member rate ID (for member rate clients)
- **api_key**: API key for authentication

## API Endpoints Used

1. **POST /membership/join**
   - Starts the join process
   - Returns cart_id for subsequent calls
   - Required: membership_type_id
   - Optional: branch_id, registration_type, auto_apply_discount_groups

2. **POST /membership/{cart_id}/members**
   - Adds member to cart
   - Required: cart_id, member_data, age_group_id
   - Optional: member_rate_id

3. **GET /membership/{cart_id}**
   - Gets cart details including version for checkout
   - Required: cart_id

4. **POST /membership/{cart_id}/checkout**
   - Completes the registration
   - Required: cart_id, version, customer_info, payment_info

## Logging

The script provides comprehensive logging:

- **Console Output**: INFO level messages showing progress
- **Log File**: DEBUG level messages with full request/response details
- **Log File Location**: `daxko_member_creation.log`

### Log Format
```
2024-01-15 10:30:45 - INFO - === Starting Join Process ===
2024-01-15 10:30:45 - INFO - Making POST request to: https://api.partners.daxko.com/api/v1/membership/join
2024-01-15 10:30:46 - DEBUG - Request body: {"membership_type_id": "MT1234", ...}
2024-01-15 10:30:47 - INFO - Response status: 201
2024-01-15 10:30:47 - DEBUG - Response body: {"cart_id": "db2c4395-888c-42ce-9056-08e2f8a5f2d0", ...}
```

## Error Handling

The script includes comprehensive error handling:

- HTTP status code validation
- JSON response parsing
- API-specific error messages
- Graceful failure with detailed logging

## Sample Data

The script includes sample data functions:

- `create_sample_member_data()`: Creates sample member information
- `create_sample_payment_info()`: Creates sample payment information

## Security Notes

- API keys should be stored as environment variables, not in code
- Log files may contain sensitive information - ensure proper access controls
- Use HTTPS for all API communications (already configured)

## Troubleshooting

### Common Issues

1. **Authentication Errors**
   - Verify your API key is correct
   - Check if additional headers are required

2. **Validation Errors**
   - Ensure all required fields are provided
   - Check data format (dates, phone numbers, etc.)

3. **Payment Errors**
   - Verify billing method ID is valid
   - Ensure payment amounts match line item totals

### Debug Mode

For detailed debugging, the script logs all request/response details to the log file. Check `daxko_member_creation.log` for complete API interaction details.

## API Documentation

For complete API documentation, refer to the Daxko Operations API specification in `openapi.json`.

## License

This script is provided as-is for educational and development purposes. Please ensure compliance with Daxko's API terms of service. 