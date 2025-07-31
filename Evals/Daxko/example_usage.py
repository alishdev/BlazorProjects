#!/usr/bin/env python3
"""
Example usage of the Daxko Member Creation Script

This script demonstrates how to use the DaxkoMemberCreator class
with custom member data.
"""

import os
from create_member import DaxkoMemberCreator

def main():
    """Example of creating a custom member."""
    
    # Configuration - Update these with your actual values
    API_KEY = os.getenv('DAXKO_API_KEY')
    MEMBERSHIP_TYPE_ID = "MT1234"  # Replace with actual membership type ID
    BRANCH_ID = "B1234"  # Replace with actual branch ID if needed
    
    # Create the member creator
    creator = DaxkoMemberCreator(api_key=API_KEY)
    
    # Custom member data
    member_data = {
        "name": {
            "first": "Jane",
            "middle": "Marie",
            "last": "Smith"
        },
        "email": "jane.smith@example.com",
        "gender": "F",
        "birth_date": "1985-06-15",
        "primary_address": {
            "line1": "456 Oak Avenue",
            "line2": "Suite 200",
            "city": "Springfield",
            "state": "IL",
            "zip": "62701",
            "country": "USA"
        },
        "primary_phone": {
            "phone": "(217) 555-0123"
        },
        "race": "White",
        "emergency_contact": {
            "first": "Robert",
            "last": "Smith"
        },
        "emergency_phone": {
            "phone": "(217) 555-0456"
        },
        "password": {
            "password": "MySecurePass456!"
        }
    }
    
    # Customer information for checkout
    customer_info = {
        "name": "Jane Smith",
        "email": "jane.smith@example.com"
    }
    
    # Payment information - Update with actual values
    payment_info = [
        {
            "payment_method_amount": 150.00,
            "apply_system_credit_amount": 0,
            "billing_method": {
                "id": "PTLvg3hhYLPUqaBJcHIybjIokxLwR6RHG4zdhF6zZQdZM",  # Replace with actual billing method ID
                "save": True
            },
            "line_item_payments": [
                {
                    "line_item_id": "4f04bd57-a51b-4d55-85ea-b6f4d7a64090",  # Replace with actual line item ID
                    "amount": 150.00,
                    "schedule_remaining": False
                }
            ]
        }
    ]
    
    try:
        print("Creating member: Jane Smith")
        print("=" * 40)
        
        # Create the member
        unit_id = creator.create_member(
            membership_type_id=MEMBERSHIP_TYPE_ID,
            member_data=member_data,
            customer_info=customer_info,
            payment_info=payment_info,
            branch_id=BRANCH_ID,
            age_group_id="any_adult"  # or "any_child" for children
        )
        
        print(f"\n✅ Success! Member created with Unit ID: {unit_id}")
        
    except Exception as e:
        print(f"\n❌ Error creating member: {str(e)}")
        print("Check the log file for detailed information.")

if __name__ == "__main__":
    main() 