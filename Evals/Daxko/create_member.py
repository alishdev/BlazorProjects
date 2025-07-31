#!/usr/bin/env python3
"""
Daxko Member Creation Script

This script creates a new member in Daxko using the following API endpoints:
1. POST /membership/join - Start the join process and get cart_id
2. POST /membership/{cart_id}/members - Add member details to cart
3. POST /membership/{cart_id}/checkout - Complete the registration

The script logs all API calls, requests, and responses to both a log file and console.
"""

import requests
import json
import logging
import sys
from datetime import datetime
from typing import Dict, Any, Optional
import os

# Configuration
BASE_URL = "https://api.partners.daxko.com/api/v1"
LOG_FILE = "daxko_member_creation.log"

# Setup logging
def setup_logging():
    """Setup logging configuration for both file and console output."""
    # Create formatter
    formatter = logging.Formatter(
        '%(asctime)s - %(levelname)s - %(message)s',
        datefmt='%Y-%m-%d %H:%M:%S'
    )
    
    # Setup file handler
    file_handler = logging.FileHandler(LOG_FILE, mode='w')
    file_handler.setLevel(logging.DEBUG)
    file_handler.setFormatter(formatter)
    
    # Setup console handler
    console_handler = logging.StreamHandler(sys.stdout)
    console_handler.setLevel(logging.INFO)
    console_handler.setFormatter(formatter)
    
    # Setup logger
    logger = logging.getLogger('DaxkoMemberCreation')
    logger.setLevel(logging.DEBUG)
    logger.addHandler(file_handler)
    logger.addHandler(console_handler)
    
    return logger

class DaxkoMemberCreator:
    """Class to handle Daxko member creation process."""
    
    def __init__(self, api_key: str = None, headers: Dict[str, str] = None):
        """
        Initialize the Daxko member creator.
        
        Args:
            api_key: API key for authentication (if required)
            headers: Additional headers to include in requests
        """
        self.base_url = BASE_URL
        self.session = requests.Session()
        
        # Set default headers
        self.session.headers.update({
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        })
        
        # Add API key if provided
        if api_key:
            self.session.headers.update({'Authorization': f'Bearer {api_key}'})
        
        # Add custom headers if provided
        if headers:
            self.session.headers.update(headers)
        
        self.logger = setup_logging()
    
    def log_request(self, method: str, url: str, data: Any = None, params: Any = None):
        """Log the request details."""
        self.logger.info(f"Making {method} request to: {url}")
        if params:
            self.logger.debug(f"Query parameters: {json.dumps(params, indent=2)}")
        if data:
            self.logger.debug(f"Request body: {json.dumps(data, indent=2)}")
    
    def log_response(self, response: requests.Response):
        """Log the response details."""
        self.logger.info(f"Response status: {response.status_code}")
        try:
            response_data = response.json()
            self.logger.debug(f"Response body: {json.dumps(response_data, indent=2)}")
        except json.JSONDecodeError:
            self.logger.debug(f"Response text: {response.text}")
    
    def make_request(self, method: str, endpoint: str, data: Any = None, params: Any = None) -> requests.Response:
        """Make an HTTP request with logging."""
        url = f"{self.base_url}{endpoint}"
        
        # Log request
        self.log_request(method, url, data, params)
        
        # Make request
        if method.upper() == 'GET':
            response = self.session.get(url, params=params)
        elif method.upper() == 'POST':
            response = self.session.post(url, json=data, params=params)
        elif method.upper() == 'PUT':
            response = self.session.put(url, json=data, params=params)
        else:
            raise ValueError(f"Unsupported HTTP method: {method}")
        
        # Log response
        self.log_response(response)
        
        return response
    
    def start_join_process(self, membership_type_id: str, branch_id: str = None, 
                          registration_type: str = "online", 
                          auto_apply_discount_groups: bool = False) -> str:
        """
        Start the join process and get cart_id.
        
        Args:
            membership_type_id: The membership type ID
            branch_id: Branch ID (required for member rate customers)
            registration_type: Type of registration ("online" or "in_house")
            auto_apply_discount_groups: Whether to auto-apply discount groups
            
        Returns:
            cart_id: The cart ID for subsequent API calls
        """
        self.logger.info("=== Starting Join Process ===")
        
        data = {
            "membership_type_id": membership_type_id,
            "registration_type": registration_type,
            "auto_apply_discount_groups": auto_apply_discount_groups
        }
        
        if branch_id:
            data["branch_id"] = branch_id
        
        response = self.make_request('POST', '/membership/join', data=data)
        
        if response.status_code == 201:
            response_data = response.json()
            cart_id = response_data.get('cart_id')
            self.logger.info(f"Successfully created cart with ID: {cart_id}")
            return cart_id
        else:
            self.logger.error(f"Failed to start join process. Status: {response.status_code}")
            response.raise_for_status()
    
    def add_member_to_cart(self, cart_id: str, member_data: Dict[str, Any], 
                          age_group_id: str = "any_adult", 
                          member_rate_id: str = None) -> str:
        """
        Add a member to the cart.
        
        Args:
            cart_id: The cart ID from start_join_process
            member_data: Member information dictionary
            age_group_id: Age group ID ("any_child", "any_adult", or specific ID)
            member_rate_id: Member rate ID (for member rate clients)
            
        Returns:
            member_guid: The member GUID
        """
        self.logger.info("=== Adding Member to Cart ===")
        
        # Build query parameters
        params = {"age_group_id": age_group_id}
        if member_rate_id:
            params["member_rate_id"] = member_rate_id
        
        response = self.make_request('POST', f'/membership/{cart_id}/members', 
                                   data=member_data, params=params)
        
        if response.status_code == 201:
            response_data = response.json()
            member_guid = response_data.get('member_guid')
            self.logger.info(f"Successfully added member with GUID: {member_guid}")
            return member_guid
        else:
            self.logger.error(f"Failed to add member to cart. Status: {response.status_code}")
            response.raise_for_status()
    
    def checkout(self, cart_id: str, version: str, customer_info: Dict[str, str], 
                payment_info: list) -> str:
        """
        Complete the registration process.
        
        Args:
            cart_id: The cart ID from start_join_process
            version: Version string from get cart API
            customer_info: Customer information (name, email)
            payment_info: Payment information array
            
        Returns:
            unit_id: The unit ID of the created unit
        """
        self.logger.info("=== Completing Checkout ===")
        
        data = {
            "version": version,
            "customer": customer_info,
            "payment_info": payment_info
        }
        
        response = self.make_request('POST', f'/membership/{cart_id}/checkout', data=data)
        
        if response.status_code == 200:
            response_data = response.json()
            unit_id = response_data.get('unit_id')
            success = response_data.get('success', False)
            
            if success:
                self.logger.info(f"Successfully completed checkout. Unit ID: {unit_id}")
                return unit_id
            else:
                errors = response_data.get('errors', [])
                self.logger.error(f"Checkout failed with errors: {errors}")
                raise Exception(f"Checkout failed: {errors}")
        else:
            self.logger.error(f"Failed to complete checkout. Status: {response.status_code}")
            response.raise_for_status()
    
    def get_cart_details(self, cart_id: str) -> Dict[str, Any]:
        """
        Get cart details including version for checkout.
        
        Args:
            cart_id: The cart ID
            
        Returns:
            Cart details including version
        """
        self.logger.info("=== Getting Cart Details ===")
        
        response = self.make_request('GET', f'/membership/{cart_id}')
        
        if response.status_code == 200:
            cart_data = response.json()
            self.logger.info("Successfully retrieved cart details")
            return cart_data
        else:
            self.logger.error(f"Failed to get cart details. Status: {response.status_code}")
            response.raise_for_status()
    
    def create_member(self, membership_type_id: str, member_data: Dict[str, Any], 
                     customer_info: Dict[str, str], payment_info: list,
                     branch_id: str = None, age_group_id: str = "any_adult",
                     member_rate_id: str = None) -> str:
        """
        Complete member creation process.
        
        Args:
            membership_type_id: The membership type ID
            member_data: Member information
            customer_info: Customer information for checkout
            payment_info: Payment information for checkout
            branch_id: Branch ID (if required)
            age_group_id: Age group ID
            member_rate_id: Member rate ID (if required)
            
        Returns:
            unit_id: The unit ID of the created unit
        """
        self.logger.info("=== Starting Complete Member Creation Process ===")
        
        try:
            # Step 1: Start join process
            cart_id = self.start_join_process(
                membership_type_id=membership_type_id,
                branch_id=branch_id
            )
            
            # Step 2: Add member to cart
            member_guid = self.add_member_to_cart(
                cart_id=cart_id,
                member_data=member_data,
                age_group_id=age_group_id,
                member_rate_id=member_rate_id
            )
            
            # Step 3: Get cart details for version
            cart_details = self.get_cart_details(cart_id)
            version = cart_details.get('version')
            
            if not version:
                raise Exception("Version not found in cart details")
            
            # Step 4: Complete checkout
            unit_id = self.checkout(
                cart_id=cart_id,
                version=version,
                customer_info=customer_info,
                payment_info=payment_info
            )
            
            self.logger.info("=== Member Creation Completed Successfully ===")
            self.logger.info(f"Final Unit ID: {unit_id}")
            
            return unit_id
            
        except Exception as e:
            self.logger.error(f"Member creation failed: {str(e)}")
            raise

def create_sample_member_data() -> Dict[str, Any]:
    """Create sample member data for testing."""
    return {
        "name": {
            "first": "John",
            "middle": "A",
            "last": "Doe"
        },
        "email": "john.doe@example.com",
        "gender": "M",
        "birth_date": "1990-01-01",
        "primary_address": {
            "line1": "123 Main Street",
            "line2": "Apt 4B",
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
        },
        "password": {
            "password": "SecurePass123!"
        }
    }

def create_sample_payment_info() -> list:
    """Create sample payment information for testing."""
    return [
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

def main():
    """Main function to demonstrate member creation."""
    print("Daxko Member Creation Script")
    print("=" * 50)
    
    # Configuration - Update these values as needed
    API_KEY = os.getenv('DAXKO_API_KEY')  # Set this environment variable
    MEMBERSHIP_TYPE_ID = "MT1234"  # Replace with actual membership type ID
    BRANCH_ID = "B1234"  # Replace with actual branch ID if needed
    
    # Create the member creator
    creator = DaxkoMemberCreator(api_key=API_KEY)
    
    # Sample data
    member_data = create_sample_member_data()
    customer_info = {
        "name": "John Doe",
        "email": "john.doe@example.com"
    }
    payment_info = create_sample_payment_info()
    
    try:
        # Create the member
        unit_id = creator.create_member(
            membership_type_id=MEMBERSHIP_TYPE_ID,
            member_data=member_data,
            customer_info=customer_info,
            payment_info=payment_info,
            branch_id=BRANCH_ID
        )
        
        print(f"\n✅ Member created successfully!")
        print(f"Unit ID: {unit_id}")
        print(f"Log file: {LOG_FILE}")
        
    except Exception as e:
        print(f"\n❌ Member creation failed: {str(e)}")
        print(f"Check log file: {LOG_FILE}")
        sys.exit(1)

if __name__ == "__main__":
    main() 