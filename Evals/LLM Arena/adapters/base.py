import signal
import time

class LLMAdapter:
    def __init__(self, config):
        self.config = config
        self.timeout = 30  # 30 second timeout

    def embed(self, text):
        raise NotImplementedError

    def generate(self, prompt, context=None, system_prompt=None):
        raise NotImplementedError
    
    def _timeout_handler(self, signum, frame):
        raise TimeoutError("API call timed out")
    
    def _call_with_timeout(self, func, *args, **kwargs):
        """Call a function with timeout handling."""
        try:
            # Set up timeout handler
            signal.signal(signal.SIGALRM, self._timeout_handler)
            signal.alarm(self.timeout)
            
            # Make the call
            result = func(*args, **kwargs)
            
            # Clear the alarm
            signal.alarm(0)
            return result
            
        except TimeoutError:
            print(f"[ERROR] API call timed out after {self.timeout} seconds")
            return None
        except Exception as e:
            signal.alarm(0)  # Clear alarm on other exceptions 